using ExternalUserService.Clients;
using ExternalUserService.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("appsettings.json", optional: false);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<ApiSettings>(context.Configuration.GetSection("ApiSettings"));
        services.AddMemoryCache();
        services.AddHttpClient<IUserApiClient, UserApiClient>();
        services.AddLogging();
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var client = host.Services.GetRequiredService<IUserApiClient>();

var allUsers = await client.GetAllUsersAsync();
logger.LogInformation($"Fetched {allUsers.Count} users.");

var user = await client.GetUserByIdAsync(2);
logger.LogInformation($"User 2: {user?.FirstName} {user?.LastName} - {user?.Email}");