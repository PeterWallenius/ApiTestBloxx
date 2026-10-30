using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiTestBloxx.Application.Bloxx;

namespace ApiTestBloxx.Infrastructure.ApiClients;

internal abstract class BloxxApiClientBase(HttpClient httpClient, IBloxxTokenStore tokenStore)
{
    private const string TokenHeader = "x-id-token";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>Path prefix of the API relative to BaseUrl, with trailing slash.</summary>
    protected abstract string ApiPath { get; }

    protected async Task<BloxxResponse<T>> SendJsonAsync<T>(
        HttpMethod method,
        string path,
        BloxxQuery? query,
        IReadOnlyDictionary<string, string?>? headers,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, path, query, headers, body, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, content);

        T? value;
        try
        {
            value = JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new BloxxApiException(response.StatusCode, content, $"Svaret kunne ikke l\u00e6ses som forventet JSON: {ex.Message}");
        }

        if (value is null)
        {
            throw new BloxxApiException(response.StatusCode, content, "Svaret var tomt, men der blev forventet JSON.");
        }

        return new BloxxResponse<T>(value, GetLastModified(response), ResolveLocation(response));
    }

    protected async Task<BloxxResponse<string>> SendTextAsync(
        string path,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Get, path, null, headers, null, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, content);
        return new BloxxResponse<string>(content, GetLastModified(response), ResolveLocation(response));
    }

    protected async Task SendWithoutContentAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Get, path, null, null, null, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, content);
    }

    /// <summary>For operations answering with a redirect (303) to the resource; 304 gives null.</summary>
    protected async Task<Uri?> SendRedirectAsync(
        string path,
        BloxxQuery? query,
        IReadOnlyDictionary<string, string?>? headers,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(HttpMethod.Get, path, query, headers, null, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotModified)
        {
            return null;
        }

        var location = ResolveLocation(response);
        if ((int)response.StatusCode is >= 300 and < 400 && location is not null)
        {
            return location;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        EnsureSuccess(response, content);
        return location;
    }

    private async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        BloxxQuery? query,
        IReadOnlyDictionary<string, string?>? headers,
        object? body,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, new Uri(ApiPath + path + query, UriKind.Relative));

        var token = tokenStore.Token?.Trim();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.TryAddWithoutValidation(TokenHeader, token);
        }

        foreach (var (name, value) in headers ?? new Dictionary<string, string?>())
        {
            if (value is not null)
            {
                request.Headers.TryAddWithoutValidation(name, value);
            }
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: JsonOptions);
        }

        return await httpClient.SendAsync(request, cancellationToken);
    }

    private static void EnsureSuccess(HttpResponseMessage response, string? body)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new BloxxApiException(response.StatusCode, body, BuildErrorMessage(response, body));
    }

    private static string BuildErrorMessage(HttpResponseMessage response, string? body)
    {
        var message = $"Bloxx API svarede {(int)response.StatusCode} {response.ReasonPhrase}".TrimEnd();
        if (string.IsNullOrWhiteSpace(body))
        {
            return message;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Object)
            {
                var detail = ReadString(root, "detail") ?? ReadString(root, "title");
                if (detail is not null)
                {
                    return $"{message}: {detail}";
                }
            }
        }
        catch (JsonException)
        {
            // Body is not JSON; the raw body is still available on the exception.
        }

        return message;
    }

    private static string? ReadString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static string? GetLastModified(HttpResponseMessage response)
    {
        if (response.Content.Headers.TryGetValues("Last-Modified", out var contentValues)
            || response.Headers.TryGetValues("Last-Modified", out contentValues))
        {
            return contentValues.FirstOrDefault();
        }

        return null;
    }

    private Uri? ResolveLocation(HttpResponseMessage response)
    {
        var location = response.Headers.Location;
        if (location is null || location.IsAbsoluteUri || httpClient.BaseAddress is null)
        {
            return location;
        }

        return new Uri(httpClient.BaseAddress, location);
    }
}
