namespace BurgerShopConsoleApp.Application.Time;

/// <summary>
///     Defines an abstraction for retrieving the current temporal state of the business context.
///     By encapsulating time retrieval behind an interface, the application adheres to the
///     Dependency Inversion Principle. This decoupling prevents direct reliance on the
///     non-deterministic system clock (e.g., <c>DateTimeOffset.Now</c>), thereby facilitating
///     deterministic unit testing through the injection of mock, stub, or frozen clock implementations.
/// </summary>
public interface IShopClock
{
    /// <summary>
    ///     Retrieves the current date and time, contextualized to the operational timezone of the shop.
    /// </summary>
    /// <remarks>
    ///     The utilization of <see cref="DateTimeOffset" /> rather than <see cref="DateTime" /> is
    ///     a deliberate architectural choice to preserve timezone awareness and prevent temporal
    ///     ambiguities during serialization, persistence, or cross-system synchronization. The semantic
    ///     designation "LocalNow" indicates that the returned value represents the local time of the
    ///     business entity (the shop), which may conceptually differ from the local time of the
    ///     underlying server infrastructure hosting the application.
    /// </remarks>
    DateTimeOffset LocalNow { get; }
}