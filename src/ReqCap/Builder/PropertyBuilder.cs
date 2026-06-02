using System.Linq.Expressions;
using ReqCap.Abstractions;
using ReqCap.Requirements;
using ReqCap.Results;
using ReqCap.Rules;

namespace ReqCap.Builder;

/// <summary>
/// Builds an ordered issue-condition chain for a requirement property.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
public sealed class RequirementPropertyBuilder<TCapability, TProperty>
    where TCapability : ICapability
    where TProperty : IComparable<TProperty>
{
    private readonly RequirementBuilder<TCapability> _parent;
    private readonly PropertyRuleChain<TCapability, TProperty> _chain;
    private bool _added;

    internal RequirementPropertyBuilder(
        RequirementBuilder<TCapability> parent,
        Expression<Func<TCapability, TProperty>> expression)
    {
        _parent = parent;
        _chain = new PropertyRuleChain<TCapability, TProperty>(expression);
    }

    /// <summary>
    /// Creates a greater-than-or-equal issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public RequirementSeverityBuilder<TCapability, TProperty> GreaterOrEqual(TProperty value)
    {
        return Create(value, ComparisonOperator.GreaterOrEqual);
    }

    /// <summary>
    /// Creates a greater-than issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public RequirementSeverityBuilder<TCapability, TProperty> GreaterThan(TProperty value)
    {
        return Create(value, ComparisonOperator.GreaterThan);
    }

    /// <summary>
    /// Creates a less-than-or-equal issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public RequirementSeverityBuilder<TCapability, TProperty> LessOrEqual(TProperty value)
    {
        return Create(value, ComparisonOperator.LessOrEqual);
    }

    /// <summary>
    /// Creates a less-than issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public RequirementSeverityBuilder<TCapability, TProperty> LessThan(TProperty value)
    {
        return Create(value, ComparisonOperator.LessThan);
    }

    /// <summary>
    /// Creates an equality issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public RequirementSeverityBuilder<TCapability, TProperty> Equal(TProperty value)
    {
        return Create(value, ComparisonOperator.Equal);
    }

    /// <summary>
    /// Creates an inequality issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public RequirementSeverityBuilder<TCapability, TProperty> NotEqual(TProperty value)
    {
        return Create(value, ComparisonOperator.NotEqual);
    }

    /// <summary>
    /// Starts a new independent property chain.
    /// </summary>
    /// <typeparam name="TOtherProperty">The other property type.</typeparam>
    /// <param name="expression">The other property expression.</param>
    /// <returns>A property chain builder for the other property.</returns>
    public RequirementPropertyBuilder<TCapability, TOtherProperty> Property<TOtherProperty>(
        Expression<Func<TCapability, TOtherProperty>> expression)
        where TOtherProperty : IComparable<TOtherProperty>
    {
        ThrowIfEmpty();
        return _parent.Property(expression);
    }

    /// <summary>
    /// Adds a custom rule instance to the parent requirement.
    /// </summary>
    /// <param name="rule">The rule to add.</param>
    /// <returns>The parent requirement builder.</returns>
    public RequirementBuilder<TCapability> AddRule(IRule<TCapability> rule)
    {
        ThrowIfEmpty();
        return _parent.AddRule(rule);
    }

    /// <summary>
    /// Adds a custom predicate issue condition to the parent requirement.
    /// </summary>
    /// <param name="name">The rule name.</param>
    /// <param name="predicate">The predicate that returns <see langword="true" /> when the issue should be produced.</param>
    /// <param name="severity">The severity used when the predicate returns <see langword="true" />.</param>
    /// <param name="alias">An optional external alias for the rule.</param>
    /// <param name="message">An optional issue message.</param>
    /// <returns>The parent requirement builder.</returns>
    public RequirementBuilder<TCapability> Rule(
        string name,
        Func<TCapability, bool> predicate,
        RequirementSeverity severity,
        string? alias = null,
        string? message = null)
    {
        ThrowIfEmpty();
        return _parent.Rule(name, predicate, severity, alias, message);
    }

    /// <summary>
    /// Adds an AND group to the parent requirement.
    /// </summary>
    /// <param name="name">The optional group name.</param>
    /// <param name="build">The group builder callback.</param>
    /// <param name="alias">The optional group alias.</param>
    /// <returns>The parent requirement builder.</returns>
    public RequirementBuilder<TCapability> And(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null)
    {
        ThrowIfEmpty();
        return _parent.And(name, build, alias);
    }

    /// <summary>
    /// Adds an OR group to the parent requirement.
    /// </summary>
    /// <param name="name">The optional group name.</param>
    /// <param name="build">The group builder callback.</param>
    /// <param name="alias">The optional group alias.</param>
    /// <returns>The parent requirement builder.</returns>
    public RequirementBuilder<TCapability> Or(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null)
    {
        ThrowIfEmpty();
        return _parent.Or(name, build, alias);
    }

    /// <summary>
    /// Builds the parent requirement.
    /// </summary>
    /// <returns>The built requirement.</returns>
    public Requirement<TCapability> Build()
    {
        ThrowIfEmpty();
        return _parent.Build();
    }

    internal RequirementPropertyBuilder<TCapability, TProperty> AddCondition(PropertyCondition<TProperty> condition)
    {
        _chain.Add(condition);

        if (!_added)
        {
            _parent.AddRule(_chain);
            _added = true;
        }

        return this;
    }

    private RequirementSeverityBuilder<TCapability, TProperty> Create(TProperty value, ComparisonOperator op)
    {
        return new RequirementSeverityBuilder<TCapability, TProperty>(this, value, op);
    }

    private void ThrowIfEmpty()
    {
        if (!_chain.HasConditions)
        {
            throw new InvalidOperationException(
                $"Property chain '{_chain.PropertyPath}' must contain at least one condition.");
        }
    }
}

/// <summary>
/// Builds an ordered issue-condition chain for a group property.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
public sealed class GroupPropertyBuilder<TCapability, TProperty>
    where TCapability : ICapability
    where TProperty : IComparable<TProperty>
{
    private readonly GroupBuilder<TCapability> _parent;
    private readonly PropertyRuleChain<TCapability, TProperty> _chain;
    private bool _added;

    internal GroupPropertyBuilder(
        GroupBuilder<TCapability> parent,
        Expression<Func<TCapability, TProperty>> expression)
    {
        _parent = parent;
        _chain = new PropertyRuleChain<TCapability, TProperty>(expression);
    }

    /// <summary>
    /// Creates a greater-than-or-equal issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public GroupSeverityBuilder<TCapability, TProperty> GreaterOrEqual(TProperty value)
    {
        return Create(value, ComparisonOperator.GreaterOrEqual);
    }

    /// <summary>
    /// Creates a greater-than issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public GroupSeverityBuilder<TCapability, TProperty> GreaterThan(TProperty value)
    {
        return Create(value, ComparisonOperator.GreaterThan);
    }

    /// <summary>
    /// Creates a less-than-or-equal issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public GroupSeverityBuilder<TCapability, TProperty> LessOrEqual(TProperty value)
    {
        return Create(value, ComparisonOperator.LessOrEqual);
    }

    /// <summary>
    /// Creates a less-than issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public GroupSeverityBuilder<TCapability, TProperty> LessThan(TProperty value)
    {
        return Create(value, ComparisonOperator.LessThan);
    }

    /// <summary>
    /// Creates an equality issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public GroupSeverityBuilder<TCapability, TProperty> Equal(TProperty value)
    {
        return Create(value, ComparisonOperator.Equal);
    }

    /// <summary>
    /// Creates an inequality issue condition.
    /// </summary>
    /// <param name="value">The value to compare against.</param>
    /// <returns>A severity builder for the issue condition.</returns>
    public GroupSeverityBuilder<TCapability, TProperty> NotEqual(TProperty value)
    {
        return Create(value, ComparisonOperator.NotEqual);
    }

    /// <summary>
    /// Starts a new independent property chain.
    /// </summary>
    /// <typeparam name="TOtherProperty">The other property type.</typeparam>
    /// <param name="expression">The other property expression.</param>
    /// <returns>A property chain builder for the other property.</returns>
    public GroupPropertyBuilder<TCapability, TOtherProperty> Property<TOtherProperty>(
        Expression<Func<TCapability, TOtherProperty>> expression)
        where TOtherProperty : IComparable<TOtherProperty>
    {
        ThrowIfEmpty();
        return _parent.Property(expression);
    }

    /// <summary>
    /// Adds a custom rule instance to the parent group.
    /// </summary>
    /// <param name="rule">The rule to add.</param>
    /// <returns>The parent group builder.</returns>
    public GroupBuilder<TCapability> AddRule(IRule<TCapability> rule)
    {
        ThrowIfEmpty();
        return _parent.AddRule(rule);
    }

    /// <summary>
    /// Adds a custom predicate issue condition to the parent group.
    /// </summary>
    /// <param name="name">The rule name.</param>
    /// <param name="predicate">The predicate that returns <see langword="true" /> when the issue should be produced.</param>
    /// <param name="severity">The severity used when the predicate returns <see langword="true" />.</param>
    /// <param name="alias">An optional external alias for the rule.</param>
    /// <param name="message">An optional issue message.</param>
    /// <returns>The parent group builder.</returns>
    public GroupBuilder<TCapability> Rule(
        string name,
        Func<TCapability, bool> predicate,
        RequirementSeverity severity,
        string? alias = null,
        string? message = null)
    {
        ThrowIfEmpty();
        return _parent.Rule(name, predicate, severity, alias, message);
    }

    /// <summary>
    /// Adds a nested AND group to the parent group.
    /// </summary>
    /// <param name="name">The optional group name.</param>
    /// <param name="build">The group builder callback.</param>
    /// <param name="alias">The optional group alias.</param>
    /// <returns>The parent group builder.</returns>
    public GroupBuilder<TCapability> And(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null)
    {
        ThrowIfEmpty();
        return _parent.And(name, build, alias);
    }

    /// <summary>
    /// Adds a nested OR group to the parent group.
    /// </summary>
    /// <param name="name">The optional group name.</param>
    /// <param name="build">The group builder callback.</param>
    /// <param name="alias">The optional group alias.</param>
    /// <returns>The parent group builder.</returns>
    public GroupBuilder<TCapability> Or(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null)
    {
        ThrowIfEmpty();
        return _parent.Or(name, build, alias);
    }

    internal GroupPropertyBuilder<TCapability, TProperty> AddCondition(PropertyCondition<TProperty> condition)
    {
        _chain.Add(condition);

        if (!_added)
        {
            _parent.AddRule(_chain);
            _added = true;
        }

        return this;
    }

    private GroupSeverityBuilder<TCapability, TProperty> Create(TProperty value, ComparisonOperator op)
    {
        return new GroupSeverityBuilder<TCapability, TProperty>(this, value, op);
    }

    private void ThrowIfEmpty()
    {
        if (!_chain.HasConditions)
        {
            throw new InvalidOperationException(
                $"Property chain '{_chain.PropertyPath}' must contain at least one condition.");
        }
    }
}
