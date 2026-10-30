namespace ApiTestBloxx.Application.Bloxx;

/// <summary>Successful Bloxx response; <paramref name="LastModified"/> is the raw header value to send back as If-Unmodified-Since.</summary>
public sealed record BloxxResponse<T>(T Value, string? LastModified, Uri? Location);
