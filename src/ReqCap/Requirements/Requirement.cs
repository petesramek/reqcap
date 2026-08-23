namespace ReqCap.Requirements;

using ReqCap.Abstractions;
using ReqCap.Builder;

/// <summary>
/// Provides the non-generic base type for requirements and exposes the entry point for creating builders.
/// </summary>
public abstract class Requirement {
    /// <summary>
    /// Creates a requirement builder for a capability type.
    /// </summary>
    /// <typeparam name="TCapability">The capability type for which requirements are built.</typeparam>
    /// <returns>A strongly typed requirement builder.</returns>
    public static RequirementBuilder<TCapability> For<TCapability>()
        where TCapability : ICapability {
        return new RequirementBuilder<TCapability>();
    }
}
