using ApiTestBloxx.Application.Bloxx;
using ApiTestBloxx.Infrastructure.ApiClients;
using ApiTestBloxx.Infrastructure.ApiClients.ClientProfile;
using ApiTestBloxx.Infrastructure.Authentication;
using ApiTestBloxx.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ApiTestBloxx.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BloxxApiOptions>()
            .Bind(configuration.GetSection(BloxxApiOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Scoped = one token per Blazor circuit; typed clients take it from the caller's scope.
        services.AddScoped<IBloxxTokenStore, BloxxTokenStore>();
        services.AddScoped<IBloxxRequestLog, BloxxRequestLog>();

        services.AddHttpClient<IClientProfileClient, ClientProfileClient>(ConfigureBloxxClient)
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler { AllowAutoRedirect = false });

        return services;
    }

    private static void ConfigureBloxxClient(IServiceProvider serviceProvider, HttpClient client)
    {
        var options = serviceProvider.GetRequiredService<IOptions<BloxxApiOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/", UriKind.Absolute);
        client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    }
}
