using System.Linq.Expressions;
using ReqCap.Abstractions;
using ReqCap.Rules;

namespace ReqCap.Builder;

/// <summary>
/// Builds comparison operations for a property.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
/// <typeparam name="TNext">The builder type returned after the rule is finalized.</typeparam>
public sealed class PropertyBuilder<TCapability, TProperty, TNext>
    where TCapability : ICapability
    where TProperty : IComparable<TProperty>
{
    private readonly Expression<Func<TCapability, TProperty>> _expression;
    private readonly Func<IRule<TCapability>, TNext> _addRule;
    private readonly Func<TNext> _next;

    internal PropertyBuilder(
        Expression<Func<TCapability, TProperty>> expression,
        Func<IRule<TCapability>, TNext> addRule,
        Func<TNext> next)
    {
        _expression = expression;
        _addRule = addRule;
        _next = next;
    }

    /// <summary>Creates a greater-than-or-equal comparison.</summary>
    public SeverityBuilder<TCapability, TProperty, TNext> GreaterOrEqual(TProperty value) => Create(value, ComparisonOperator.GreaterOrEqual);

    /// <summary>Creates a greater-than comparison.</summary>
    public SeverityBuilder<TCapability, TProperty, TNext> GreaterThan(TProperty value) => Create(value, ComparisonOperator.GreaterThan);

    /// <summary>Creates a less-than-or-equal comparison.</summary>
    public SeverityBuilder<TCapability, TProperty, TNext> LessOrEqual(TProperty value) => Create(value, ComparisonOperator.LessOrEqual);

    /// <summary>Creates a less-than comparison.</summary>
    public SeverityBuilder<TCapability, TProperty, TNext> LessThan(TProperty value) => Create(value, ComparisonOperator.LessThan);

    /// <summary>Creates an equality comparison.</summary>
    public SeverityBuilder<TCapability, TProperty, TNext> Equal(TProperty value) => Create(value, ComparisonOperator.Equal);

    /// <summary>Creates an inequality comparison.</summary>
    public SeverityBuilder<TCapability, TProperty, TNext> NotEqual(TProperty value) => Create(value, ComparisonOperator.NotEqual);

    private SeverityBuilder<TCapability, TProperty, TNext> Create(TProperty value, ComparisonOperator op)
    {
        return new SeverityBuilder<TCapability, TProperty, TNext>(_expression, value, op, _addRule, _next);
    }
}
