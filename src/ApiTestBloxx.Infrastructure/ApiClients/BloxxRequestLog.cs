using ApiTestBloxx.Application.Bloxx;

namespace ApiTestBloxx.Infrastructure.ApiClients;

internal sealed class BloxxRequestLog : IBloxxRequestLog
{
    public string? LastCurl { get; set; }
}
