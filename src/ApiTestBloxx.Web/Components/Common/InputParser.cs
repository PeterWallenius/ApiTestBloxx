using ApiTestBloxx.Infrastructure.ApiClients.ClientProfile;

namespace ApiTestBloxx.Web.Components.Common;

public sealed class AnswerRow
{
    public string? QuestionId { get; set; }

    public string? AnswerId { get; set; }

    public string? Justification { get; set; }
}

public static class InputParser
{
    private static readonly char[] ListSeparators = [',', ';', '\n', '\r'];

    /// <summary>Splits on comma, semicolon and line breaks; trims and drops empty entries.</summary>
    public static string[] SplitList(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static string? NullIfEmpty(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    public static bool HasCompleteAnswers(IEnumerable<AnswerRow> rows) =>
        rows.Any() && rows.All(r => !string.IsNullOrWhiteSpace(r.QuestionId) && !string.IsNullOrWhiteSpace(r.AnswerId));

    public static IReadOnlyList<Answer> ToAnswers(IEnumerable<AnswerRow> rows) =>
        [.. rows.Select(r => new Answer
        {
            QuestionId = r.QuestionId!.Trim(),
            AnswerId = r.AnswerId!.Trim(),
            Justification = NullIfEmpty(r.Justification),
        })];
}
