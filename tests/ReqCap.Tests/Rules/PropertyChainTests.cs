using FluentAssertions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Rules;

public class PropertyChainTests
{
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

        result.Errors.Should().ContainSingle(x => x.RuleName == "MinimumVolume");
        result.Warnings.Should().ContainSingle(x => x.RuleName == "RecommendedVolume");
    }
}
