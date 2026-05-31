using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Groups;
using ReqCap.Results;
using Xunit;

namespace ReqCap.Tests.Unit.Groups;

public class RuleGroupNotSemanticsTests
{
    private sealed class TestCapability : ICapability
    {
        public string Material { get; init; } = string.Empty;
    }

    private sealed class PassRule : IRule<TestCapability>
    {
        public EvaluationResult Evaluate(TestCapability capability)
        {
            return EvaluationResult.Ok();
        }
    }

    private sealed class FailRule : IRule<TestCapability>
    {
        public EvaluationResult Evaluate(TestCapability capability)
        {
            return new EvaluationResult
            {
                Allowed = false,
                Errors =
                [
                    new Issue
                    {
                        Property = nameof(TestCapability.Material),
                        Message = "Material is not allowed.",
                        Severity = RequirementSeverity.Error,
                        RuleName = "ForbiddenMaterial",
                    },
                ],
            };
        }
    }

    [Fact]
    public void Evaluate_WhenNotChildFails_ReturnsAllowedWithoutChildErrors()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.Not, "NotForbiddenMaterial");
        group.Add(new FailRule());

        var result = group.Evaluate(new TestCapability { Material = "Plastic" });

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenNotChildPasses_ReturnsBlockedWithGroupIssue()
    {
        var group = new RuleGroup<TestCapability>(
            LogicalOperator.Not,
            name: "ForbiddenMaterial",
            alias: "material.forbidden");

        group.Add(new PassRule());

        var result = group.Evaluate(new TestCapability { Material = "Metal" });

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Property.Should().Be("ForbiddenMaterial");
        result.Errors[0].GroupName.Should().Be("ForbiddenMaterial");
        result.Errors[0].GroupAlias.Should().Be("material.forbidden");
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenNotHasNoChildren_ThrowsInvalidOperationException()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.Not);

        var act = () => group.Evaluate(new TestCapability());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Evaluate_WhenNotHasMultipleChildren_ThrowsInvalidOperationException()
    {
        var group = new RuleGroup<TestCapability>(LogicalOperator.Not);
        group.Add(new PassRule());
        group.Add(new FailRule());

        var act = () => group.Evaluate(new TestCapability());

        act.Should().Throw<InvalidOperationException>();
    }
}
