using ReqCap.Abstractions;

namespace ReqCap.Tests.Fixtures;

internal sealed class BooleanCapability : ICapability {
    public bool Value { get; init; }
}
