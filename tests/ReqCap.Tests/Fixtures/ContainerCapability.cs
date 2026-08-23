namespace ReqCap.Tests.Fixtures;

using ReqCap.Abstractions;

internal sealed class ContainerCapability : ICapability {
    public decimal Volume { get; init; }

    public string Material { get; init; } = string.Empty;

    public bool HasDrainage { get; init; }
}
