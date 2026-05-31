using FluentAssertions;
using ReqCap.Builder;
using ReqCap.Evaluation;
using Xunit;

namespace ReqCap.Tests.Integration;

public class NestedRequirementTests
{
    private sealed class Cap : ICapability
    {
        public int Value { get; init; }
        public int Temperature { get; init; }
    }

    [Fact]
    public void Evaluate_WithNestedOrAndNot_ReturnsExpectedResult()
    {
        var requirement = Requirement.For<Cap>()
            .Or("RootDecision", root =>
            {
                root.Property(x => x.Value)
                    .GreaterOrEqual(20)
                    .AsError("HighValue");

                root.And("SafeRange", safe =>
                {
                    safe.Property(x => x.Value)
                        .GreaterOrEqual(10)
                        .AsError("MinValue");

                    safe.Property(x => x.Temperature)
                        .GreaterOrEqual(5)
                        .AsError("MinTemperature");
                });

                root.Not("ForbiddenRange", forbidden =>
                {
                    forbidden.Property(x => x.Value)
                        .GreaterOrEqual(30)
                        .AsError("ForbiddenHigh");
                });
            })
            .Build();

        var result = Evaluator.Evaluate(
            new Cap
            {
                Value = 12,
                Temperature = 6
            },
            requirement);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WithNestedRequirementFailure_ReturnsErrors()
    {
        var requirement = Requirement.For<Cap>()
            .And("Root", root =>
            {
                root.Property(x => x.Value)
                    .GreaterOrEqual(10)
                    .AsError("MinValue");

                root.Property(x => x.Temperature)
                    .GreaterOrEqual(5)
                    .AsError("MinTemperature");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new Cap
            {
                Value = 9,
                Temperature = 4
            },
            requirement);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
    }
}
