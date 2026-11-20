using BurgerShopConsoleApp.Application.Receipts;
using FluentAssertions;

namespace BurgerShopConsoleAppTests.Application;

/// <summary>
///     Encapsulates the automated verification suite for the <see cref="ReceiptFormatter" />
///     presentation utility. This test class validates the textual serialization of domain
///     read models, ensuring that formatting logic remains strictly isolated from domain
///     business rules and adheres to deterministic, locale-independent output conventions.
/// </summary>
public sealed class ReceiptFormatterTests
{
    /// <summary>
    ///     Validates the primary formatting method using a fully constructed value object.
    /// </summary>
    /// <remarks>
    ///     The <c>in</c> modifier is utilized to pass the <see cref="ReceiptLine" />
    ///     (a <c>readonly record struct</c>) by readonly reference. This prevents unnecessary
    ///     defensive copying on the call stack while the compiler enforces that the struct's
    ///     immutability is not violated within the method scope. The assertion verifies exact
    ///     string equality, guaranteeing that financial amounts are rendered with precise
    ///     base-10 fractional representation and invariant cultural formatting.
    /// </remarks>
    [Fact]
    public void FormatLine_formats_eur_amounts()
    {
        var line = new ReceiptLine("Burger", 2, 5.90m, 11.80m);

        ReceiptFormatter.FormatLine(in line)
            .Should()
            .Be("Burger 2 x 5.90 EUR = 11.80 EUR");
    }

    /// <summary>
    ///     Validates the overloaded entry point that accepts primitive parameters.
    /// </summary>
    /// <remarks>
    ///     This test ensures comprehensive coverage of the utility's API surface. It verifies
    ///     that the overloaded method correctly computes the line total internally before
    ///     delegating to the primary formatting logic, confirming the correctness of the
    ///     convenience mechanism provided to the calling boundary.
    /// </remarks>
    [Fact]
    public void FormatLine_overload_calculates_line_total()
    {
        ReceiptFormatter.FormatLine("Fries", 2, 2.80m)
            .Should()
            .Be("Fries 2 x 2.80 EUR = 5.60 EUR");
    }

    /// <summary>
    ///     Validates the comprehensive formatting of a complete receipt read model.
    /// </summary>
    /// <remarks>
    ///     The test employs the <c>ContainAll</c> assertion rather than exact string equality.
    ///     This is a deliberate testing strategy for multi-line textual outputs; it verifies
    ///     the presence of critical data points without tightly coupling the test to the exact
    ///     whitespace, line break characters, or structural layout, thereby reducing test
    ///     fragility during minor presentation adjustments. Furthermore, the initialization
    ///     of the <see cref="IReadOnlyList{T}" /> parameter utilizes modern C# collection
    ///     expressions for concise syntax. The assertions collectively confirm that temporal
    ///     and financial data are rendered using strict, invariant formatting conventions.
    /// </remarks>
    [Fact]
    public void Format_contains_header_totals_and_ready_time()
    {
        var receipt = new Receipt(
            new DateOnly(2026, 11, 7),
            new TimeOnly(12, 0),
            new TimeOnly(12, 8),
            [
                new ReceiptLine("Burger", 2, 5.90m, 11.80m)
            ],
            11.80m);

        var result = ReceiptFormatter.Format(receipt);

        result.Should().ContainAll(
            "Date: 2026-11-07",
            "Time: 12:00",
            "Ready: 12:08",
            "Burger 2 x 5.90 EUR = 11.80 EUR",
            "Total: 11.80 EUR");
    }
}