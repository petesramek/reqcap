using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;
using Xunit;

namespace ReqCap.Tests.Integration;

public class NotRequirementScenarioTests
{
    private sealed class CompositeCapability : ICapability
    {
        public decimal Volume { get; init; }

        public string Material { get; init; } = string.Empty;

        public bool Enabled { get; init; }
    }

    [Fact]
    public void Evaluate_WhenNotInsideAndPasses_DoesNotBlockParent()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("Root", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterOrEqual(5m)
                    .AsError("MinimumVolume");

                group.Not("ForbiddenMaterial", not =>
                {
                    not.Property(x => x.Material)
                        .Equal("Metal")
                        .AsError("MaterialMustNotBeMetal");
                });
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 10m,
                Material = "Plastic",
                Enabled = true,
            },
            requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenNotInsideAndFails_BlocksParentWithNotGroupIssue()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("Root", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterOrEqual(5m)
                    .AsError("MinimumVolume");

                group.Not("ForbiddenMaterial", not =>
                {
                    not.Property(x => x.Material)
                        .Equal("Metal")
                        .AsError("MaterialMustNotBeMetal");
                }, alias: "material.forbidden");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 10m,
                Material = "Metal",
                Enabled = true,
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Property.Should().Be("ForbiddenMaterial");
        result.Errors[0].GroupName.Should().Be("ForbiddenMaterial");
        result.Errors[0].GroupAlias.Should().Be("material.forbidden");
    }

    [Fact]
    public void Evaluate_WhenNotInsideOrPasses_AllowsOrGroup()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .Or("AllowedByEitherVolumeOrNotForbidden", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterOrEqual(100m)
                    .AsError("VeryLargeVolume");

                group.Not("ForbiddenMaterial", not =>
                {
                    not.Property(x => x.Material)
                        .Equal("Metal")
                        .AsError("MaterialMustNotBeMetal");
                });
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 10m,
                Material = "Plastic",
                Enabled = true,
            },
            requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenNotInsideOrFailsAndOtherBranchFails_ReturnsAllOrErrors()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .Or("AllowedByEitherVolumeOrNotForbidden", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterOrEqual(100m)
                    .AsError("VeryLargeVolume");

                group.Not("ForbiddenMaterial", not =>
                {
                    not.Property(x => x.Material)
                        .Equal("Metal")
                        .AsError("MaterialMustNotBeMetal");
                });
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 10m,
                Material = "Metal",
                Enabled = true,
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(x => x.RuleName == "VeryLargeVolume");
        result.Errors.Should().Contain(x => x.GroupName == "ForbiddenMaterial");
    }

    [Fact]
    public void Evaluate_WhenNotChildFailsWithWarning_ReturnsAllowedAndDiscardsChildWarning()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .Not("NotRecommendedMaterial", not =>
            {
                not.Property(x => x.Material)
                    .Equal("Ceramic")
                    .AsWarning("RecommendedCeramic");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 10m,
                Material = "Plastic",
                Enabled = true,
            },
            requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }
}
