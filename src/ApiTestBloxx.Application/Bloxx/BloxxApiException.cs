using System.Net;

namespace ApiTestBloxx.Application.Bloxx;

public sealed class BloxxApiException : Exception
{
    public BloxxApiException(HttpStatusCode statusCode, string? responseBody, string message)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }

    /// <summary>Raw response body as received, so the UI can show it unchanged.</summary>
    public string? ResponseBody { get; }
}
