using FluentAssertions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;
using ReqCap.Rules;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Rules;

public class ComparisonOperatorBehaviorTests
{
    public static TheoryData<ComparisonOperator, decimal, decimal, bool> OperatorCases => new()
    {
        { ComparisonOperator.GreaterOrEqual, 10m, 10m, true },
        { ComparisonOperator.GreaterOrEqual, 9m, 10m, false },
        { ComparisonOperator.GreaterThan, 11m, 10m, true },
        { ComparisonOperator.GreaterThan, 10m, 10m, false },
        { ComparisonOperator.LessOrEqual, 10m, 10m, true },
        { ComparisonOperator.LessOrEqual, 11m, 10m, false },
        { ComparisonOperator.LessThan, 9m, 10m, true },
        { ComparisonOperator.LessThan, 10m, 10m, false },
        { ComparisonOperator.Equal, 10m, 10m, true },
        { ComparisonOperator.Equal, 11m, 10m, false },
        { ComparisonOperator.NotEqual, 11m, 10m, true },
        { ComparisonOperator.NotEqual, 10m, 10m, false },
    };

    [Theory]
    [MemberData(nameof(OperatorCases))]
    public void Evaluate_WhenOperatorIsUsedByComparisonRule_ReturnsExpectedIssueResult(
        ComparisonOperator op,
        decimal actual,
        decimal expected,
        bool issueExpected)
    {
        var rule = new ComparisonRule<ContainerCapability, decimal>(
            x => x.Volume,
            expected,
            op,
            RequirementSeverity.Error,
            "VolumeIssue");

        var result = rule.Evaluate(new ContainerCapability { Volume = actual });

        result.Allowed.Should().Be(!issueExpected);
        result.Errors.Count.Should().Be(issueExpected ? 1 : 0);
    }

    [Theory]
    [InlineData(ComparisonOperator.GreaterOrEqual)]
    [InlineData(ComparisonOperator.GreaterThan)]
    [InlineData(ComparisonOperator.LessOrEqual)]
    [InlineData(ComparisonOperator.NotEqual)]
    public void Evaluate_WhenRootPropertyBuilderOperatorIsUsed_CoversOperatorForwarding(ComparisonOperator op)
    {
        var requirement = BuildRootRequirement(op);

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 11m }, requirement);

        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(ComparisonOperator.GreaterOrEqual)]
    [InlineData(ComparisonOperator.GreaterThan)]
    [InlineData(ComparisonOperator.LessOrEqual)]
    public void Evaluate_WhenGroupPropertyBuilderOperatorIsUsed_CoversOperatorForwarding(ComparisonOperator op)
    {
        var requirement = Requirement
            .For<ContainerCapability>()
            .And("Root", group =>
            {
                AddGroupCondition(group, op);
            })
            .Build();

        var result = Evaluator.Evaluate(new ContainerCapability { Volume = 11m }, requirement);

        result.Should().NotBeNull();
    }

    private static ReqCap.Requirements.Requirement<ContainerCapability> BuildRootRequirement(ComparisonOperator op)
    {
        var property = Requirement
            .For<ContainerCapability>()
            .Property(x => x.Volume);

        return op switch
        {
            ComparisonOperator.GreaterOrEqual => property.GreaterOrEqual(10m).AsError("VolumeIssue").Build(),
            ComparisonOperator.GreaterThan => property.GreaterThan(10m).AsError("VolumeIssue").Build(),
            ComparisonOperator.LessOrEqual => property.LessOrEqual(10m).AsError("VolumeIssue").Build(),
            ComparisonOperator.NotEqual => property.NotEqual(10m).AsError("VolumeIssue").Build(),
            _ => property.Equal(10m).AsError("VolumeIssue").Build(),
        };
    }

    private static void AddGroupCondition(ReqCap.Builder.GroupBuilder<ContainerCapability> group, ComparisonOperator op)
    {
        var property = group.Property(x => x.Volume);

        _ = op switch
        {
            ComparisonOperator.GreaterOrEqual => property.GreaterOrEqual(10m).AsError("VolumeIssue"),
            ComparisonOperator.GreaterThan => property.GreaterThan(10m).AsError("VolumeIssue"),
            ComparisonOperator.LessOrEqual => property.LessOrEqual(10m).AsError("VolumeIssue"),
            _ => property.Equal(10m).AsError("VolumeIssue"),
        };
    }
}
