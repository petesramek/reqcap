namespace ReqCap.Groups;

/// <summary>
/// Defines logical group operators.
/// </summary>
public enum LogicalOperator
{
    /// <summary>
    /// All child rules or groups must pass.
    /// </summary>
    And,

    /// <summary>
    /// At least one child rule or group must pass.
    /// </summary>
    Or,
}
