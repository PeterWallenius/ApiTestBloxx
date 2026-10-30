using System.Text;

namespace ApiTestBloxx.Infrastructure.ApiClients;

internal sealed class BloxxQuery
{
    private readonly List<string> _parts = [];

    public BloxxQuery Add(string name, string? value)
    {
        if (value is not null)
        {
            _parts.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}");
        }

        return this;
    }

    public BloxxQuery Add(string name, bool value) => Add(name, value ? "true" : "false");

    public BloxxQuery Add(string name, IEnumerable<string>? values)
    {
        foreach (var value in values ?? [])
        {
            Add(name, value);
        }

        return this;
    }

    public override string ToString()
    {
        if (_parts.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder("?");
        builder.AppendJoin('&', _parts);
        return builder.ToString();
    }
}
