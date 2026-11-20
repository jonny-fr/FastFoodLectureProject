namespace BurgerShopConsoleApp.Application.Time;

/// <summary>
///     Represents a concrete implementation of the settable clock abstraction,
///     scoped to the lifecycle of a specific operational or testing session.
///     By encapsulating temporal state within a session boundary, this class
///     ensures that time manipulation remains isolated and does not induce
///     global side effects across the broader application domain. The class
///     is sealed to preclude inheritance, thereby preserving the integrity
///     of its temporal encapsulation.
/// </summary>
public sealed class SessionClock : ISettableShopClock
{
    /// <summary>
    ///     Maintains a reference to the abstracted system time provider.
    ///     The utilization of the <see cref="TimeProvider" /> abstraction decouples
    ///     the class from the static, non-testable system clock properties,
    ///     thereby facilitating advanced temporal mocking and deterministic
    ///     execution in automated testing environments.
    /// </summary>
    private readonly TimeProvider _systemTime;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SessionClock" /> class.
    /// </summary>
    /// <param name="systemTime">The abstracted time provider used to resolve the baseline temporal state.</param>
    public SessionClock(TimeProvider systemTime)
    {
        // A guard clause enforces the dependency injection contract, ensuring 
        // that the temporal provider is never null, which would otherwise result 
        // in a deferred NullReferenceException upon time resolution.
        ArgumentNullException.ThrowIfNull(systemTime);

        _systemTime = systemTime;

        // The initial state of the session clock is synchronized with the 
        // system time at the exact moment of instantiation. This guarantees 
        // that the clock reflects a valid, coherent temporal baseline before 
        // any explicit manipulation occurs.
        LocalNow = systemTime.GetLocalNow();
    }

    /// <summary>
    ///     Represents the current temporal state of the session.
    /// </summary>
    /// <remarks>
    ///     The property exposes a public getter to satisfy the read-only
    ///     <see cref="IShopClock" /> contract, while restricting mutation to the
    ///     class's internal scope via a private setter. This encapsulation ensures
    ///     that the temporal state can only be modified through the explicit,
    ///     intention-revealing methods defined by the <see cref="ISettableShopClock" />
    ///     interface, preventing accidental state corruption by external consumers.
    /// </remarks>
    public DateTimeOffset LocalNow { get; private set; }

    /// <summary>
    ///     Overrides the session's temporal state with the specified <see cref="DateTimeOffset" />.
    /// </summary>
    /// <param name="localNow">The fixed temporal value to be applied to the session.</param>
    /// <remarks>
    ///     This method serves as the primary mechanism for temporal manipulation,
    ///     allowing test harnesses or specific application workflows to freeze or
    ///     advance time deterministically. Because the state is session-scoped,
    ///     this mutation does not affect the underlying physical system clock.
    /// </remarks>
    public void Set(DateTimeOffset localNow)
    {
        LocalNow = localNow;
    }

    /// <summary>
    ///     Re-synchronizes the session's temporal state with the underlying system
    ///     time provided by the injected <see cref="TimeProvider" />.
    /// </summary>
    /// <remarks>
    ///     This reset mechanism is critical for restoring the clock to its natural
    ///     progression. It is typically invoked during test teardown phases or upon
    ///     the initialization of a new session to prevent temporal state leakage
    ///     and ensure that subsequent operations evaluate against the actual current time.
    /// </remarks>
    public void UseSystemTime()
    {
        LocalNow = _systemTime.GetLocalNow();
    }
}