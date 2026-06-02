using System.Linq.Expressions;
using FluentAssertions;
using ReqCap.Groups;
using ReqCap.Requirements;
using ReqCap.Results;
using ReqCap.Rules;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Validation;

public class ValidationTests
{
    [Fact]
    public void Rule_WhenNameIsWhitespace_ThrowsArgumentException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Rule(" ", x => true, RequirementSeverity.Error);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rule_WhenPredicateIsNull_ThrowsArgumentNullException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Rule("Invalid", null!, RequirementSeverity.Error);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Rule_WhenSeverityIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Rule("Invalid", x => true, (RequirementSeverity)999);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void And_WhenBuildIsNull_ThrowsArgumentNullException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .And("Group", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void And_WhenNameIsWhitespace_ThrowsArgumentException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .And(" ", group => group.Rule("Invalid", x => true, RequirementSeverity.Error));

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void And_WhenGroupIsEmpty_ThrowsInvalidOperationException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .And("Empty", group => { });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Or_WhenGroupIsEmpty_ThrowsInvalidOperationException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Or("Empty", group => { });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Property_WhenExpressionIsNull_ThrowsArgumentNullException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Property<decimal>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Property_WhenExpressionIsNotMemberPath_ThrowsArgumentException()
    {
        Expression<Func<ContainerCapability, decimal>> expression = x => x.Volume + 1m;

        var act = () => Requirement
            .For<ContainerCapability>()
            .Property(expression);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PropertyChain_WhenBuildIsCalledWithoutCondition_ThrowsInvalidOperationException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .Build();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PropertyChain_WhenNewPropertyIsStartedWithoutCondition_ThrowsInvalidOperationException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .Property(x => x.Material);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AsError_WhenNameIsWhitespace_ThrowsArgumentException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsError(" ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AsWarning_WhenAliasIsWhitespace_ThrowsArgumentException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume)
            .LessThan(7m)
            .AsWarning(alias: " ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Comparison_WhenExpectedValueIsNull_ThrowsArgumentNullException()
    {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Property(x => x.Material)
            .Equal(null!)
            .AsError("MaterialMissing");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RuleGroup_WhenOperatorIsInvalid_ThrowsArgumentOutOfRangeException()
    {
        var act = () => new RuleGroup<ContainerCapability>((LogicalOperator)999);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void RuleGroup_WhenEvaluatedWithoutRules_ThrowsInvalidOperationException()
    {
        var group = new RuleGroup<ContainerCapability>(LogicalOperator.And);

        var act = () => group.Evaluate(new ContainerCapability());

        act.Should().Throw<InvalidOperationException>();
    }
}
