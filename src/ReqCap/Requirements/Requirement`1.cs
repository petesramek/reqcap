namespace ReqCap.Requirements;

using ReqCap.Abstractions;

/// <summary>
/// Represents a collection of rules for a capability type.
/// </summary>
/// <typeparam name="TCapability">The capability type evaluated by the requirement.</typeparam>
public sealed class Requirement<TCapability> : Requirement
    where TCapability : ICapability {
    /// <summary>
    /// Initializes a new instance of the <see cref="Requirement{TCapability}" /> class.
    /// </summary>
    /// <param name="rules">The rules in the requirement.</param>
    public Requirement(IEnumerable<IRule<TCapability>> rules) {
        ArgumentNullException.ThrowIfNull(rules);
        Rules = rules.ToList();
    }

    /// <summary>
    /// Gets the rules in this requirement.
    /// </summary>
    public IReadOnlyList<IRule<TCapability>> Rules { get; }
}
