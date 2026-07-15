namespace ReqCap.Builder;

using ReqCap.Abstractions;

/// <summary>
/// Provides neutral result projection-oriented extensions for severity builders.
/// </summary>
public static class SeverityBuilderExtensions
{
    /// <summary>
    /// Completes the condition as a neutral requirement match.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="builder">The severity builder.</param>
    /// <param name="name">The match name.</param>
    /// <param name="alias">An optional external alias for the match.</param>
    /// <param name="message">An optional match message.</param>
    /// <returns>The requirement property builder.</returns>
    public static RequirementPropertyBuilder<TCapability, TProperty> Then<TCapability, TProperty>(
        this RequirementSeverityBuilder<TCapability, TProperty> builder,
        string name,
        string? alias = null,
        string? message = null)
        where TCapability : ICapability
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AsError(name, alias, message);
    }

    /// <summary>
    /// Completes the condition as a neutral requirement match inside a group.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <typeparam name="TProperty">The property type.</typeparam>
    /// <param name="builder">The severity builder.</param>
    /// <param name="name">The match name.</param>
    /// <param name="alias">An optional external alias for the match.</param>
    /// <param name="message">An optional match message.</param>
    /// <returns>The group property builder.</returns>
    public static GroupPropertyBuilder<TCapability, TProperty> Then<TCapability, TProperty>(
        this GroupSeverityBuilder<TCapability, TProperty> builder,
        string name,
        string? alias = null,
        string? message = null)
        where TCapability : ICapability
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AsError(name, alias, message);
    }
}
