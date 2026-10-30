using System.ComponentModel.DataAnnotations;

namespace ApiTestBloxx.Infrastructure.Configuration;

public sealed class BloxxApiOptions
{
    public const string SectionName = "BloxxApi";

    /// <summary>Host root shared by all Bloxx APIs; each API adds its own path prefix (e.g. client-profiling/).</summary>
    [Required, Url]
    public string BaseUrl { get; set; } = string.Empty;
}
