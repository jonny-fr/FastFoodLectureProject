namespace BurgerShopConsoleApp.Domain.Time;

/// <summary>
///     Represents an immutable value object encapsulating the temporal constraints
///     of business operations. The utilization of a <c>sealed record</c> with a
///     primary constructor enforces immutability, structural equality, and precludes
///     inheritance. In the context of Domain-Driven Design, this models a Value Object
///     where identity is defined strictly by its attribute values rather than a
///     unique conceptual identity.
/// </summary>
public sealed record OpeningHours(TimeOnly OpensAt, TimeOnly LastOrderAt)
{
    /// <summary>
    ///     Provides a canonical, pre-configured instance representing standard
    ///     operational hours.
    /// </summary>
    /// <remarks>
    ///     Exposing this as a static property ensures a single, globally accessible
    ///     instance of these default parameters. This pattern optimizes memory
    ///     allocation by preventing redundant object instantiation and enforces
    ///     consistency across the application domain when default configurations
    ///     are required.
    /// </remarks>
    public static OpeningHours Standard { get; } =
        new(new TimeOnly(10, 0), new TimeOnly(21, 45));

    /// <summary>
    ///     Evaluates the business invariants required to accept a customer order.
    /// </summary>
    /// <param name="date">The calendar date of the proposed order.</param>
    /// <param name="orderTime">The specific time the order is placed.</param>
    /// <returns>
    ///     <c>true</c> if the order complies with all operational constraints;
    ///     otherwise, <c>false</c>.
    /// </returns>
    public bool CanAcceptOrder(DateOnly date, TimeOnly orderTime)
    {
        // This method encapsulates core domain rules, specifically the exclusion 
        // of Sundays and the enforcement of temporal boundaries. By returning a 
        // boolean predicate, it provides a definitive validation mechanism, 
        // adhering to the principle of separating domain logic from infrastructure 
        // and presentation concerns. The short-circuit evaluation of the logical 
        // AND operators ensures optimal performance by halting execution upon the 
        // first violated constraint.
        return date.DayOfWeek != DayOfWeek.Sunday &&
               orderTime >= OpensAt &&
               orderTime <= LastOrderAt;
    }

    /// <summary>
    ///     Computes the anticipated readiness timestamp by adding the required
    ///     preparation duration to the order time.
    /// </summary>
    /// <param name="orderTime">The baseline time at which the order is placed.</param>
    /// <param name="preparationMinutes">The non-negative duration required for preparation.</param>
    /// <returns>A new <see cref="TimeOnly" /> instance representing the calculated ready time.</returns>
    public TimeOnly GetReadyTime(TimeOnly orderTime, int preparationMinutes)
    {
        // A guard clause utilizing ArgumentOutOfRangeException enforces a strict 
        // precondition: the preparation duration must be non-negative. This 
        // fail-fast mechanism ensures mathematical and domain validity before 
        // computational logic is executed.
        ArgumentOutOfRangeException.ThrowIfNegative(preparationMinutes);

        // The method is declared static because it performs a pure temporal 
        // calculation that does not rely on the instance state (OpensAt or LastOrderAt). 
        // This design promotes reusability and aligns with the principles of 
        // functional programming by ensuring the output is determined solely by 
        // the input parameters, devoid of side effects.
        var readyTime = orderTime;

        // The actual temporal mutation is delegated to the TimeShift utility. 
        // This demonstrates the separation of concerns, isolating the mechanics 
        // of time arithmetic from the business logic of order processing.
        TimeShift.AddMinutes(ref readyTime, preparationMinutes);

        return readyTime;
    }
}