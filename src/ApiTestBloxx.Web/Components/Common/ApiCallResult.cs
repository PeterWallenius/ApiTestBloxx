using System.Text.Json;
using System.Text.Json.Serialization;
using ApiTestBloxx.Application.Bloxx;

namespace ApiTestBloxx.Web.Components.Common;

public sealed record ApiCallResult(string? Body, string? LastModified, Uri? Location, string? Note = null)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static ApiCallResult From<T>(BloxxResponse<T> response) =>
        new(
            response.Value is string text ? text : JsonSerializer.Serialize(response.Value, JsonOptions),
            response.LastModified,
            response.Location);

    public static ApiCallResult FromLocation(Uri? location) =>
        location is null
            ? new ApiCallResult(null, null, null, "304 Not Modified - ingen ændring siden sidste kald.")
            : new ApiCallResult(null, null, location, "Omdirigeret til ressourcen (3xx).");

    public static ApiCallResult Success(string note) => new(null, null, null, note);
}
