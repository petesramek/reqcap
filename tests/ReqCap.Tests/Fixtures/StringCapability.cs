using ReqCap.Abstractions;

namespace ReqCap.Tests.Fixtures;

internal sealed class StringCapability : ICapability {
    public string Value { get; init; } = string.Empty;
}
