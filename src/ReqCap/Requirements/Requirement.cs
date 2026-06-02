using ReqCap.Abstractions;
using ReqCap.Builder;

namespace ReqCap.Requirements;

/// <summary>
/// Provides the non-generic base type for requirements and exposes the entry point for creating builders.
/// </summary>
public abstract class Requirement
{
    /// <summary>
    /// Creates a requirement builder for a capability type.
    /// </summary>
    /// <typeparam name="TCapability">The capability type for which requirements are built.</typeparam>
    /// <returns>A strongly typed requirement builder.</returns>
    public static RequirementBuilder<TCapability> For<TCapability>()
        where TCapability : ICapability
    {
        return new RequirementBuilder<TCapability>();
    }
}

/// <summary>
/// Represents a collection of rules for a capability type.
/// </summary>
/// <typeparam name="TCapability">The capability type evaluated by the requirement.</typeparam>
public sealed class Requirement<TCapability> : Requirement
    where TCapability : ICapability
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Requirement{TCapability}" /> class.
    /// </summary>
    /// <param name="rules">The rules in the requirement.</param>
    public Requirement(IEnumerable<IRule<TCapability>> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        Rules = rules.ToList();
    }

    /// <summary>
    /// Gets the rules in this requirement.
    /// </summary>
    public IReadOnlyList<IRule<TCapability>> Rules { get; }
}
