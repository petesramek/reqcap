using ReqCap.Abstractions;
using ReqCap.Results;
using ReqCap.Rules;
using System.Linq.Expressions;

namespace ReqCap.Builder;

/// <summary>
/// Finalizes a rule by assigning severity and metadata.
/// </summary>
public sealed class SeverityBuilder<TCapability, TProperty, TNext>
    where TCapability : ICapability
    where TProperty : IComparable<TProperty> {
    private readonly Expression<Func<TCapability, TProperty>> _expression;
    private readonly TProperty _value;
    private readonly ComparisonOperator _operator;
    private readonly Func<IRule<TCapability>, TNext> _addRule;
    private readonly Func<TNext> _next;

    internal SeverityBuilder(Expression<Func<TCapability, TProperty>> expression, TProperty value, ComparisonOperator op, Func<IRule<TCapability>, TNext> addRule, Func<TNext> next) {
        _expression = expression;
        _value = value;
        _operator = op;
        _addRule = addRule;
        _next = next;
    }

    /// <summary>Creates an error rule.</summary>
    public TNext AsError(string? name = null, string? alias = null, string? message = null) => AddComparisonRule(RequirementSeverity.Error, name, alias, message);
    /// <summary>Creates a warning rule.</summary>
    public TNext AsWarning(string? name = null, string? alias = null, string? message = null) => AddComparisonRule(RequirementSeverity.Warning, name, alias, message);

    private TNext AddComparisonRule(RequirementSeverity severity, string? name, string? alias, string? message) {
        var rule = new ComparisonRule<TCapability, TProperty>(_expression, _value, _operator, severity, name, alias, message);
        _addRule(rule);
        return _next();
    }
}
