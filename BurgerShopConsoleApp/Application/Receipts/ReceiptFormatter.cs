using System.Globalization;
using System.Text;

namespace BurgerShopConsoleApp.Application.Receipts;

/// <summary>
///     Represents a stateless utility construct responsible for the textual serialization
///     of receipt domain objects. By isolating presentation and formatting logic within
///     a dedicated static class, the application adheres to the Single Responsibility
///     Principle. This separation ensures that the core domain models (<see cref="Receipt" />
///     and <see cref="ReceiptLine" />) remain entirely agnostic to how their data is
///     rendered, preserving their purity and facilitating alternative output formats
///     (e.g., JSON, XML, or HTML) without modifying the underlying domain structures.
/// </summary>
public static class ReceiptFormatter
{
    /// <summary>
    ///     Generates a comprehensive, human-readable textual representation of a finalized receipt.
    /// </summary>
    /// <param name="receipt">The immutable receipt object to be formatted.</param>
    /// <returns>A multi-line string containing the formatted receipt data.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided receipt is null.</exception>
    public static string Format(Receipt receipt)
    {
        // A defensive guard clause enforces the precondition that the input object 
        // must be initialized, adhering to the fail-fast paradigm to prevent deferred 
        // NullReferenceExceptions during the formatting process.
        ArgumentNullException.ThrowIfNull(receipt);

        // A StringBuilder is utilized instead of standard string concatenation. 
        // Because strings in .NET are immutable, repeated concatenation operations 
        // would result in the allocation of multiple intermediate string objects on 
        // the managed heap. StringBuilder maintains a mutable character buffer, 
        // significantly minimizing memory allocation and reducing Garbage Collection 
        // (GC) pressure, which is a critical optimization in high-throughput scenarios.
        var builder = new StringBuilder();

        builder.AppendLine("Fast-food order receipt");

        // CultureInfo.InvariantCulture is strictly mandated for all temporal and 
        // numerical formatting. This ensures deterministic, locale-independent output, 
        // preventing regional discrepancies (e.g., '1.00' vs '1,00' or varying date 
        // formats) that could compromise the integrity of printed receipts, logs, 
        // or machine-parsed data exchanges.
        builder.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "Date: {0}",
            receipt.OrderDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));

        builder.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "Time: {0}",
            receipt.OrderTime.ToString("HH:mm", CultureInfo.InvariantCulture)));

        builder.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "Ready: {0}",
            receipt.ReadyTime.ToString("HH:mm", CultureInfo.InvariantCulture)));

        // A visual delimiter is constructed using the string constructor to repeat 
        // the hyphen character, establishing a clear structural boundary in the 
        // textual output without requiring hardcoded string literals.
        builder.AppendLine(new string('-', 44));

        foreach (var line in receipt.Lines) builder.AppendLine(FormatLine(line));

        builder.AppendLine(new string('-', 44));

        // The 'F2' format specifier ensures that the monetary total is consistently 
        // rendered with exactly two decimal places, conforming to standard financial 
        // display conventions.
        builder.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "Total: {0:F2} EUR",
            receipt.Total));

        return builder.ToString();
    }

    /// <summary>
    ///     Formats an individual receipt line item into a standardized textual representation.
    /// </summary>
    /// <param name="line">The receipt line to be formatted.</param>
    /// <returns>A formatted string representing the line item's details and total.</returns>
    /// <remarks>
    ///     The parameter is passed using the <c>in</c> modifier. Because <see cref="ReceiptLine" />
    ///     is defined as a <c>readonly record struct</c>, passing it by value (the default for
    ///     structs) would incur the overhead of copying all its fields. The <c>in</c> modifier
    ///     passes the struct by readonly reference, eliminating the copy overhead while the
    ///     compiler enforces that the struct's immutability is not violated within the method scope.
    /// </remarks>
    public static string FormatLine(in ReceiptLine line)
    {
        // The string.Create method is invoked with an IFormatProvider and a FormattableString 
        // (which is the compile-time target of the interpolated string $"..."). This specific 
        // overload is a high-performance pattern in modern .NET; it bypasses the intermediate 
        // allocation of a composite string, formatting the components directly into the final 
        // allocated string buffer, thereby optimizing both memory and CPU utilization.
        return string.Create(
            CultureInfo.InvariantCulture,
            $"{line.ProductName} {line.Quantity} x {line.UnitPrice:F2} EUR = {line.LineTotal:F2} EUR");
    }

    /// <summary>
    ///     Provides an overloaded formatting entry point that accepts primitive parameters,
    ///     calculates the line total, and delegates to the primary formatting method.
    /// </summary>
    /// <param name="productName">The textual name of the product.</param>
    /// <param name="quantity">The discrete count of the product units.</param>
    /// <param name="unitPrice">The monetary value of a single product unit.</param>
    /// <returns>A formatted string representing the calculated line item.</returns>
    public static string FormatLine(string productName, int quantity, decimal unitPrice)
    {
        // Guard clauses enforce domain invariants at the method boundary, ensuring that 
        // physical quantities and monetary values are strictly positive before any 
        // computational logic is executed.
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(unitPrice);

        var lineTotal = unitPrice * quantity;

        // Delegates the actual string construction to the primary FormatLine method. 
        // This adheres to the Don't Repeat Yourself (DRY) principle, centralizing the 
        // formatting logic and ensuring consistency across all invocation paths.
        return FormatLine(new ReceiptLine(productName, quantity, unitPrice, lineTotal));
    }
}