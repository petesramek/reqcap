using FluentAssertions;
using ReqCap.Abstractions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Rules;

public class NullConditionTests
{
    private sealed class NullableCapability : ICapability
    {
        public decimal? Volume { get; init; }

        public object? Payload { get; init; }
    }

    [Fact]
    public void Evaluate_WhenReferencePropertyIsNull_ReturnsIssue()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Material)
            .Null()
            .AsError("MaterialRequired")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Material = null! }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(issue => issue.RuleName == "MaterialRequired");
    }

    [Fact]
    public void Evaluate_WhenReferencePropertyIsNotNull_ReturnsAllowed()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Material)
            .Null()
            .AsError("MaterialRequired")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Material = "Plastic" }, requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenNullableValuePropertyIsNull_ReturnsIssue()
    {
        var requirement = Requirement
            .For<NullableCapability>()
            .Property(x => x.Volume)
            .Null()
            .AsError("VolumeRequired")
            .Build();

        var result = Evaluator.Evaluate(new NullableCapability { Volume = null }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(issue => issue.RuleName == "VolumeRequired");
    }

    [Fact]
    public void Evaluate_WhenNullableValuePropertyHasValue_ReturnsAllowed()
    {
        var requirement = Requirement
            .For<NullableCapability>()
            .Property(x => x.Volume)
            .Null()
            .AsError("VolumeRequired")
            .Build();

        var result = Evaluator.Evaluate(new NullableCapability { Volume = 7m }, requirement);

        result.Allowed.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenObjectPropertyIsNull_ReturnsIssue()
    {
        var requirement = Requirement
            .For<NullableCapability>()
            .Property(x => x.Payload)
            .Null()
            .AsError("PayloadRequired")
            .Build();

        var result = Evaluator.Evaluate(new NullableCapability { Payload = null }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(issue => issue.RuleName == "PayloadRequired");
    }

    [Fact]
    public void Evaluate_WhenNullConditionMatchesInPropertyChain_ReturnsOnlyFirstIssue()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Material)
            .Null()
            .AsError("MaterialRequired")
            .Equal("Metal")
            .AsWarning("AvoidMetal")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Material = null! }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(issue => issue.RuleName == "MaterialRequired");
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void Evaluate_WhenNullConditionIsInsideGroup_ReturnsGroupedIssue()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("MaterialRules", group =>
            {
                group.Property(x => x.Material)
                    .Null()
                    .AsError("MaterialRequired");
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Material = null! }, requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().ContainSingle(issue =>
            issue.RuleName == "MaterialRequired" &&
            issue.GroupName == "MaterialRules");
    }

    [Fact]
    public void Evaluate_WhenNullConditionIsWarning_ReturnsWarning()
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Material)
            .Null()
            .AsWarning("MaterialMissing")
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Material = null! }, requirement);

        result.Allowed.Should().BeTrue();
        result.Warnings.Should().ContainSingle(issue => issue.RuleName == "MaterialMissing");
    }

    [Fact]
    public void Equal_WhenExpectedValueIsNull_ThrowsWithNullConditionGuidance()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Property(x => x.Material)
            .Equal(null!)
            .AsError("MaterialRequired");

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("*Use Null() to match null property values.*");
    }

    [Fact]
    public void LessThan_WhenPropertyDoesNotSupportComparison_ThrowsInvalidOperationException()
    {
        var act = () => Requirement
            .For<NullableCapability>()
            .Property(x => x.Payload)
            .LessThan(new object())
            .AsError("PayloadIssue");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*does not support comparison*");
    }
}
