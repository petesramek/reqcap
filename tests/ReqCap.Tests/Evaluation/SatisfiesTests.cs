namespace ReqCap.Tests.Evaluation;

using FluentAssertions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Tests.Fixtures;

public class SatisfiesTests
{
    [Fact]
    public void Satisfies_WhenNoRuleMatches_ReturnsTrue()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .Build();

        var result = Evaluator.Satisfies(new ContainerCapability { Volume = 10m }, requirement);

        result.Should().BeTrue();
    }

    [Fact]
    public void Satisfies_WhenAnyRuleMatches_ReturnsFalse()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Material)
            .Null()
            .AsWarning("MaterialMissing")
            .Build();

        var result = Evaluator.Satisfies(new ContainerCapability { Material = null! }, requirement);

        result.Should().BeFalse();
    }
}
