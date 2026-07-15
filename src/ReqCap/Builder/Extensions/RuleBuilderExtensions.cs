namespace ReqCap.Builder;

using ReqCap.Abstractions;
using ReqCap.Results;
using ReqCap.Rules;

/// <summary>
/// Provides neutral result projection-oriented extensions for custom predicate rules.
/// </summary>
public static class RuleBuilderExtensions
{
    /// <summary>
    /// Adds a custom predicate requirement match.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <param name="builder">The requirement builder.</param>
    /// <param name="name">The match name.</param>
    /// <param name="predicate">The predicate that returns <see langword="true" /> when the match should be produced.</param>
    /// <param name="alias">An optional external alias for the match.</param>
    /// <param name="message">An optional match message.</param>
    /// <returns>The current requirement builder.</returns>
    public static RequirementBuilder<TCapability> Rule<TCapability>(
        this RequirementBuilder<TCapability> builder,
        string name,
        Func<TCapability, bool> predicate,
        string? alias = null,
        string? message = null)
        where TCapability : ICapability
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddRule(new PredicateRule<TCapability>(
            name,
            predicate,
            RequirementSeverity.Error,
            alias,
            message));
    }

    /// <summary>
    /// Adds a custom predicate requirement match after the current property-builder chain.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="builder">The requirement property builder.</param>
    /// <param name="name">The match name.</param>
    /// <param name="predicate">The predicate that returns <see langword="true" /> when the match should be produced.</param>
    /// <param name="alias">An optional external alias for the match.</param>
    /// <param name="message">An optional match message.</param>
    /// <returns>The parent requirement builder.</returns>
    public static RequirementBuilder<TCapability> Rule<TCapability, TProperty>(
        this RequirementPropertyBuilder<TCapability, TProperty> builder,
        string name,
        Func<TCapability, bool> predicate,
        string? alias = null,
        string? message = null)
        where TCapability : ICapability
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddRule(new PredicateRule<TCapability>(
            name,
            predicate,
            RequirementSeverity.Error,
            alias,
            message));
    }

    /// <summary>
    /// Adds a custom predicate requirement match inside a group.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <param name="builder">The group builder.</param>
    /// <param name="name">The match name.</param>
    /// <param name="predicate">The predicate that returns <see langword="true" /> when the match should be produced.</param>
    /// <param name="alias">An optional external alias for the match.</param>
    /// <param name="message">An optional match message.</param>
    /// <returns>The group builder.</returns>
    public static GroupBuilder<TCapability> Rule<TCapability>(
        this GroupBuilder<TCapability> builder,
        string name,
        Func<TCapability, bool> predicate,
        string? alias = null,
        string? message = null)
        where TCapability : ICapability
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddRule(new PredicateRule<TCapability>(
            name,
            predicate,
            RequirementSeverity.Error,
            alias,
            message));
    }

    /// <summary>
    /// Adds a custom predicate requirement match after the current group property-builder chain.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="builder">The group property builder.</param>
    /// <param name="name">The match name.</param>
    /// <param name="predicate">The predicate that returns <see langword="true" /> when the match should be produced.</param>
    /// <param name="alias">An optional external alias for the match.</param>
    /// <param name="message">An optional match message.</param>
    /// <returns>The parent group builder.</returns>
    public static GroupBuilder<TCapability> Rule<TCapability, TProperty>(
        this GroupPropertyBuilder<TCapability, TProperty> builder,
        string name,
        Func<TCapability, bool> predicate,
        string? alias = null,
        string? message = null)
        where TCapability : ICapability
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddRule(new PredicateRule<TCapability>(
            name,
            predicate,
            RequirementSeverity.Error,
            alias,
            message));
    }
}
