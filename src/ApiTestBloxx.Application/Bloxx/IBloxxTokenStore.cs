namespace ApiTestBloxx.Application.Bloxx;

/// <summary>Holds the user's x-id-token in memory only; register as scoped (one per Blazor circuit).</summary>
public interface IBloxxTokenStore
{
    string? Token { get; set; }
}
