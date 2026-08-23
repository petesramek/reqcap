namespace ReqCap.Benchmarks;

using ReqCap.Abstractions;

public sealed class BenchmarkCapability : ICapability {
    public decimal Volume { get; init; }

    public decimal? OptionalVolume { get; init; }

    public string? Material { get; init; }

    public bool HasDrainage { get; init; }

    public int Index { get; init; }

    public NestedBenchmarkCapability? Nested { get; init; }
}

public sealed class NestedBenchmarkCapability {
    public string? Code { get; init; }
}
