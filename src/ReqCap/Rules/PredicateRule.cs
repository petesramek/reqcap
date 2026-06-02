using ReqCap.Abstractions;
using ReqCap.Internal;
using ReqCap.Results;

namespace ReqCap.Rules;

/// <summary>
/// Represents a custom predicate-based issue condition for a capability.
/// </summary>
/// <typeparam name="TCapability">The capability type evaluated by the rule.</typeparam>
public sealed class PredicateRule<TCapability> : IRule<TCapability>
    where TCapability : ICapability
{
    private readonly string _name;
    private readonly Func<TCapability, bool> _predicate;
    private readonly RequirementSeverity _severity;
    private readonly string? _alias;
    private readonly string? _message;

    /// <summary>
    /// Initializes a new instance of the <see cref="PredicateRule{TCapability}" /> class.
    /// </summary>
    /// <param name="name">The rule name.</param>
    /// <param name="predicate">The predicate that returns <see langword="true" /> when the issue should be produced.</param>
    /// <param name="severity">The severity used when the predicate returns <see langword="true" />.</param>
    /// <param name="alias">An optional external alias for the rule.</param>
    /// <param name="message">An optional issue message.</param>
    public PredicateRule(
        string name,
        Func<TCapability, bool> predicate,
        RequirementSeverity severity,
        string? alias = null,
        string? message = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentValidation.ThrowIfInvalidEnum(severity, nameof(severity));
        ArgumentValidation.ThrowIfWhiteSpace(alias, nameof(alias));
        ArgumentValidation.ThrowIfWhiteSpace(message, nameof(message));

        _name = name;
        _predicate = predicate;
        _severity = severity;
        _alias = alias;
        _message = message;
    }

    /// <inheritdoc />
    public EvaluationResult Evaluate(TCapability capability)
    {
        if (!_predicate(capability))
        {
            return EvaluationResult.Ok();
        }

        return EvaluationResult.FromIssue(new Issue
        {
            Property = string.Empty,
            Message = _message ?? $"Rule '{_name}' matched.",
            Severity = _severity,
            RuleName = _name,
            RuleAlias = _alias,
        });
    }
}
