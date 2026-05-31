using ReqCap.Abstractions;

namespace ReqCap.Tests.Fixtures;

internal sealed class DecimalCapability : ICapability {
    public decimal Value { get; init; }
}
