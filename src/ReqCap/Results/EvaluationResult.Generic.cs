namespace ReqCap.Results;

/// <summary>
/// Represents the result of evaluating a requirement and projecting matched rules into consumer-defined result objects.
/// </summary>
/// <typeparam name="TResult">The consumer-defined result type.</typeparam>
public sealed class EvaluationResult<TResult>
{
    private static readonly EvaluationResult<TResult> SatisfiedResult = new()
    {
        Satisfied = true,
        Results = Array.Empty<TResult>(),
    };

    /// <summary>
    /// Gets a value indicating whether no requirement rules matched.
    /// </summary>
    public bool Satisfied { get; init; }

    /// <summary>
    /// Gets the consumer-defined result objects created from matched rules.
    /// </summary>
    public IReadOnlyList<TResult> Results { get; init; } = Array.Empty<TResult>();

    /// <summary>
    /// Creates a satisfied evaluation result.
    /// </summary>
    /// <returns>A satisfied evaluation result.</returns>
    public static EvaluationResult<TResult> SatisfiedResultInstance()
    {
        return SatisfiedResult;
    }

    /// <summary>
    /// Creates an evaluation result from projected consumer-defined result objects.
    /// </summary>
    /// <param name="results">The projected result objects.</param>
    /// <returns>An evaluation result containing the projected result objects.</returns>
    public static EvaluationResult<TResult> FromResults(IReadOnlyList<TResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);

        return results.Count == 0
            ? SatisfiedResult
            : new EvaluationResult<TResult>
            {
                Satisfied = false,
                Results = results,
            };
    }
}
