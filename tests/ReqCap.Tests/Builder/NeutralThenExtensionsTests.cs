namespace ReqCap.Tests.Builder;

using FluentAssertions;
using ReqCap.Builder;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Tests.Fixtures;

public class NeutralThenExtensionsTests
{
    private sealed record Projection(string RuleName, string? PropertyPath, string? Message, string? GroupName);

    [Fact]
    public void Evaluate_WhenRequirementPropertyConditionUsesThen_ProjectsNeutralMatch()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .Then("MinimumVolume", message: "Volume is too small.")
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
    public void Evaluate_WhenGroupPropertyConditionUsesThen_ProjectsNeutralMatchWithGroupMetadata()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("ContainerRules", group =>
            {
                group.Property(x => x.Material)
                    .Null()
                    .Then("MaterialMissing", message: "Material should be provided.");
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
            "Material should be provided.",
            "ContainerRules"));
    }

    [Fact]
    public void Satisfies_WhenConditionCompletedWithThenDoesNotMatch_ReturnsTrue()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .Then("MinimumVolume")
            .Build();

        var result = Evaluator.Satisfies(new ContainerCapability { Volume = 10m }, requirement);

        result.Should().BeTrue();
    }

    [Fact]
    public void Satisfies_WhenConditionCompletedWithThenMatches_ReturnsFalse()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .Then("MinimumVolume")
            .Build();

        var result = Evaluator.Satisfies(new ContainerCapability { Volume = 5m }, requirement);

        result.Should().BeFalse();
    }
}
