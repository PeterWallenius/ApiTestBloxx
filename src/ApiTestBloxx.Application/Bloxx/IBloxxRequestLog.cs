namespace ApiTestBloxx.Application.Bloxx;

/// <summary>Holds the most recent outgoing request as a curl command; register as scoped (one per Blazor circuit).</summary>
public interface IBloxxRequestLog
{
    string? LastCurl { get; set; }
}
