using ReqCap.Abstractions;
using ReqCap.Results;

namespace ReqCap.Groups;

/// <summary>
/// Represents a logical group of rules.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
public sealed class RuleGroup<TCapability> : IRule<TCapability>
    where TCapability : ICapability
{
    private readonly List<IRule<TCapability>> _rules = [];
    private readonly LogicalOperator _operator;
    private readonly string? _name;
    private readonly string? _alias;

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleGroup{TCapability}"/> class.
    /// </summary>
    /// <param name="op">The logical operator.</param>
    /// <param name="name">The optional group name.</param>
    /// <param name="alias">The optional group alias.</param>
    public RuleGroup(LogicalOperator op, string? name = null, string? alias = null)
    {
        _operator = op;
        _name = name;
        _alias = alias;
    }

    /// <summary>
    /// Adds a rule to the group.
    /// </summary>
    /// <param name="rule">The rule to add.</param>
    public void Add(IRule<TCapability> rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rules.Add(rule);
    }

    /// <inheritdoc />
    public EvaluationResult Evaluate(TCapability capability)
    {
        return _operator switch
        {
            LogicalOperator.And => EvaluateAnd(capability),
            LogicalOperator.Or => EvaluateOr(capability),
            _ => throw new InvalidOperationException($"Unsupported logical operator '{_operator}'."),
        };
    }

    private EvaluationResult EvaluateAnd(TCapability capability)
    {
        var errors = new List<Issue>();
        var warnings = new List<Issue>();

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(capability);

            if (!result.Allowed)
            {
                errors.AddRange(result.Errors);
            }

            warnings.AddRange(result.Warnings);
        }

        Tag(errors, warnings);

        return new EvaluationResult
        {
            Allowed = errors.Count == 0,
            Errors = errors,
            Warnings = warnings,
        };
    }

    private EvaluationResult EvaluateOr(TCapability capability)
    {
        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(capability);
            if (result.Allowed)
            {
                return EvaluationResult.Ok();
            }
        }

        var errors = new List<Issue>();
        var warnings = new List<Issue>();

        foreach (var rule in _rules)
        {
            var result = rule.Evaluate(capability);
            errors.AddRange(result.Errors);
            warnings.AddRange(result.Warnings);
        }

        Tag(errors, warnings);

        return new EvaluationResult
        {
            Allowed = false,
            Errors = errors,
            Warnings = warnings,
        };
    }

    private void Tag(IReadOnlyList<Issue> errors, IReadOnlyList<Issue> warnings)
    {
        foreach (var issue in errors.Concat(warnings))
        {
            issue.GroupName ??= _name;
            issue.GroupAlias ??= _alias;
        }
    }
}
