using System.Linq.Expressions;
using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Requirements;
using ReqCap.Results;
using ReqCap.Rules;

namespace ReqCap.Builder;

/// <summary>
/// Builds requirements for a capability type.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
public sealed class RequirementBuilder<TCapability>
    where TCapability : ICapability
{
    private readonly List<IRule<TCapability>> _rules = [];

    /// <summary>
    /// Adds a custom rule instance.
    /// </summary>
    /// <param name="rule">The rule to add.</param>
    /// <returns>The current requirement builder.</returns>
    public RequirementBuilder<TCapability> AddRule(IRule<TCapability> rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        _rules.Add(rule);
        return this;
    }

    /// <summary>
    /// Adds a custom predicate rule.
    /// </summary>
    /// <param name="name">The rule name.</param>
    /// <param name="predicate">The predicate used to evaluate the capability.</param>
    /// <param name="severity">The severity used when the rule fails.</param>
    /// <param name="alias">An optional external alias for the rule.</param>
    /// <param name="message">An optional failure message.</param>
    /// <returns>The current requirement builder.</returns>
    public RequirementBuilder<TCapability> Rule(
        string name,
        Func<TCapability, bool> predicate,
        RequirementSeverity severity,
        string? alias = null,
        string? message = null)
    {
        return AddRule(new PredicateRule<TCapability>(name, predicate, severity, alias, message));
    }

    /// <summary>
    /// Starts a comparable property rule.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="expression">The property expression.</param>
    /// <returns>A property rule builder.</returns>
    public PropertyBuilder<TCapability, TProperty, RequirementBuilder<TCapability>> Property<TProperty>(
        Expression<Func<TCapability, TProperty>> expression)
        where TProperty : IComparable<TProperty>
    {
        return new PropertyBuilder<TCapability, TProperty, RequirementBuilder<TCapability>>(
            expression,
            AddRule,
            () => this);
    }

    /// <summary>
    /// Adds an AND group.
    /// </summary>
    /// <param name="name">The optional group name.</param>
    /// <param name="build">The group builder callback.</param>
    /// <param name="alias">The optional group alias.</param>
    /// <returns>The current requirement builder.</returns>
    public RequirementBuilder<TCapability> And(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null)
    {
        ArgumentNullException.ThrowIfNull(build);

        var group = new RuleGroup<TCapability>(LogicalOperator.And, name, alias);
        build(new GroupBuilder<TCapability>(group));
        return AddRule(group);
    }

    /// <summary>
    /// Adds an OR group.
    /// </summary>
    /// <param name="name">The optional group name.</param>
    /// <param name="build">The group builder callback.</param>
    /// <param name="alias">The optional group alias.</param>
    /// <returns>The current requirement builder.</returns>
    public RequirementBuilder<TCapability> Or(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null)
    {
        ArgumentNullException.ThrowIfNull(build);

        var group = new RuleGroup<TCapability>(LogicalOperator.Or, name, alias);
        build(new GroupBuilder<TCapability>(group));
        return AddRule(group);
    }

    /// <summary>
    /// Builds the requirement.
    /// </summary>
    /// <returns>The requirement.</returns>
    public Requirement<TCapability> Build()
    {
        return new Requirement<TCapability>(_rules);
    }
}
