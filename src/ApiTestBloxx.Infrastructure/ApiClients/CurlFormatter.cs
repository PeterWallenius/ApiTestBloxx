using System.Text;

namespace ApiTestBloxx.Infrastructure.ApiClients;

internal static class CurlFormatter
{
    public static string Format(HttpClient client, HttpRequestMessage request, string? body, string tokenHeader, string tokenPlaceholder)
    {
        var uri = new Uri(client.BaseAddress!, request.RequestUri!);
        var builder = new StringBuilder("curl");
        if (request.Method != HttpMethod.Get)
        {
            builder.Append(" -X ").Append(request.Method.Method);
        }

        builder.Append(' ').Append(Quote(uri.AbsoluteUri));

        var headers = client.DefaultRequestHeaders.Concat(request.Headers);
        if (request.Content is not null)
        {
            headers = headers.Concat(request.Content.Headers);
        }

        foreach (var (name, values) in headers)
        {
            var value = string.Equals(name, tokenHeader, StringComparison.OrdinalIgnoreCase)
                ? tokenPlaceholder
                : string.Join(", ", values);
            builder.Append(" -H ").Append(Quote($"{name}: {value}"));
        }

        if (body is not null)
        {
            builder.Append(" -d ").Append(Quote(body));
        }

        return builder.ToString();
    }

    private static string Quote(string value) => "'" + value.Replace("'", "'\\''", StringComparison.Ordinal) + "'";
}
