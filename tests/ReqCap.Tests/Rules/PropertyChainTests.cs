using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;

namespace ReqCap.Tests.Rules;

public class PropertyChainTests
{
    private sealed class ContainerCapability : ICapability
    {
        public decimal Volume { get; init; }

        public string Material { get; init; } = string.Empty;
    }

    [Fact]
    public void Evaluate_WhenFirstConditionInPropertyChainMatches_ReturnsOnlyFirstIssue()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .LessThan(10m)
            .AsWarning("RecommendedVolume")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 5m }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.RuleName == "MinimumVolume");
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenFirstConditionDoesNotMatchButSecondMatches_ReturnsSecondIssue()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .LessThan(10m)
            .AsWarning("RecommendedVolume")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 8m }, requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().ContainSingle(x => x.RuleName == "RecommendedVolume");
    }

    [Fact]
    public void Evaluate_WhenSamePropertyIsDeclaredSeparately_ReturnsBothIndependentIssues()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .Property(x => x.Volume)
            .LessThan(10m)
            .AsWarning("RecommendedVolume")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 5m }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.RuleName == "MinimumVolume");
        result.Warnings.Should().ContainSingle(x => x.RuleName == "RecommendedVolume");
    }

    [Fact]
    public void Evaluate_WhenDifferentPropertyAlsoMatches_ReturnsPropertyChainIssueAndOtherPropertyIssue()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .LessThan(10m)
            .AsWarning("RecommendedVolume")
            .Property(x => x.Material)
            .Equal("Metal")
            .AsWarning("AvoidMetal")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 5m, Material = "Metal" }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.RuleName == "MinimumVolume");
        result.Warnings.Should().ContainSingle(x => x.RuleName == "AvoidMetal");
    }
}
