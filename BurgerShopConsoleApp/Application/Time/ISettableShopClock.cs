namespace BurgerShopConsoleApp.Application.Time;

/// <summary>
///     Represents an extended temporal abstraction that introduces mutability to the
///     underlying clock mechanism. By inheriting from the read-only <see cref="IShopClock" />,
///     this interface adheres to the Interface Segregation Principle. It delineates a
///     distinct architectural role: while core domain and application services depend
///     solely on the immutable <see cref="IShopClock" /> for querying time, the
///     infrastructure layer or test harnesses depend on <see cref="ISettableShopClock" />
///     to manipulate the temporal state. This separation prevents domain logic from
///     inadvertently altering the system clock.
/// </summary>
public interface ISettableShopClock : IShopClock
{
    /// <summary>
    ///     Instructs the clock implementation to return the specified
    ///     <see cref="DateTimeOffset" /> for all subsequent queries of <see cref="IShopClock.LocalNow" />.
    /// </summary>
    /// <param name="localNow">The fixed temporal value to be returned by the clock.</param>
    /// <remarks>
    ///     This capability is fundamentally instrumental in achieving deterministic behavior
    ///     in automated testing. By fixing the temporal state, test suites can reliably
    ///     evaluate time-dependent business rules (e.g., order deadlines, opening hours)
    ///     without relying on the non-deterministic progression of the physical system clock.
    /// </remarks>
    void Set(DateTimeOffset localNow);

    /// <summary>
    ///     Reconfigures the clock implementation to delegate time retrieval back to the
    ///     underlying physical system clock.
    /// </summary>
    /// <remarks>
    ///     This method facilitates state reset mechanisms within test fixtures or
    ///     application lifecycle events. It ensures that temporal manipulation in one
    ///     context does not leak into subsequent operations, thereby preserving test
    ///     isolation, idempotency, and predictable execution flows.
    /// </remarks>
    void UseSystemTime();
}