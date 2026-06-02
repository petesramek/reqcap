using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Rules;

public class NullActualComparisonTests
{
    private sealed class NullableCapability : ICapability
    {
        public decimal? Volume { get; init; }
    }

    [Fact]
    public void Evaluate_WhenReferencePropertyIsNullAndComparedForEquality_ReturnsAllowed()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Material)
            .Equal("Metal")
            .AsError("AvoidMetal")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Material = null! }, requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenNullableValuePropertyIsNullAndComparedWithOrderedOperator_ReturnsAllowed()
    {
        var requirement = Requirement
            .For<NullableCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .Build();

        var result = Evaluator.Evaluate(new NullableCapability { Volume = null }, requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }
}
