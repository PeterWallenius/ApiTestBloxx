using ApiTestBloxx.Application.Bloxx;

namespace ApiTestBloxx.Infrastructure.Authentication;

internal sealed class BloxxTokenStore : IBloxxTokenStore
{
    public string? Token { get; set; }
}
