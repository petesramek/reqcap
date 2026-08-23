namespace ReqCap.Internal;

/// <summary>
/// Provides internal argument validation helpers.
/// </summary>
internal static class ArgumentValidation {
    /// <summary>
    /// Throws when the supplied value is not <see langword="null" /> but is empty or whitespace.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    public static void ThrowIfWhiteSpace(string? value, string paramName) {
        if (value is not null && string.IsNullOrWhiteSpace(value)) {
            throw new ArgumentException("Value cannot be empty or whitespace.", paramName);
        }
    }

    /// <summary>
    /// Throws when the supplied enum value is not defined by its enum type.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="value">The enum value to validate.</param>
    /// <param name="paramName">The name of the parameter being validated.</param>
    public static void ThrowIfInvalidEnum<TEnum>(TEnum value, string paramName)
        where TEnum : struct, Enum {
        if (!Enum.IsDefined(value)) {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                "Value is not a defined enum value.");
        }
    }
}
