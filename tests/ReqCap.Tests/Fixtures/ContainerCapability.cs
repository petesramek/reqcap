using ReqCap.Abstractions;

namespace ReqCap.Tests.Fixtures;

internal sealed class ContainerCapability : ICapability
{
    public decimal Volume { get; init; }

    public string Material { get; init; } = string.Empty;

    public bool HasDrainage { get; init; }
}
