using System.Globalization;

namespace BurgerShopConsoleApp.Application.Parsing;

/// <summary>
///     Represents a boundary translation mechanism responsible for converting
///     unstructured textual input into strongly typed domain primitives.
///     The class is declared as <c>static</c> to preclude instantiation,
///     enforcing a stateless utility pattern. This segregation ensures that
///     parsing logic is isolated from domain business rules, adhering to the
///     Single Responsibility Principle.
/// </summary>
public static class InputParser
{
    /// <summary>
    ///     Attempts to parse a textual representation of a quantity into a strictly positive integer.
    /// </summary>
    /// <param name="text">The input string to evaluate.</param>
    /// <param name="quantity">
    ///     When this method returns <c>true</c>, contains the parsed integer value;
    ///     otherwise, contains the default value of zero.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the parsing succeeds and the resulting quantity is strictly greater than zero;
    ///     otherwise, <c>false</c>.
    /// </returns>
    public static bool TryParseQuantity(string? text, out int quantity)
    {
        // The TryParse pattern is utilized to implement a fail-safe mechanism, 
        // avoiding the performance overhead and control-flow disruption of exceptions 
        // for expected invalid inputs. CultureInfo.InvariantCulture ensures 
        // locale-independent parsing, preventing regional formatting discrepancies.
        if (!int.TryParse(text?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out quantity)) return false;

        // Enforces the domain invariant that a physical quantity must be strictly positive. 
        // This boundary check prevents invalid state propagation into the domain layer.
        return quantity > 0;
    }

    /// <summary>
    ///     Attempts to parse a textual representation of a monetary value into a strictly positive decimal.
    /// </summary>
    /// <param name="text">The input string to evaluate.</param>
    /// <param name="amount">
    ///     When this method returns <c>true</c>, contains the parsed decimal value;
    ///     otherwise, contains zero.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the parsing succeeds and the resulting amount is strictly positive;
    ///     otherwise, <c>false</c>.
    /// </returns>
    public static bool TryParseMoney(string? text, out decimal amount)
    {
        // Explicit initialization satisfies the C# compiler's definite assignment rule 
        // for 'out' parameters, ensuring the variable is assigned even if the method 
        // exits via an early return statement.
        amount = 0m;

        // The 'decimal' type is explicitly chosen over binary floating-point types 
        // (e.g., 'double') to mitigate precision loss. This is a fundamental requirement 
        // in financial computations where exact base-10 fractional representation is critical.
        if (!decimal.TryParse(text?.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
            return false;

        // Enforces the business rule that monetary values in this context must be positive.
        if (parsed <= 0m) return false;

        amount = parsed;
        return true;
    }

    /// <summary>
    ///     Attempts to parse a textual representation of time into a <see cref="TimeOnly" /> instance.
    /// </summary>
    /// <param name="text">The input string to evaluate.</param>
    /// <param name="time">
    ///     When this method returns <c>true</c>, contains the parsed time value;
    ///     otherwise, contains the default value.
    /// </param>
    /// <returns><c>true</c> if the parsing succeeds; otherwise, <c>false</c>.</returns>
    public static bool TryParseTime(string? text, out TimeOnly time)
    {
        // TryParseExact enforces strict adherence to the 24-hour clock convention ("HH:mm"). 
        // This prevents ambiguous interpretations (e.g., 12-hour AM/PM formats) and ensures 
        // data integrity at the ingestion boundary by rejecting non-conforming strings.
        return TimeOnly.TryParseExact(
            text?.Trim(),
            "HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out time);
    }

    /// <summary>
    ///     Attempts to parse a textual representation of a calendar date into a <see cref="DateOnly" /> instance.
    /// </summary>
    /// <param name="text">The input string to evaluate.</param>
    /// <param name="date">
    ///     When this method returns <c>true</c>, contains the parsed date value;
    ///     otherwise, contains the default value.
    /// </param>
    /// <returns><c>true</c> if the parsing succeeds; otherwise, <c>false</c>.</returns>
    public static bool TryParseDate(string? text, out DateOnly date)
    {
        // Enforces the ISO 8601 date format ("yyyy-MM-dd"). The utilization of DateOnly 
        // abstracts away time-of-day and timezone complexities, modeling a pure calendar 
        // date which is semantically appropriate for business scheduling and daily operations.
        return DateOnly.TryParseExact(
            text?.Trim(),
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);
    }

    /// <summary>
    ///     Attempts to parse a textual representation of a combined date and time into a <see cref="DateTimeOffset" />
    ///     instance.
    /// </summary>
    /// <param name="text">The input string to evaluate.</param>
    /// <param name="localDateTime">
    ///     When this method returns <c>true</c>, contains the parsed temporal value;
    ///     otherwise, contains the default value.
    /// </param>
    /// <returns><c>true</c> if the parsing succeeds; otherwise, <c>false</c>.</returns>
    public static bool TryParseLocalDateTime(string? text, out DateTimeOffset localDateTime)
    {
        localDateTime = default;

        // Parses an ISO 8601 combined date and time representation. 
        // The use of TryParseExact ensures the input strictly conforms to the expected structural format.
        if (!DateTime.TryParseExact(
                text?.Trim(),
                "yyyy-MM-dd'T'HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var dateTime))
            return false;

        // The parsed DateTime is explicitly encapsulated within a DateTimeOffset utilizing 
        // a zero offset (TimeSpan.Zero). This designates the temporal value as Coordinated 
        // Universal Time (UTC), establishing a canonical, timezone-agnostic reference point 
        // which is a fundamental practice in temporal data management to prevent localization ambiguities.
        localDateTime = new DateTimeOffset(dateTime, TimeSpan.Zero);
        return true;
    }
}