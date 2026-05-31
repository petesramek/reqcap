
using ReqCap.Builder;
using FluentAssertions;
using Xunit;

namespace ReqCap.Tests.Unit.Builder;

public class RequirementBuilderTests
{
    private class Cap : ICapability { public int Value { get; set; } }

    [Fact]
    public void Build_WithSimpleRule_ShouldAddRule()
    {
        var req = Requirement.For<Cap>()
            .And("root", g =>
            {
                g.Property(x => x.Value)
                 .GreaterOrEqual(10)
                 .AsError("min");
            })
            .Build();

        req.Rules.Should().HaveCount(1);
    }
}
