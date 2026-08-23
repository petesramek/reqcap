namespace ReqCap.Tests.Builder;

using FluentAssertions;
using ReqCap.Requirements;
using ReqCap.Tests.Fixtures;

public class UnnamedGroupValidationTests {
    [Fact]
    public void And_WhenRootGroupIsUnnamedAndEmpty_ThrowsGenericGroupMessage() {
        var act = () => Requirement
            .For<ContainerCapability>()
            .And(null, group => { });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Group must contain at least one rule.");
    }

    [Fact]
    public void Or_WhenRootGroupIsUnnamedAndEmpty_ThrowsGenericGroupMessage() {
        var act = () => Requirement
            .For<ContainerCapability>()
            .Or(null, group => { });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Group must contain at least one rule.");
    }

    [Fact]
    public void And_WhenNestedGroupIsUnnamedAndEmpty_ThrowsGenericGroupMessage() {
        var act = () => Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.And(null, nested => { });
            });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Group must contain at least one rule.");
    }

    [Fact]
    public void Or_WhenNestedGroupIsUnnamedAndEmpty_ThrowsGenericGroupMessage() {
        var act = () => Requirement
            .For<ContainerCapability>()
            .And("Root", group => {
                group.Or(null, nested => { });
            });

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Group must contain at least one rule.");
    }
}
