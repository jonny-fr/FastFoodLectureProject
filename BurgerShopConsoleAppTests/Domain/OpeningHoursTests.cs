using BurgerShopConsoleApp.Domain.Time;
using FluentAssertions;

namespace BurgerShopConsoleAppTests.Domain;

/// <summary>
///     Encapsulates the automated verification suite for the <see cref="OpeningHours" />
///     value object. This test class validates the enforcement of temporal and calendrical
///     business invariants, ensuring that operational constraints are strictly evaluated
///     during order acceptance and readiness estimation.
/// </summary>
public sealed class OpeningHoursTests
{
    /// <summary>
    ///     Defines a test fixture representing a Saturday.
    /// </summary>
    /// <remarks>
    ///     The specific date (November 7, 2026) is deliberately selected to align with
    ///     the required day of the week. Declaring the fixture as <c>static readonly</c>
    ///     optimizes memory allocation by instantiating the value exactly once per test
    ///     assembly execution, while the immutability guarantees that the test data
    ///     remains uncontaminated across the test lifecycle.
    /// </remarks>
    private static readonly DateOnly Saturday = new(2026, 11, 7);

    /// <summary>
    ///     Defines a test fixture representing a Sunday, utilized to validate the
    ///     calendrical closure constraint.
    /// </summary>
    private static readonly DateOnly Sunday = new(2026, 11, 8);

    /// <summary>
    ///     Represents the System Under Test (SUT), initialized to the canonical
    ///     <see cref="OpeningHours.Standard" /> configuration.
    /// </summary>
    /// <remarks>
    ///     Although <see cref="OpeningHours" /> is an immutable value object, declaring
    ///     the SUT as an instance field aligns with standard xUnit lifecycle practices,
    ///     ensuring strict test isolation and preventing potential state leakage.
    /// </remarks>
    private readonly OpeningHours _sut = OpeningHours.Standard;

    /// <summary>
    ///     Validates the temporal boundary conditions of the operational window.
    /// </summary>
    /// <remarks>
    ///     The utilization of the <c>[Theory]</c> attribute in conjunction with
    ///     <c>[InlineData]</c> facilitates parameterized testing. The selected data points
    ///     represent a rigorous boundary value analysis, evaluating the exact thresholds
    ///     of acceptance (10:00, 21:45) and the immediate rejection points (9:59, 21:46).
    ///     This ensures that the inclusive boundaries of the time window are correctly
    ///     enforced without off-by-one errors.
    /// </remarks>
    [Theory]
    [InlineData(10, 0, true)]
    [InlineData(9, 59, false)]
    [InlineData(21, 45, true)]
    [InlineData(21, 46, false)]
    public void Orders_are_accepted_only_inside_time_window(int hour, int minute, bool expected)
    {
        // The TimeOnly structure is instantiated using the parameterized hour and minute 
        // values to construct the precise temporal input for the boundary evaluation.
        var time = new TimeOnly(hour, minute);

        _sut.CanAcceptOrder(Saturday, time).Should().Be(expected);
    }

    /// <summary>
    ///     Validates the calendrical constraint that prohibits order acceptance on Sundays,
    ///     regardless of the time of day.
    /// </summary>
    /// <remarks>
    ///     This test isolates the day-of-week evaluation logic, ensuring that the temporal
    ///     boundaries are not erroneously applied when the date itself violates the operational
    ///     schedule. The selection of a midday time (12:00) guarantees that the failure is
    ///     strictly attributable to the day-of-week constraint rather than a temporal boundary violation.
    /// </remarks>
    [Fact]
    public void Sunday_is_closed()
    {
        _sut.CanAcceptOrder(Sunday, new TimeOnly(12, 0)).Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the deterministic temporal arithmetic for calculating the readiness
    ///     timestamp when the addition of preparation minutes does not cross the midnight boundary.
    /// </summary>
    /// <remarks>
    ///     This baseline test confirms that the underlying temporal addition mechanism
    ///     functions correctly within a standard diurnal cycle, providing a control case
    ///     for the more complex day-boundary transition scenario.
    /// </remarks>
    [Fact]
    public void Ready_time_adds_preparation_minutes()
    {
        _sut.GetReadyTime(new TimeOnly(12, 0), 8)
            .Should()
            .Be(new TimeOnly(12, 8));
    }

    /// <summary>
    ///     Validates the modular arithmetic behavior of the underlying <see cref="TimeOnly" />
    ///     structure when the preparation duration extends beyond the 24-hour diurnal cycle.
    /// </summary>
    /// <remarks>
    ///     This test ensures that the domain logic correctly handles day-boundary transitions.
    ///     By verifying that 23:55 plus 10 minutes yields 00:05, it confirms that the temporal
    ///     calculation inherently wraps around midnight without requiring explicit date-tracking
    ///     or manual modulo operations within the domain model. This abstraction significantly
    ///     simplifies the calculation of readiness times for late-night orders.
    /// </remarks>
    [Fact]
    public void Ready_time_wraps_after_midnight()
    {
        _sut.GetReadyTime(new TimeOnly(23, 55), 10)
            .Should()
            .Be(new TimeOnly(0, 5));
    }
}