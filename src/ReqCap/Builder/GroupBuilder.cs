namespace ReqCap.Builder;

using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Internal;
using ReqCap.Results;
using ReqCap.Rules;
using System.Linq.Expressions;

/// <summary>
/// Builds rules inside a logical group.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
public sealed class GroupBuilder<TCapability>
    where TCapability : ICapability {
    private readonly RuleGroup<TCapability> _group;

    internal GroupBuilder(RuleGroup<TCapability> group) {
        _group = group;
    }

    /// <summary>
    /// Adds a custom rule instance to the group.
    /// </summary>
    /// <param name="rule">The rule to add.</param>
    /// <returns>The current group builder.</returns>
    public GroupBuilder<TCapability> AddRule(IRule<TCapability> rule) {
        _group.Add(rule);
        return this;
    }

    /// <summary>
    /// Adds a custom predicate issue condition to the group.
    /// </summary>
    /// <param name="name">The rule name.</param>
    /// <param name="predicate">The predicate that returns <see langword="true" /> when the issue should be produced.</param>
    /// <param name="severity">The severity used when the predicate returns <see langword="true" />.</param>
    /// <param name="alias">An optional external alias for the rule.</param>
    /// <param name="message">An optional issue message.</param>
    /// <returns>The current group builder.</returns>
    public GroupBuilder<TCapability> Rule(
        string name,
        Func<TCapability, bool> predicate,
        RequirementSeverity severity,
        string? alias = null,
        string? message = null) {
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
    public GroupPropertyBuilder<TCapability, TProperty> Property<TProperty>(
        Expression<Func<TCapability, TProperty>> expression) {
        return new GroupPropertyBuilder<TCapability, TProperty>(this, expression);
    }

    /// <summary>
    /// Adds a nested AND group.
    /// </summary>
    /// <param name="name">The optional group name.</param>
    /// <param name="build">The group builder callback.</param>
    /// <param name="alias">The optional group alias.</param>
    /// <returns>The current group builder.</returns>
    public GroupBuilder<TCapability> And(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        ArgumentNullException.ThrowIfNull(build);
        ArgumentValidation.ThrowIfWhiteSpace(name, nameof(name));
        ArgumentValidation.ThrowIfWhiteSpace(alias, nameof(alias));

        var group = new RuleGroup<TCapability>(LogicalOperator.And, name, alias);
        build(new GroupBuilder<TCapability>(group));
        ThrowIfEmptyGroup(group, name);
        return AddRule(group);
    }

    /// <summary>
    /// Adds a nested OR group.
    /// </summary>
    /// <param name="name">The optional group name.</param>
    /// <param name="build">The group builder callback.</param>
    /// <param name="alias">The optional group alias.</param>
    /// <returns>The current group builder.</returns>
    public GroupBuilder<TCapability> Or(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        ArgumentNullException.ThrowIfNull(build);
        ArgumentValidation.ThrowIfWhiteSpace(name, nameof(name));
        ArgumentValidation.ThrowIfWhiteSpace(alias, nameof(alias));

        var group = new RuleGroup<TCapability>(LogicalOperator.Or, name, alias);
        build(new GroupBuilder<TCapability>(group));
        ThrowIfEmptyGroup(group, name);
        return AddRule(group);
    }

    private static void ThrowIfEmptyGroup(RuleGroup<TCapability> group, string? name) {
        if (group.RuleCount == 0) {
            throw new InvalidOperationException(name is null
                ? "Group must contain at least one rule."
                : $"Group '{name}' must contain at least one rule.");
        }
    }
}
