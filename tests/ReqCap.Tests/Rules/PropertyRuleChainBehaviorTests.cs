namespace ReqCap.Tests.Rules;

using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Rules;
using ReqCap.Tests.Fixtures;

public class PropertyRuleChainBehaviorTests {
    private sealed class NestedCapability : ICapability {
        public NestedValue? Child { get; init; }
    }

    private sealed class NestedValue {
        public decimal Value { get; init; }
    }

    [Fact]
    public void Evaluate_WhenPropertyRuleChainHasNoConditions_ReturnsAllowed() {
        var chain = new PropertyRuleChain<ContainerCapability, decimal>(x => x.Volume);

        var result = chain.Evaluate(new ContainerCapability { Volume = 5m });

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenNestedPropertyPathContainsNull_ReturnsIssueForPropertyPath() {
        var requirement = Requirement
            .For<NestedCapability>()
            .Property(x => x.Child!.Value)
            .LessThan(10m)
            .AsError("NestedValueMissing")
            .Build();

        var result = Evaluator.Evaluate(new NestedCapability { Child = null }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(issue =>
            issue.Property == "Child.Value" &&
            issue.RuleName == "NestedValueMissing" &&
            issue.Message.Contains("could not be evaluated", StringComparison.Ordinal));
    }

    [Fact]
    public void Evaluate_WhenNoPropertyConditionMatches_ReturnsAllowed() {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError("MinimumVolume")
            .GreaterThan(20m)
            .AsWarning("MaximumVolume")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 10m }, requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.Warnings.Should().BeEmpty();
    }
}
