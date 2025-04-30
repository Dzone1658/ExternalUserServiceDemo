using ExternalUserService.Clients;
using ExternalUserService.Configuration;
using ExternalUserService.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Xunit;

namespace ExternalUserService.Tests;

public class UserApiClientTests
{
    private UserApiClient CreateClient(HttpResponseMessage responseMessage)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            var httpClient = new HttpClient(handlerMock.Object);

            var options = Options.Create(new ApiSettings { BaseUrl = "https://reqres.in/api/" });
            var logger = new Mock<ILogger<UserApiClient>>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            return new UserApiClient(httpClient, options, logger.Object, memoryCache);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsUser_WhenSuccess()
        {
            var user = new { data = new User { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" } };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(user), System.Text.Encoding.UTF8, "application/json")
            };
            var client = CreateClient(response);

            var result = await client.GetUserByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("John", result.FirstName);
        }

        [Fact]
        public async Task GetUserByIdAsync_ReturnsNull_WhenNotFound()
        {
            var response = new HttpResponseMessage(HttpStatusCode.NotFound);
            var client = CreateClient(response);

            var result = await client.GetUserByIdAsync(999);

            Assert.Null(result);
        }
}