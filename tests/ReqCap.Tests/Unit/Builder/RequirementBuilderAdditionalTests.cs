using FluentAssertions;
using ReqCap.Builder;
using ReqCap.Evaluation;
using Xunit;

namespace ReqCap.Tests.Unit.Builder;

public class RequirementBuilderAdditionalTests
{
    private sealed class Cap : ICapability
    {
        public int Value { get; init; }
    }

    [Fact]
    public void Build_WithRootPropertyRule_AddsRule()
    {
        var builder = Requirement.For<Cap>();

        builder.Property(x => x.Value)
            .GreaterOrEqual(10)
            .AsError("MinValue", "cap.value.min");

        var requirement = builder.Build();

        requirement.Rules.Should().ContainSingle();
    }

    [Fact]
    public void Build_WithRootOrGroup_AddsGroup()
    {
        var requirement = Requirement.For<Cap>()
            .Or("RootOr", g =>
            {
                g.Property(x => x.Value)
                    .GreaterOrEqual(10)
                    .AsError();
            })
            .Build();

        requirement.Rules.Should().ContainSingle();
    }

    [Fact]
    public void Build_WithRootNotGroup_AddsGroup()
    {
        var requirement = Requirement.For<Cap>()
            .Not("RootNot", g =>
            {
                g.Property(x => x.Value)
                    .GreaterOrEqual(10)
                    .AsError();
            })
            .Build();

        requirement.Rules.Should().ContainSingle();
    }

    [Fact]
    public void Build_WithNestedAndGroup_EvaluatesCorrectly()
    {
        var requirement = Requirement.For<Cap>()
            .Or("RootOr", g =>
            {
                g.And("NestedAnd", inner =>
                {
                    inner.Property(x => x.Value)
                        .GreaterOrEqual(10)
                        .AsError();
                });
            })
            .Build();

        var result = Evaluator.Evaluate(new Cap { Value = 10 }, requirement);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Build_WithNestedOrGroup_EvaluatesCorrectly()
    {
        var requirement = Requirement.For<Cap>()
            .And("RootAnd", g =>
            {
                g.Or("NestedOr", inner =>
                {
                    inner.Property(x => x.Value)
                        .GreaterOrEqual(100)
                        .AsError();

                    inner.Property(x => x.Value)
                        .GreaterOrEqual(10)
                        .AsError();
                });
            })
            .Build();

        var result = Evaluator.Evaluate(new Cap { Value = 10 }, requirement);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Build_WithNestedNotGroup_EvaluatesCorrectly()
    {
        var requirement = Requirement.For<Cap>()
            .And("RootAnd", g =>
            {
                g.Not("NestedNot", inner =>
                {
                    inner.Property(x => x.Value)
                        .GreaterOrEqual(100)
                        .AsError();
                });
            })
            .Build();

        var result = Evaluator.Evaluate(new Cap { Value = 10 }, requirement);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Build_WithWarningRule_ReturnsWarning()
    {
        var builder = Requirement.For<Cap>();

        builder.Property(x => x.Value)
            .GreaterOrEqual(10)
            .AsWarning("RecommendedValue", "cap.value.recommended");

        var requirement = builder.Build();

        var result = Evaluator.Evaluate(new Cap { Value = 5 }, requirement);

        result.Allowed.Should().BeTrue();
        result.Warnings.Should().ContainSingle();
        result.Warnings[0].RuleName.Should().Be("RecommendedValue");
        result.Warnings[0].RuleAlias.Should().Be("cap.value.recommended");
    }
}
