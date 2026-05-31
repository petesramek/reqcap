using ReqCap.Abstractions;

namespace ReqCap.Tests.Fixtures;

internal sealed class LocationCapability : ICapability {
    public Coordinate? Coordinate { get; init; }
}

internal sealed class Coordinate {
    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }
}
