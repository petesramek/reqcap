namespace ReqCap.Tests.Evaluation;

using FluentAssertions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Tests.Fixtures;

public class GenericEvaluationResultTests
{
    private sealed record Projection(string RuleName, string? PropertyPath, string? Message, string? GroupName);

    [Fact]
    public void Evaluate_WhenNoRuleMatches_ReturnsSatisfiedGenericResult()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .Build();

        var result = Evaluator.Evaluate(
            new ContainerCapability { Volume = 10m },
            requirement,
            match => new Projection(match.RuleName, match.PropertyPath, match.Message, match.GroupName));

        result.Satisfied.Should().BeTrue();
        result.Results.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenRuleMatches_ProjectsMatchIntoConsumerResult()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume", message: "Volume is too small.")
            .Build();

        var result = Evaluator.Evaluate(
            new ContainerCapability { Volume = 5m },
            requirement,
            match => new Projection(match.RuleName, match.PropertyPath, match.Message, match.GroupName));

        result.Satisfied.Should().BeFalse();
        result.Results.Should().ContainSingle().Which.Should().Be(new Projection(
            "MinimumVolume",
            "Volume",
            "Volume is too small.",
            null));
    }

    [Fact]
    public void Evaluate_WhenGroupedRuleMatches_ProjectsGroupMetadata()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("ContainerRules", group =>
            {
                group.Property(x => x.Material)
                    .Null()
                    .AsWarning("MaterialMissing");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new ContainerCapability { Material = null! },
            requirement,
            match => new Projection(match.RuleName, match.PropertyPath, match.Message, match.GroupName));

        result.Satisfied.Should().BeFalse();
        result.Results.Should().ContainSingle().Which.Should().Be(new Projection(
            "MaterialMissing",
            "Material",
            "Material matched condition Null.",
            "ContainerRules"));
    }
}
