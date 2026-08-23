namespace ReqCap.Results;

/// <summary>
/// Defines how a matched issue condition affects evaluation.
/// </summary>
public enum RequirementSeverity {
    /// <summary>
    /// The matched issue condition blocks evaluation.
    /// </summary>
    Error,

    /// <summary>
    /// The matched issue condition is reported but does not block evaluation.
    /// </summary>
    Warning,
}
