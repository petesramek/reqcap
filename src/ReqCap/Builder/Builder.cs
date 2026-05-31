
using System.Linq.Expressions;
using ReqCap.Groups;
using ReqCap.Rules;
using ReqCap.Abstractions;
using ReqCap.Models;

namespace ReqCap.Builder;

public static class Requirement {
    public static RequirementBuilder<TCapability> For<TCapability>()
        where TCapability : ICapability
        => new();

}

public sealed class RequirementBuilder<TCapability>
     where TCapability : ICapability {
    private readonly List<IRule<TCapability>> _rules = new();

    public PropertyBuilder<TCapability, TProp> Property<TProp>(Expression<Func<TCapability, TProp>> expr)
        where TProp : IComparable<TProp>
        => new(expr, Add);

    public RequirementBuilder<TCapability> And(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        var g = new RuleGroup<TCapability>(LogicalOperator.And, name, alias);
        build(new GroupBuilder<TCapability>(g));
        _rules.Add(g);
        return this;
    }

    public RequirementBuilder<TCapability> Or(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        var g = new RuleGroup<TCapability>(LogicalOperator.Or, name, alias);
        build(new GroupBuilder<TCapability>(g));
        _rules.Add(g);
        return this;
    }

    public RequirementBuilder<TCapability> Not(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        var outer = new RuleGroup<TCapability>(LogicalOperator.Not, name, alias);
        var inner = new RuleGroup<TCapability>(LogicalOperator.And);
        build(new GroupBuilder<TCapability>(inner));
        outer.Add(inner);
        _rules.Add(outer);
        return this;
    }

    public RequirementModel<TCapability> Build() {
        return new RequirementModel<TCapability>(_rules);
    }

    internal void Add(IRule<TCapability> rule) => _rules.Add(rule);
}

public sealed class GroupBuilder<TCapability>
    where TCapability : ICapability {
    private readonly RuleGroup<TCapability> _group;

    internal GroupBuilder(RuleGroup<TCapability> group) {
        _group = group;
    }

    public PropertyBuilder<TCapability, TProp> Property<TProp>(Expression<Func<TCapability, TProp>> expr)
        where TProp : IComparable<TProp>
        => new(expr, _group.Add);

    public void And(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        var g = new RuleGroup<TCapability>(LogicalOperator.And, name, alias);
        build(new GroupBuilder<TCapability>(g));
        _group.Add(g);
    }

    public void Or(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        var g = new RuleGroup<TCapability>(LogicalOperator.Or, name, alias);
        build(new GroupBuilder<TCapability>(g));
        _group.Add(g);
    }

    public void Not(string? name, Action<GroupBuilder<TCapability>> build, string? alias = null) {
        var outer = new RuleGroup<TCapability>(LogicalOperator.Not, name, alias);
        var inner = new RuleGroup<TCapability>(LogicalOperator.And);
        build(new GroupBuilder<TCapability>(inner));
        outer.Add(inner);
        _group.Add(outer);
    }
}

public sealed class PropertyBuilder<TCapability, TProp>
    where TCapability : ICapability
    where TProp : IComparable<TProp> {
    private readonly Expression<Func<TCapability, TProp>> _expr;
    private readonly Action<IRule<TCapability>> _add;

    internal PropertyBuilder(Expression<Func<TCapability, TProp>> expr, Action<IRule<TCapability>> add) {
        _expr = expr;
        _add = add;
    }

    public SeverityBuilder<TCapability, TProp> GreaterOrEqual(TProp value)
        => new(_expr, value, ComparisonOperator.GreaterOrEqual, _add);
}

public sealed class SeverityBuilder<TCapability, TProp>
    where TCapability : ICapability
    where TProp : IComparable<TProp> {
    private readonly Expression<Func<TCapability, TProp>> _expr;
    private readonly TProp _value;
    private readonly ComparisonOperator _op;
    private readonly Action<IRule<TCapability>> _add;

    internal SeverityBuilder(Expression<Func<TCapability, TProp>> expr, TProp value, ComparisonOperator op, Action<IRule<TCapability>> add) {
        _expr = expr;
        _value = value;
        _op = op;
        _add = add;
    }

    public void AsError(string? name = null, string? alias = null) {
        _add(new ComparisonRule<TCapability, TProp>(_expr, _value, _op, RequirementSeverity.Error, name, alias));
    }

    public void AsWarning(string? name = null, string? alias = null) {
        _add(new ComparisonRule<TCapability, TProp>(_expr, _value, _op, RequirementSeverity.Warning, name, alias));
    }
}

public sealed class RequirementModel<TCapability>
    where TCapability : ICapability {
    public IReadOnlyList<IRule<TCapability>> Rules { get; }

    public RequirementModel(IReadOnlyList<IRule<TCapability>> rules) {
        Rules = rules;
    }
}
