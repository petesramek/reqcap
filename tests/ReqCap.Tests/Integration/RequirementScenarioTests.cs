using FluentAssertions;
using ReqCap.Evaluation;
using ReqCap.Requirements;
using ReqCap.Results;
using ReqCap.Tests.Fixtures;

namespace ReqCap.Tests.Integration;

public class RequirementScenarioTests {
    [Fact]
    public void Evaluate_WhenCompositeAndAllPass_ReturnsAllowed() {
        var requirement = Requirement.For<CompositeCapability>()
            .And("ContainerRules", group => {
                group.Property(x => x.Volume).GreaterOrEqual(10m).AsError("MinimumVolume");
                group.Property(x => x.Material).Equal("Plastic").AsError("RequiredMaterial");
                group.Property(x => x.Enabled).Equal(true).AsError("MustBeEnabled");
            })
            .Build();

        var result = Evaluator.Evaluate(
            new CompositeCapability { Volume = 10m, Material = "Plastic", Enabled = true },
            requirement);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenPredicateRuleFails_ReturnsError() {
        var requirement = Requirement.For<CompositeCapability>()
            .Rule("EnabledPlastic", x => x.Enabled && x.Material == "Plastic", RequirementSeverity.Error)
            .Build();

        var result = Evaluator.Evaluate(new CompositeCapability { Enabled = true, Material = "Metal" }, requirement);

        result.Allowed.Should().BeFalse();
    }
}
