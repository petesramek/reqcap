using ReqCap.Abstractions;
using ReqCap.Results;
using ReqCap.Rules;

namespace ReqCap.Builder;

/// <summary>
/// Finalizes a requirement property issue condition by assigning severity and metadata.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
public sealed class RequirementSeverityBuilder<TCapability, TProperty>
    where TCapability : ICapability
    where TProperty : IComparable<TProperty>
{
    private readonly RequirementPropertyBuilder<TCapability, TProperty> _property;
    private readonly TProperty _value;
    private readonly ComparisonOperator _operator;

    internal RequirementSeverityBuilder(
        RequirementPropertyBuilder<TCapability, TProperty> property,
        TProperty value,
        ComparisonOperator op)
    {
        _property = property;
        _value = value;
        _operator = op;
    }

    /// <summary>
    /// Creates an error condition.
    /// </summary>
    /// <param name="name">The optional rule name.</param>
    /// <param name="alias">The optional rule alias.</param>
    /// <param name="message">The optional issue message.</param>
    /// <returns>The current property builder.</returns>
    public RequirementPropertyBuilder<TCapability, TProperty> AsError(
        string? name = null,
        string? alias = null,
        string? message = null)
    {
        return AddCondition(RequirementSeverity.Error, name, alias, message);
    }

    /// <summary>
    /// Creates a warning condition.
    /// </summary>
    /// <param name="name">The optional rule name.</param>
    /// <param name="alias">The optional rule alias.</param>
    /// <param name="message">The optional issue message.</param>
    /// <returns>The current property builder.</returns>
    public RequirementPropertyBuilder<TCapability, TProperty> AsWarning(
        string? name = null,
        string? alias = null,
        string? message = null)
    {
        return AddCondition(RequirementSeverity.Warning, name, alias, message);
    }

    private RequirementPropertyBuilder<TCapability, TProperty> AddCondition(
        RequirementSeverity severity,
        string? name,
        string? alias,
        string? message)
    {
        return _property.AddCondition(new PropertyCondition<TProperty>(
            _operator,
            _value,
            severity,
            name,
            alias,
            message));
    }
}

/// <summary>
/// Finalizes a group property issue condition by assigning severity and metadata.
/// </summary>
/// <typeparam name="TCapability">The capability type.</typeparam>
/// <typeparam name="TProperty">The property type.</typeparam>
public sealed class GroupSeverityBuilder<TCapability, TProperty>
    where TCapability : ICapability
    where TProperty : IComparable<TProperty>
{
    private readonly GroupPropertyBuilder<TCapability, TProperty> _property;
    private readonly TProperty _value;
    private readonly ComparisonOperator _operator;

    internal GroupSeverityBuilder(
        GroupPropertyBuilder<TCapability, TProperty> property,
        TProperty value,
        ComparisonOperator op)
    {
        _property = property;
        _value = value;
        _operator = op;
    }

    /// <summary>
    /// Creates an error condition.
    /// </summary>
    /// <param name="name">The optional rule name.</param>
    /// <param name="alias">The optional rule alias.</param>
    /// <param name="message">The optional issue message.</param>
    /// <returns>The current property builder.</returns>
    public GroupPropertyBuilder<TCapability, TProperty> AsError(
        string? name = null,
        string? alias = null,
        string? message = null)
    {
        return AddCondition(RequirementSeverity.Error, name, alias, message);
    }

    /// <summary>
    /// Creates a warning condition.
    /// </summary>
    /// <param name="name">The optional rule name.</param>
    /// <param name="alias">The optional rule alias.</param>
    /// <param name="message">The optional issue message.</param>
    /// <returns>The current property builder.</returns>
    public GroupPropertyBuilder<TCapability, TProperty> AsWarning(
        string? name = null,
        string? alias = null,
        string? message = null)
    {
        return AddCondition(RequirementSeverity.Warning, name, alias, message);
    }

    private GroupPropertyBuilder<TCapability, TProperty> AddCondition(
        RequirementSeverity severity,
        string? name,
        string? alias,
        string? message)
    {
        return _property.AddCondition(new PropertyCondition<TProperty>(
            _operator,
            _value,
            severity,
            name,
            alias,
            message));
    }
}
