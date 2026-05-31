using ReqCap.Abstractions;

namespace ReqCap.Tests.Fixtures;

internal sealed class CompositeCapability : ICapability {
    public decimal Volume { get; init; }
    public string Material { get; init; } = string.Empty;
    public bool Enabled { get; init; }
}
