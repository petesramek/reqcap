using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;

namespace ReqCap.Tests.Unit.Builder;

public class BuilderChainingTests
{
    private sealed class CompositeCapability : ICapability
    {
        public decimal Volume { get; init; }

        public string Material { get; init; } = string.Empty;

        public bool Enabled { get; init; }
    }

    private sealed class AlwaysPassRule : IRule<CompositeCapability>
    {
        public EvaluationResult Evaluate(CompositeCapability capability)
        {
            return EvaluationResult.Ok();
        }
    }

    private sealed class AlwaysFailRule : IRule<CompositeCapability>
    {
        public EvaluationResult Evaluate(CompositeCapability capability)
        {
            return new EvaluationResult
            {
                Allowed = false,
                Errors =
                [
                    new Issue
                    {
                        Property = string.Empty,
                        Message = "Failed.",
                        Severity = RequirementSeverity.Error,
                        RuleName = "AlwaysFail",
                    },
                ],
            };
        }
    }

    [Fact]
    public void Build_WhenRootPropertyRulesAreChained_EvaluatesAllRules()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .Property(x => x.Volume)
            .GreaterOrEqual(10m)
            .AsError("MinimumVolume")
            .Property(x => x.Enabled)
            .Equal(true)
            .AsError("RequiredEnabled")
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 5m,
                Enabled = false,
                Material = "Plastic",
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(x => x.RuleName == "MinimumVolume");
        result.Errors.Should().Contain(x => x.RuleName == "RequiredEnabled");
    }

    [Fact]
    public void Build_WhenGroupPropertyRulesAreChained_EvaluatesAllRules()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("Root", group =>
            {
                group
                    .Property(x => x.Volume)
                    .GreaterOrEqual(10m)
                    .AsError("MinimumVolume")
                    .Property(x => x.Enabled)
                    .Equal(true)
                    .AsError("RequiredEnabled");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 5m,
                Enabled = false,
                Material = "Plastic",
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "Root");
    }

    [Fact]
    public void Build_WhenRootPredicateRulesAreChained_EvaluatesAllRules()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .Rule(
                "Enabled",
                capability => capability.Enabled,
                RequirementSeverity.Error)
            .Rule(
                "PlasticMaterial",
                capability => capability.Material == "Plastic",
                RequirementSeverity.Error)
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 10m,
                Enabled = false,
                Material = "Metal",
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(x => x.RuleName == "Enabled");
        result.Errors.Should().Contain(x => x.RuleName == "PlasticMaterial");
    }

    [Fact]
    public void Build_WhenGroupPredicateRulesAreChained_EvaluatesAllRules()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("Root", group =>
            {
                group
                    .Rule(
                        "Enabled",
                        capability => capability.Enabled,
                        RequirementSeverity.Error)
                    .Rule(
                        "PlasticMaterial",
                        capability => capability.Material == "Plastic",
                        RequirementSeverity.Error);
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability
            {
                Volume = 10m,
                Enabled = false,
                Material = "Metal",
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().OnlyContain(x => x.GroupName == "Root");
    }

    [Fact]
    public void Build_WhenRootCustomRulesAreChained_EvaluatesAllRules()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .AddRule(new AlwaysPassRule())
            .AddRule(new AlwaysFailRule())
            .Build();

        var result = Evaluator.Evaluate(new CompositeCapability(), requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].RuleName.Should().Be("AlwaysFail");
    }

    [Fact]
    public void Build_WhenGroupCustomRulesAreChained_EvaluatesAllRules()
    {
        var requirement = Requirement
            .For<CompositeCapability>()
            .And("Root", group =>
            {
                group
                    .AddRule(new AlwaysPassRule())
                    .AddRule(new AlwaysFailRule());
            })
            .Build();

        var result = Evaluator.Evaluate(new CompositeCapability(), requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].GroupName.Should().Be("Root");
        result.Errors[0].RuleName.Should().Be("AlwaysFail");
    }

    [Fact]
    public void AddRule_WhenRootRuleIsNull_ThrowsArgumentNullException()
    {
        var builder = Requirement.For<CompositeCapability>();

        var act = () => builder.AddRule(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddRule_WhenGroupRuleIsNull_ThrowsArgumentNullException()
    {
        var act = () => Requirement
            .For<CompositeCapability>()
            .And("Root", group =>
            {
                group.AddRule(null!);
            })
            .Build();

        act.Should().Throw<ArgumentNullException>();
    }
}
