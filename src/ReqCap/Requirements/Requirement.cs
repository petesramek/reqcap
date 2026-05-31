using ReqCap.Abstractions;
using ReqCap.Builder;

namespace ReqCap.Requirements;


/// <summary>
/// Provides the non-generic base type for all capability requirements and exposes
/// the entry point for creating requirement builders.
/// </summary>
/// <remarks>
/// Use <see cref="For{TCapability}"/> to create a strongly typed requirement builder
/// for a specific capability type.
/// </remarks>
public abstract class Requirement {
    /// <summary>
    /// Creates a requirement builder for a capability type.
    /// </summary>
    /// <typeparam name="TCapability">The capability type.</typeparam>
    /// <returns>A requirement builder.</returns>
    public static RequirementBuilder<TCapability> For<TCapability>()
        where TCapability : ICapability {
        return new RequirementBuilder<TCapability>();
    }
}

/// <summary>
/// Represents a collection of rules for a capability type.
/// </summary>
/// <typeparam name="TCapability">The capability type evaluated by the requirement.</typeparam>
public sealed class Requirement<TCapability> : Requirement
    where TCapability : ICapability {
    /// <summary>
    /// Initializes a new instance of the <see cref="Requirement{TCapability}"/> class.
    /// </summary>
    /// <param name="rules">The rules in the requirement.</param>
    public Requirement(IEnumerable<IRule<TCapability>> rules) {
        ArgumentNullException.ThrowIfNull(rules);
        Rules = rules.ToList();
    }

    /// <summary>Gets the rules in this requirement.</summary>
    public IReadOnlyList<IRule<TCapability>> Rules { get; }
}
