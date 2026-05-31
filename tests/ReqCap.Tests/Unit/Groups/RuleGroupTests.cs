
using FluentAssertions;
using ReqCap.Groups;
using ReqCap.Models;
using ReqCap.Abstractions;
using Xunit;

namespace ReqCap.Tests.Unit.Groups;

public class RuleGroupTests
{
    internal class Cap : ICapability { }

    private sealed class PassRule : IRule<Cap>
    {
        public EvaluationResult Evaluate(Cap _) => EvaluationResult.Ok();
    }

    private sealed class FailRule : IRule<Cap>
    {
        public EvaluationResult Evaluate(Cap _) => new EvaluationResult
        { Allowed = false, Errors = new[] { new Issue { Property="x", Message="fail", Severity=RequirementSeverity.Error } } };
    }

    [Fact]
    public void Evaluate_And_AllPass_ShouldAllow()
    {
        var g = new RuleGroup<Cap>(LogicalOperator.And);
        g.Add(new PassRule());
        g.Add(new PassRule());

        var result = g.Evaluate(new Cap());

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_Or_OnePass_ShouldAllow()
    {
        var g = new RuleGroup<Cap>(LogicalOperator.Or);
        g.Add(new FailRule());
        g.Add(new PassRule());

        var result = g.Evaluate(new Cap());

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_Not_Inverts()
    {
        var g = new RuleGroup<Cap>(LogicalOperator.Not);
        g.Add(new PassRule());

        var result = g.Evaluate(new Cap());

        result.Allowed.Should().BeFalse();
    }
}
