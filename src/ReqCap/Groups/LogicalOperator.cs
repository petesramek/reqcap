namespace ReqCap.Groups;

/// <summary>
/// Defines logical group operators.
/// </summary>
public enum LogicalOperator
{
    /// <summary>
    /// All child rules or groups are evaluated.
    /// </summary>
    And,

    /// <summary>
    /// At least one child rule or group must be allowed.
    /// </summary>
    Or,
}
