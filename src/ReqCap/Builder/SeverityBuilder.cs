using System.Linq.Expressions;
using ReqCap.Abstractions;
using ReqCap.Results;
using ReqCap.Rules;

namespace ReqCap.Builder;

/// <summary>
/// Finalizes a rule by assigning severity and metadata.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
/// <typeparam name="TNext">The builder type returned after the rule is finalized.</typeparam>
public sealed class SeverityBuilder<TCapability, TProperty, TNext>
    where TCapability : ICapability
    where TProperty : IComparable<TProperty>
{
    private readonly Expression<Func<TCapability, TProperty>> _expression;
    private readonly TProperty _value;
    private readonly ComparisonOperator _operator;
    private readonly Func<IRule<TCapability>, TNext> _addRule;
    private readonly Func<TNext> _next;

    internal SeverityBuilder(
        Expression<Func<TCapability, TProperty>> expression,
        TProperty value,
        ComparisonOperator op,
        Func<IRule<TCapability>, TNext> addRule,
        Func<TNext> next)
    {
        _expression = expression;
        _value = value;
        _operator = op;
        _addRule = addRule;
        _next = next;
    }

    /// <summary>
    /// Creates an error rule.
    /// </summary>
    /// <param name="name">The optional rule name.</param>
    /// <param name="alias">The optional rule alias.</param>
    /// <param name="message">The optional failure message.</param>
    /// <returns>The next builder.</returns>
    public TNext AsError(string? name = null, string? alias = null, string? message = null)
    {
        return AddComparisonRule(RequirementSeverity.Error, name, alias, message);
    }

    /// <summary>
    /// Creates a warning rule.
    /// </summary>
    /// <param name="name">The optional rule name.</param>
    /// <param name="alias">The optional rule alias.</param>
    /// <param name="message">The optional failure message.</param>
    /// <returns>The next builder.</returns>
    public TNext AsWarning(string? name = null, string? alias = null, string? message = null)
    {
        return AddComparisonRule(RequirementSeverity.Warning, name, alias, message);
    }

    private TNext AddComparisonRule(
        RequirementSeverity severity,
        string? name,
        string? alias,
        string? message)
    {
        var rule = new ComparisonRule<TCapability, TProperty>(
            _expression,
            _value,
            _operator,
            severity,
            name,
            alias,
            message);

        _addRule(rule);
        return _next();
    }
}
