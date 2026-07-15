namespace ReqCap.Tests.Builder;

using FluentAssertions;
using ReqCap.Builder;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Tests.Fixtures;

public class NeutralRuleExtensionsTests
{
    private sealed record Projection(string RuleName, string? PropertyPath, string? Message, string? GroupName);

    [Fact]
    public void Evaluate_WhenRequirementRuleMatches_ProjectsNeutralMatch()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Rule("DrainageMissing", x => !x.HasDrainage, message: "Drainage should be provided.")
            .Build();

        var result = Evaluator.Evaluate(
            new ContainerCapability { HasDrainage = false },
            requirement,
            match => new Projection(match.RuleName, match.PropertyPath, match.Message, match.GroupName));

        result.Satisfied.Should().BeFalse();
        result.Results.Should().ContainSingle().Which.Should().Be(new Projection(
            "DrainageMissing",
            null,
            "Drainage should be provided.",
            null));
    }

    [Fact]
    public void Evaluate_WhenRequirementPropertyBuilderRuleMatches_ProjectsNeutralMatch()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .GreaterThan(100m)
            .Then("LargeVolume")
            .Rule("DrainageMissing", x => !x.HasDrainage, message: "Drainage should be provided.")
            .Build();

        var result = Evaluator.Evaluate(
            new ContainerCapability { Volume = 50m, HasDrainage = false },
            requirement,
            match => new Projection(match.RuleName, match.PropertyPath, match.Message, match.GroupName));

        result.Satisfied.Should().BeFalse();
        result.Results.Should().ContainSingle().Which.Should().Be(new Projection(
            "DrainageMissing",
            null,
            "Drainage should be provided.",
            null));
    }

    [Fact]
    public void Evaluate_WhenGroupRuleMatches_ProjectsNeutralMatchWithGroupMetadata()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("ContainerRules", group =>
            {
                group.Rule("DrainageMissing", x => !x.HasDrainage, message: "Drainage should be provided.");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new ContainerCapability { HasDrainage = false },
            requirement,
            match => new Projection(match.RuleName, match.PropertyPath, match.Message, match.GroupName));

        result.Satisfied.Should().BeFalse();
        result.Results.Should().ContainSingle().Which.Should().Be(new Projection(
            "DrainageMissing",
            null,
            "Drainage should be provided.",
            "ContainerRules"));
    }

    [Fact]
    public void Evaluate_WhenGroupPropertyBuilderRuleMatches_ProjectsNeutralMatchWithGroupMetadata()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("ContainerRules", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterThan(100m)
                    .Then("LargeVolume")
                    .Rule("DrainageMissing", x => !x.HasDrainage, message: "Drainage should be provided.");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new ContainerCapability { Volume = 50m, HasDrainage = false },
            requirement,
            match => new Projection(match.RuleName, match.PropertyPath, match.Message, match.GroupName));

        result.Satisfied.Should().BeFalse();
        result.Results.Should().ContainSingle().Which.Should().Be(new Projection(
            "DrainageMissing",
            null,
            "Drainage should be provided.",
            "ContainerRules"));
    }
}
