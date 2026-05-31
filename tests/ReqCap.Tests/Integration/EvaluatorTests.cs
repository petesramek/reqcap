
using FluentAssertions;
using ReqCap.Evaluation;
using ReqCap.Builder;
using ReqCap.Abstractions;
using ReqCap.Models;
using Xunit;

namespace ReqCap.Tests.Integration;

public class EvaluatorTests
{
    internal class Cap : ICapability { }

    private sealed class PassRule : IRule<Cap>
    {
        public EvaluationResult Evaluate(Cap _) => EvaluationResult.Ok();
    }

    private sealed class FailRule : IRule<Cap>
    {
        public EvaluationResult Evaluate(Cap _) => new EvaluationResult
        {
            Allowed = false,
            Errors = new[] { new Issue { Property = "X", Message = "fail", Severity = RequirementSeverity.Error } }
        };
    }

    [Fact]
    public void Evaluate_AllRulesPass_ShouldAllow()
    {
        var req = new RequirementModel<Cap>(new IRule<Cap>[] { new PassRule(), new PassRule() });

        var result = Evaluator.Evaluate(new Cap(), req);

        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_AnyRuleFails_ShouldBlock()
    {
        var req = new RequirementModel<Cap>(new IRule<Cap>[] { new PassRule(), new FailRule() });

        var result = Evaluator.Evaluate(new Cap(), req);

        result.Allowed.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
    }
}
