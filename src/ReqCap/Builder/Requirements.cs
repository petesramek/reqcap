using ReqCap.Abstractions;

namespace ReqCap.Builder;

/// <summary>
/// Entry point for building requirements.
/// </summary>
public static class Requirements {
    /// <summary>
    /// Creates a requirement builder for a capability type.
    /// </summary>
    public static RequirementBuilder<TCapability> For<TCapability>()
        where TCapability : ICapability {
        return new RequirementBuilder<TCapability>();
    }
}
