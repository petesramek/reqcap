using System.Linq.Expressions;
using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Internal;
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
    /// Adds a custom predicate issue condition.
    /// </summary>
    /// <param name="name">The rule name.</param>
    /// <param name="predicate">The predicate that returns <see langword="true" /> when the issue should be produced.</param>
    /// <param name="severity">The severity used when the predicate returns <see langword="true" />.</param>
    /// <param name="alias">An optional external alias for the rule.</param>
    /// <param name="message">An optional issue message.</param>
    /// <returns>The current requirement builder.</returns>
    public RequirementBuilder<TCapability> Rule(
        string name,
        Func<TCapability, bool> predicate,
        RequirementSeverity severity,
        string? alias = null,
        string? message = null)
    {
        return AddRule(new PredicateRule<TCapability>(
            name,
            predicate,
            severity,
            alias,
            message));
    }

    /// <summary>
    /// Starts an ordered issue-condition chain for a property.
    /// </summary>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="expression">The property expression.</param>
    /// <returns>A property chain builder.</returns>
    public RequirementPropertyBuilder<TCapability, TProperty> Property<TProperty>(
        Expression<Func<TCapability, TProperty>> expression)
    {
        return new RequirementPropertyBuilder<TCapability, TProperty>(this, expression);
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
        ArgumentValidation.ThrowIfWhiteSpace(name, nameof(name));
        ArgumentValidation.ThrowIfWhiteSpace(alias, nameof(alias));

        var group = new RuleGroup<TCapability>(LogicalOperator.And, name, alias);
        build(new GroupBuilder<TCapability>(group));
        ThrowIfEmptyGroup(group, name);
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
        ArgumentValidation.ThrowIfWhiteSpace(name, nameof(name));
        ArgumentValidation.ThrowIfWhiteSpace(alias, nameof(alias));

        var group = new RuleGroup<TCapability>(LogicalOperator.Or, name, alias);
        build(new GroupBuilder<TCapability>(group));
        ThrowIfEmptyGroup(group, name);
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

    private static void ThrowIfEmptyGroup(RuleGroup<TCapability> group, string? name)
    {
        if (group.RuleCount == 0)
        {
            throw new InvalidOperationException(name is null
                ? "Group must contain at least one rule."
                : $"Group '{name}' must contain at least one rule.");
        }
    }
}
