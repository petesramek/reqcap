
using ReqCap.Abstractions;
using ReqCap.Models;

namespace ReqCap.Groups;

public sealed class RuleGroup<TCapability> : IRule<TCapability>
    where TCapability : ICapability
{
    private readonly List<IRule<TCapability>> _rules = new();
    private readonly LogicalOperator _operator;
    private readonly string? _name;
    private readonly string? _alias;

    public RuleGroup(LogicalOperator op, string? name = null, string? alias = null)
    {
        _operator = op;
        _name = name;
        _alias = alias;
    }

    public void Add(IRule<TCapability> rule) => _rules.Add(rule);

    public EvaluationResult Evaluate(TCapability instance)
    {
        return _operator switch
        {
            LogicalOperator.And => EvaluateAnd(instance),
            LogicalOperator.Or => EvaluateOr(instance),
            LogicalOperator.Not => EvaluateNot(instance),
            _ => throw new InvalidOperationException()
        };
    }

    private EvaluationResult EvaluateAnd(TCapability instance)
    {
        var errors = new List<Issue>();
        var warnings = new List<Issue>();

        foreach (var rule in _rules)
        {
            var res = rule.Evaluate(instance);
            errors.AddRange(res.Errors);
            warnings.AddRange(res.Warnings);
        }

        Tag(errors, warnings);

        return new EvaluationResult
        {
            Allowed = errors.Count == 0,
            Errors = errors,
            Warnings = warnings
        };
    }

    private EvaluationResult EvaluateOr(TCapability instance)
    {
        foreach (var rule in _rules)
        {
            var res = rule.Evaluate(instance);
            if (res.Allowed)
                return EvaluationResult.Ok();
        }

        var errors = new List<Issue>();
        var warnings = new List<Issue>();

        foreach (var rule in _rules)
        {
            var res = rule.Evaluate(instance);
            errors.AddRange(res.Errors);
            warnings.AddRange(res.Warnings);
        }

        Tag(errors, warnings);

        return new EvaluationResult
        {
            Allowed = false,
            Errors = errors,
            Warnings = warnings
        };
    }

    private EvaluationResult EvaluateNot(TCapability instance) {
        if (_rules.Count != 1) {
            throw new InvalidOperationException("NOT requires exactly one child");
        }

        var result = _rules[0].Evaluate(instance);

        if (!result.Allowed) {
            return EvaluationResult.Ok();
        }

        var issue = new Issue {
            Property = _name ?? "NOT",
            Message = _name is null
                ? "NOT group condition was satisfied."
                : $"NOT group '{_name}' condition was satisfied.",
            Severity = RequirementSeverity.Error,
            GroupName = _name,
            GroupAlias = _alias
        };

        return new EvaluationResult {
            Allowed = false,
            Errors = [issue],
            Warnings = result.Warnings
        };
    }

    private void Tag(IReadOnlyList<Issue> errors, IReadOnlyList<Issue> warnings)
    {
        foreach (var i in errors.Concat(warnings))
        {
            i.GroupName ??= _name;
            i.GroupAlias ??= _alias;
        }
    }
}
