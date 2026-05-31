namespace ReqCap.Results;

/// <summary>
/// Defines how a failed rule affects evaluation.
/// </summary>
public enum RequirementSeverity {
    /// <summary>A blocking failure.</summary>
    Error,
    /// <summary>A non-blocking failure.</summary>
    Warning,
}
