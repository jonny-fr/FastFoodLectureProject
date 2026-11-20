namespace BurgerShopConsoleApp.Domain.Time;

/// <summary>
///     Provides stateless utility operations for temporal manipulation.
///     The class is declared as <c>static</c> to preclude instantiation and enforce
///     that its members operate solely on provided parameters without
///     maintaining internal instance state. The namespace delineates a specific
///     bounded context concerned with domain-specific temporal logic.
/// </summary>
public static class TimeShift
{
    /// <summary>
    ///     Advances the specified <see cref="TimeOnly" /> instance by a given
    ///     number of minutes, mutating the original value.
    /// </summary>
    /// <param name="time">
    ///     The reference to the <see cref="TimeOnly" /> value to be modified.
    ///     The <c>ref</c> modifier is strictly required because <see cref="TimeOnly" />
    ///     is a value type (struct). Passing it by value would result in a local
    ///     copy being modified within the method scope, leaving the caller's
    ///     original variable unchanged.
    /// </param>
    /// <param name="minutes">
    ///     The non-negative number of minutes to add to the specified time.
    /// </param>
    public static void AddMinutes(ref TimeOnly time, int minutes)
    {
        // A guard clause utilizing ArgumentOutOfRangeException enforces a strict 
        // precondition: the temporal shift must be non-negative. This fail-fast 
        // mechanism ensures domain invariants are maintained at the method boundary, 
        // preventing invalid state transitions before any computational logic is executed.
        ArgumentOutOfRangeException.ThrowIfNegative(minutes);

        // The TimeOnly.AddMinutes method computes the resultant time. It is didactically 
        // pertinent to note that the underlying .NET implementation inherently handles 
        // day-boundary transitions (i.e., it wraps around midnight). Consequently, adding 
        // minutes to a time near the end of the day correctly yields a time on the 
        // subsequent day, abstracting the complexity of modular arithmetic from the domain logic.
        // The assignment directly mutates the caller's variable due to the 'ref' parameter.
        time = time.AddMinutes(minutes);
    }
}