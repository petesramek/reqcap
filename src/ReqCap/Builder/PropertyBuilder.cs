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

    /// <summary>Creates a greater-than-or-equal issue condition.</summary>
    public RequirementSeverityBuilder<TCapability, TProperty> GreaterOrEqual(TProperty value) => Create(value, ComparisonOperator.GreaterOrEqual);

    /// <summary>Creates a greater-than issue condition.</summary>
    public RequirementSeverityBuilder<TCapability, TProperty> GreaterThan(TProperty value) => Create(value, ComparisonOperator.GreaterThan);

    /// <summary>Creates a less-than-or-equal issue condition.</summary>
    public RequirementSeverityBuilder<TCapability, TProperty> LessOrEqual(TProperty value) => Create(value, ComparisonOperator.LessOrEqual);

    /// <summary>Creates a less-than issue condition.</summary>
    public RequirementSeverityBuilder<TCapability, TProperty> LessThan(TProperty value) => Create(value, ComparisonOperator.LessThan);

    /// <summary>Creates an equality issue condition.</summary>
    public RequirementSeverityBuilder<TCapability, TProperty> Equal(TProperty value) => Create(value, ComparisonOperator.Equal);

    /// <summary>Creates an inequality issue condition.</summary>
    public RequirementSeverityBuilder<TCapability, TProperty> NotEqual(TProperty value) => Create(value, ComparisonOperator.NotEqual);

    /// <summary>Starts a new independent property chain.</summary>
    public RequirementPropertyBuilder<TCapability, TOtherProperty> Property<TOtherProperty>(Expression<Func<TCapability, TOtherProperty>> expression)
        where TOtherProperty : IComparable<TOtherProperty>
    {
        return _parent.Property(expression);
    }

    /// <summary>Adds a custom rule instance to the parent requirement.</summary>
    public RequirementBuilder<TCapability> AddRule(IRule<TCapability> rule) => _parent.AddRule(rule);

    /// <summary>Adds a custom predicate issue condition to the parent requirement.</summary>
    public RequirementBuilder<TCapability> Rule(string name, Func<TCapability, bool> predicate, RequirementSeverity severity, string? alias = null, string? message = null)
    {
        return _parent.Rule(name, predicate, severity, alias, message);
    }

    /// <summary>Adds an AND group to the parent requirement.</summary>
    public RequirementBuilder<TCapability> And(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) => _parent.And(name, build, alias);

    /// <summary>Adds an OR group to the parent requirement.</summary>
    public RequirementBuilder<TCapability> Or(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) => _parent.Or(name, build, alias);

    /// <summary>Builds the parent requirement.</summary>
    public Requirement<TCapability> Build() => _parent.Build();

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

    /// <summary>Creates a greater-than-or-equal issue condition.</summary>
    public GroupSeverityBuilder<TCapability, TProperty> GreaterOrEqual(TProperty value) => Create(value, ComparisonOperator.GreaterOrEqual);

    /// <summary>Creates a greater-than issue condition.</summary>
    public GroupSeverityBuilder<TCapability, TProperty> GreaterThan(TProperty value) => Create(value, ComparisonOperator.GreaterThan);

    /// <summary>Creates a less-than-or-equal issue condition.</summary>
    public GroupSeverityBuilder<TCapability, TProperty> LessOrEqual(TProperty value) => Create(value, ComparisonOperator.LessOrEqual);

    /// <summary>Creates a less-than issue condition.</summary>
    public GroupSeverityBuilder<TCapability, TProperty> LessThan(TProperty value) => Create(value, ComparisonOperator.LessThan);

    /// <summary>Creates an equality issue condition.</summary>
    public GroupSeverityBuilder<TCapability, TProperty> Equal(TProperty value) => Create(value, ComparisonOperator.Equal);

    /// <summary>Creates an inequality issue condition.</summary>
    public GroupSeverityBuilder<TCapability, TProperty> NotEqual(TProperty value) => Create(value, ComparisonOperator.NotEqual);

    /// <summary>Starts a new independent property chain.</summary>
    public GroupPropertyBuilder<TCapability, TOtherProperty> Property<TOtherProperty>(Expression<Func<TCapability, TOtherProperty>> expression)
        where TOtherProperty : IComparable<TOtherProperty>
    {
        return _parent.Property(expression);
    }

    /// <summary>Adds a custom rule instance to the parent group.</summary>
    public GroupBuilder<TCapability> AddRule(IRule<TCapability> rule) => _parent.AddRule(rule);

    /// <summary>Adds a custom predicate issue condition to the parent group.</summary>
    public GroupBuilder<TCapability> Rule(string name, Func<TCapability, bool> predicate, RequirementSeverity severity, string? alias = null, string? message = null)
    {
        return _parent.Rule(name, predicate, severity, alias, message);
    }

    /// <summary>Adds a nested AND group to the parent group.</summary>
    public GroupBuilder<TCapability> And(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) => _parent.And(name, build, alias);

    /// <summary>Adds a nested OR group to the parent group.</summary>
    public GroupBuilder<TCapability> Or(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) => _parent.Or(name, build, alias);

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
}
