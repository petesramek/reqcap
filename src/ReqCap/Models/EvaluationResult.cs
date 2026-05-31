
namespace ReqCap.Models;

public sealed class EvaluationResult
{
    public bool Allowed { get; init; }
    public IReadOnlyList<Issue> Errors { get; init; } = [];
    public IReadOnlyList<Issue> Warnings { get; init; } = [];

    public static EvaluationResult Ok() => new() { Allowed = true };
}
