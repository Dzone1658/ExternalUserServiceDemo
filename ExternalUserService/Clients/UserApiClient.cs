using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Json;
using ExternalUserService.Configuration;
using ExternalUserService.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace ExternalUserService.Clients
{
    public class UserApiClient : IUserApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UserApiClient> _logger;
        private readonly IMemoryCache _cache;
        private readonly AsyncRetryPolicy _retryPolicy;

        public UserApiClient(HttpClient httpClient, IOptions<ApiSettings> options, ILogger<UserApiClient> logger, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
            _logger = logger;
            _cache = cache;

            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to {exception.Message}");
                    });
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var cacheKey = $"user_{id}";
            if (_cache.TryGetValue(cacheKey, out User user))
                return user;

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await _httpClient.GetAsync($"users/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to fetch user with ID {id}. StatusCode: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadFromJsonAsync<ApiUserResponse>();
                var fetchedUser = content?.Data;

                if (fetchedUser != null)
                    _cache.Set(cacheKey, fetchedUser, TimeSpan.FromMinutes(5));

                return fetchedUser;
            });
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var cacheKey = "all_users";
            if (_cache.TryGetValue(cacheKey, out List<User> users))
                return users;

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var result = new List<User>();
                int page = 1;
                bool hasMore;
                do
                {
                    var response = await _httpClient.GetFromJsonAsync<UserResponse>($"users?page={page}");
                    if (response?.Data == null) break;

                    result.AddRange(response.Data);
                    page++;
                    hasMore = page <= response.TotalPages;
                } while (hasMore);

                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
                return result;
            });
        }

        private class ApiUserResponse
        {
            public User? Data { get; set; }
        }
    }
}