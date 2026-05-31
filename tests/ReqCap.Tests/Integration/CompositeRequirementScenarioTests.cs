using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;
using Xunit;

namespace ReqCap.Tests.Integration;

public class CompositeRequirementScenarioTests
{
    private sealed class CompositeCapability : ICapability
    {
        public decimal Volume { get; init; }

        public string Material { get; init; } = string.Empty;

        public bool Enabled { get; init; }
    }

    [Fact]
    public void Evaluate_WhenAndGroupAllRulesPass_ReturnsAllowed()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("ContainerRules", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterOrEqual(10m)
                    .AsError("MinimumVolume");

                group.Property(x => x.Material)
                    .Equal("Plastic")
                    .AsError("RequiredMaterial");

                group.Property(x => x.Enabled)
                    .Equal(true)
                    .AsError("MustBeEnabled");
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
    public void Evaluate_WhenAndGroupMultipleRulesFail_ReturnsAllErrors()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("ContainerRules", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterOrEqual(10m)
                    .AsError("MinimumVolume");

                group.Property(x => x.Material)
                    .Equal("Plastic")
                    .AsError("RequiredMaterial");

                group.Property(x => x.Enabled)
                    .Equal(true)
                    .AsError("MustBeEnabled");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 5m,
                Material = "Metal",
                Enabled = false,
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().OnlyContain(x => x.GroupName == "ContainerRules");
    }

    [Fact]
    public void Evaluate_WhenOrGroupFirstAlternativePasses_ReturnsAllowedWithoutFailedAlternativeErrors()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .Or("AllowedContainer", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterOrEqual(10m)
                    .AsError("MinimumVolume");

                group.Property(x => x.Material)
                    .Equal("Plastic")
                    .AsError("RequiredMaterial");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 12m,
                Material = "Metal",
                Enabled = false,
            },
            requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenOrGroupAllAlternativesFail_ReturnsAllErrors()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .Or("AllowedContainer", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterOrEqual(10m)
                    .AsError("MinimumVolume");

                group.Property(x => x.Material)
                    .Equal("Plastic")
                    .AsError("RequiredMaterial");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 5m,
                Material = "Metal",
                Enabled = false,
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "AllowedContainer");
    }

    [Fact]
    public void Evaluate_WhenNestedOrInsideAndPasses_ReturnsAllowed()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("Root", root =>
            {
                root.Property(x => x.Volume)
                    .GreaterOrEqual(10m)
                    .AsError("MinimumVolume");

                root.Or("AllowedMaterials", material =>
                {
                    material.Property(x => x.Material)
                        .Equal("Plastic")
                        .AsError("PlasticAllowed");

                    material.Property(x => x.Material)
                        .Equal("Ceramic")
                        .AsError("CeramicAllowed");
                });
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 10m,
                Material = "Ceramic",
                Enabled = false,
            },
            requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenNestedOrInsideAndFails_ReturnsNestedErrors()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("Root", root =>
            {
                root.Property(x => x.Volume)
                    .GreaterOrEqual(10m)
                    .AsError("MinimumVolume");

                root.Or("AllowedMaterials", material =>
                {
                    material.Property(x => x.Material)
                        .Equal("Plastic")
                        .AsError("PlasticAllowed");

                    material.Property(x => x.Material)
                        .Equal("Ceramic")
                        .AsError("CeramicAllowed");
                });
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 10m,
                Material = "Metal",
                Enabled = false,
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "AllowedMaterials");
    }

    [Fact]
    public void Evaluate_WhenWarningsOnlyFail_ReturnsAllowedWithWarnings()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("Recommendations", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterOrEqual(10m)
                    .AsWarning("RecommendedVolume");

                group.Property(x => x.Enabled)
                    .Equal(true)
                    .AsWarning("RecommendedEnabled");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 5m,
                Material = "Plastic",
                Enabled = false,
            },
            requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().HaveCount(2);
        result.Warnings.Should().OnlyContain(x => x.GroupName == "Recommendations");
    }

    [Fact]
    public void Evaluate_WhenErrorAndWarningFail_ReturnsBlockedWithBoth()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("Mixed", group =>
            {
                group.Property(x => x.Volume)
                    .GreaterOrEqual(10m)
                    .AsWarning("RecommendedVolume");

                group.Property(x => x.Enabled)
                    .Equal(true)
                    .AsError("RequiredEnabled");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 5m,
                Material = "Plastic",
                Enabled = false,
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Warnings.Should().ContainSingle();
        result.Errors[0].GroupName.Should().Be("Mixed");
        result.Warnings[0].GroupName.Should().Be("Mixed");
    }
}
