using System.Net;
using System.Text;
using ApiTestBloxx.Application.Bloxx;
using ApiTestBloxx.Infrastructure;
using ApiTestBloxx.Infrastructure.ApiClients.ClientProfile;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTestBloxx.Infrastructure.IntegrationTests;

public sealed class ClientProfileClientTests
{
    [Fact]
    public async Task CreateRiskProfile_SendsTokenAndQuery_AndReadsHeaders()
    {
        var handler = new StubHandler(_ =>
        {
            var response = Json(HttpStatusCode.OK, """{"id":"rp-1","status":"NEW","questionGroups":[]}""");
            response.Headers.Location = new Uri("/client-profiling/risk-profiles/rp-1", UriKind.Relative);
            response.Content.Headers.LastModified = new DateTimeOffset(2019, 9, 27, 11, 0, 0, TimeSpan.Zero);
            return response;
        });
        await using var provider = BuildProvider(handler, out var scope);
        scope.ServiceProvider.GetRequiredService<IBloxxTokenStore>().Token = " my-token ";

        var result = await scope.ServiceProvider.GetRequiredService<IClientProfileClient>()
            .CreateRiskProfileAsync(["p1", "p2"], createEmpty: false, tags: ["retail"]);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(
            "https://example.test/client-profiling/risk-profiles?ownerId=p1&ownerId=p2&createEmpty=false&tag=retail",
            request.Uri.ToString());
        Assert.Equal("my-token", request.Headers["x-id-token"]);
        Assert.Equal("rp-1", result.Value.Id);
        Assert.Equal(QuestionnaireStatus.New, result.Value.Status);
        Assert.Equal("Fri, 27 Sep 2019 11:00:00 GMT", result.LastModified);
        Assert.Equal("https://example.test/client-profiling/risk-profiles/rp-1", result.Location?.ToString());
    }

    [Fact]
    public async Task ResolveRiskProfile_RedirectIsNotFollowed_AndLocationReturned()
    {
        var handler = new StubHandler(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.SeeOther);
            response.Headers.Location = new Uri("https://example.test/client-profiling/risk-profiles/rp-9");
            return response;
        });
        await using var provider = BuildProvider(handler, out var scope);

        var location = await scope.ServiceProvider.GetRequiredService<IClientProfileClient>()
            .ResolveRiskProfileAsync(["p1"]);

        Assert.Equal("https://example.test/client-profiling/risk-profiles/rp-9", location?.ToString());
    }

    [Fact]
    public async Task GetRiskProfile_ErrorResponse_ThrowsWithProblemDetail()
    {
        var handler = new StubHandler(_ =>
            Json(HttpStatusCode.Forbidden, """{"title":"Forbidden","detail":"Missing CP_RISK_PROFILE"}"""));
        await using var provider = BuildProvider(handler, out var scope);

        var ex = await Assert.ThrowsAsync<BloxxApiException>(() =>
            scope.ServiceProvider.GetRequiredService<IClientProfileClient>().GetRiskProfileAsync("rp-1"));

        Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
        Assert.Contains("Missing CP_RISK_PROFILE", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Forbidden", ex.ResponseBody, StringComparison.Ordinal);
    }

    private static ServiceProvider BuildProvider(StubHandler handler, out IServiceScope scope)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["BloxxApi:BaseUrl"] = "https://example.test/" })
            .Build();

        var services = new ServiceCollection();
        services.AddInfrastructure(configuration);
        services.AddHttpClient(nameof(IClientProfileClient)).ConfigurePrimaryHttpMessageHandler(() => handler);

        var provider = services.BuildServiceProvider();
        scope = provider.CreateScope();
        return provider;
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body) =>
        new(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };

    private sealed record RecordedRequest(HttpMethod Method, Uri Uri, Dictionary<string, string> Headers);

    private sealed class StubHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public List<RecordedRequest> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(new RecordedRequest(
                request.Method,
                request.RequestUri!,
                request.Headers.ToDictionary(h => h.Key, h => string.Join(',', h.Value), StringComparer.OrdinalIgnoreCase)));
            return Task.FromResult(respond(request));
        }
    }
}
