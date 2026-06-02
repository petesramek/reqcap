using FluentAssertions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Integration;

public class CompositeScenarioTests
{
    [Fact]
    public void Evaluate_WhenHardAndSoftVolumeRulesAreChained_ReturnsOnlyMostRelevantVolumeIssue()
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

        var result = Evaluator.Evaluate(new ContainerCapability
        {
            Volume = 5m,
            Material = "Metal",
            HasDrainage = true,
        }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.RuleName == "MinimumVolume");
        result.Warnings.Should().ContainSingle(x => x.RuleName == "AvoidMetal");
    }

    [Fact]
    public void Evaluate_WhenOrGroupUsesViolationRules_PassesWhenOneAlternativeDoesNotMatch()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Or("AllowedMaterials", group =>
            {
                group.Property(x => x.Material)
                    .NotEqual("Plastic")
                    .AsError("NotPlastic");

                group.Property(x => x.Material)
                    .NotEqual("Ceramic")
                    .AsError("NotCeramic");
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Material = "Plastic" }, requirement);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenOrGroupUsesViolationRules_FailsWhenAllAlternativesMatch()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Or("AllowedMaterials", group =>
            {
                group.Property(x => x.Material)
                    .NotEqual("Plastic")
                    .AsError("NotPlastic");

                group.Property(x => x.Material)
                    .NotEqual("Ceramic")
                    .AsError("NotCeramic");
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Material = "Metal" }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "AllowedMaterials");
    }

    [Fact]
    public void Evaluate_WhenPredicateMatches_ReturnsIssue()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Rule(
                "InvalidContainer",
                x => x.Volume < 7m || !x.HasDrainage,
                RequirementSeverity.Error)
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 8m, HasDrainage = false }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(x => x.RuleName == "InvalidContainer");
    }
}
