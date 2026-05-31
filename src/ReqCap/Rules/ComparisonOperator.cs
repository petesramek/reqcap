namespace ReqCap.Rules;

/// <summary>
/// Defines comparison operators supported by built-in comparison rules.
/// </summary>
public enum ComparisonOperator {
    /// <summary>Requires the actual value to be greater than or equal to the expected value.</summary>
    GreaterOrEqual,
    /// <summary>Requires the actual value to be greater than the expected value.</summary>
    GreaterThan,
    /// <summary>Requires the actual value to be less than or equal to the expected value.</summary>
    LessOrEqual,
    /// <summary>Requires the actual value to be less than the expected value.</summary>
    LessThan,
    /// <summary>Requires the actual value to be equal to the expected value.</summary>
    Equal,
    /// <summary>Requires the actual value to be different from the expected value.</summary>
    NotEqual,
}
