using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Results;
using ReqCap.Rules;
using System.Linq.Expressions;

namespace ReqCap.Builder;

/// <summary>
/// Builds rules inside a logical group.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
public sealed class GroupBuilder<TCapability>
    where TCapability : ICapability {
    private readonly RuleGroup<TCapability> _group;

    internal GroupBuilder(RuleGroup<TCapability> group) => _group = group;

    /// <summary>Adds a custom rule instance to the group.</summary>
    public GroupBuilder<TCapability> AddRule(IRule<TCapability> rule) {
        _group.Add(rule);
        return this;
    }

    /// <summary>Adds a custom predicate rule to the group.</summary>
    public GroupBuilder<TCapability> Rule(string name, Func<TCapability, bool> predicate, RequirementSeverity severity, string? alias = null, string? message = null) {
        return AddRule(new PredicateRule<TCapability>(name, predicate, severity, alias, message));
    }

    /// <summary>Starts a comparable property rule inside the group.</summary>
    public PropertyBuilder<TCapability, TProperty, GroupBuilder<TCapability>> Property<TProperty>(Expression<Func<TCapability, TProperty>> expression)
        where TProperty : IComparable<TProperty> {
        return new PropertyBuilder<TCapability, TProperty, GroupBuilder<TCapability>>(expression, AddRule, () => this);
    }

    /// <summary>Adds a nested AND group.</summary>
    public GroupBuilder<TCapability> And(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        ArgumentNullException.ThrowIfNull(build);
        var group = new RuleGroup<TCapability>(LogicalOperator.And, name, alias);
        build(new GroupBuilder<TCapability>(group));
        return AddRule(group);
    }

    /// <summary>Adds a nested OR group.</summary>
    public GroupBuilder<TCapability> Or(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        ArgumentNullException.ThrowIfNull(build);
        var group = new RuleGroup<TCapability>(LogicalOperator.Or, name, alias);
        build(new GroupBuilder<TCapability>(group));
        return AddRule(group);
    }

    /// <summary>Adds a nested NOT group.</summary>
    public GroupBuilder<TCapability> Not(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        ArgumentNullException.ThrowIfNull(build);
        var outer = new RuleGroup<TCapability>(LogicalOperator.Not, name, alias);
        var inner = new RuleGroup<TCapability>(LogicalOperator.And);
        build(new GroupBuilder<TCapability>(inner));
        outer.Add(inner);
        return AddRule(outer);
    }
}
