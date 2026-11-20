using System.Diagnostics.CodeAnalysis;
using BurgerShopConsoleApp.Application.Parsing;
using BurgerShopConsoleApp.Domain.Products;

namespace BurgerShopConsoleApp.Application.Menu;

/// <summary>
///     Represents a boundary translation mechanism responsible for deserializing
///     delimited textual records into strongly typed domain objects. The class is
///     declared as <c>static</c> to enforce a stateless utility pattern, ensuring
///     that parsing logic remains isolated from domain business rules and adheres
///     to the Single Responsibility Principle.
/// </summary>
public static class CatalogLineParser
{
    /// <summary>
    ///     Attempts to parse a semicolon-delimited textual record into a concrete
    ///     <see cref="MenuItem" /> instance, employing the standard .NET "Try" pattern
    ///     to handle expected parsing failures without resorting to exception-driven
    ///     control flow.
    /// </summary>
    /// <param name="line">The raw textual input representing a single catalog record.</param>
    /// <param name="product">
    ///     When this method returns <c>true</c>, contains the parsed <see cref="MenuItem" />;
    ///     otherwise, contains <c>null</c>. The <see cref="NotNullWhenAttribute" /> instructs
    ///     the compiler's static analysis to guarantee non-nullability upon a successful return,
    ///     eliminating the need for redundant null checks in the calling code.
    /// </param>
    /// <param name="errorMessage">
    ///     When this method returns <c>false</c>, contains a diagnostic message detailing the
    ///     parsing failure; otherwise, contains <c>null</c>. The inverse
    ///     <see cref="NotNullWhenAttribute" /> ensures the compiler recognizes the mutual
    ///     exclusivity of the success and failure states.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the record was successfully parsed and validated; otherwise, <c>false</c>.
    /// </returns>
    public static bool TryParse(
        string? line,
        [NotNullWhen(true)] out MenuItem? product,
        [NotNullWhen(false)] out string? errorMessage)
    {
        // Explicit initialization of the out parameters satisfies the C# compiler's 
        // definite assignment rules and establishes a known default state for the 
        // failure path, ensuring variables are safely initialized before any early returns.
        product = null;
        errorMessage = null;

        // A defensive guard clause validates the structural integrity of the input, 
        // ensuring that null, empty, or whitespace-only records are rejected immediately 
        // before any computational resources are expended on parsing.
        if (string.IsNullOrWhiteSpace(line))
        {
            errorMessage = "Catalog line must not be empty.";
            return false;
        }

        // The Split method is invoked with TrimEntries and RemoveEmptyEntries flags. 
        // This configuration robustly handles minor formatting inconsistencies in the 
        // input data, such as extraneous whitespace or consecutive delimiters, thereby 
        // increasing the parser's resilience to malformed but semantically valid records.
        var fields = line.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        // Enforces the structural invariant that a valid record must contain exactly 
        // three delimited fields. This fail-fast mechanism prevents index-out-of-range 
        // exceptions during subsequent field extraction and provides precise diagnostic feedback.
        if (fields.Length != 3)
        {
            errorMessage = "Expected format: Category;Price;PreparationMinutes.";
            return false;
        }

        // Utilizes Enum.TryParse with case-insensitivity to map the textual category 
        // identifier to the ProductCategory enumeration. This provides flexibility in 
        // input formatting while maintaining strict type safety within the domain model.
        if (!Enum.TryParse<ProductCategory>(fields[0], true, out var category))
        {
            errorMessage = $"Unknown category '{fields[0]}'.";
            return false;
        }

        // Delegates the monetary parsing to a specialized boundary utility. This 
        // separation of concerns ensures that financial validation rules (e.g., positive 
        // values, invariant culture formatting) are centralized and consistently applied 
        // across the application boundary.
        if (!InputParser.TryParseMoney(fields[1], out var price))
        {
            errorMessage = $"Invalid price '{fields[1]}'.";
            return false;
        }

        // Similarly delegates integer parsing for the preparation duration, reusing the 
        // established validation logic for physical quantities to maintain uniform 
        // boundary enforcement.
        if (!InputParser.TryParseQuantity(fields[2], out var preparationMinutes))
        {
            errorMessage = $"Invalid preparation minutes '{fields[2]}'.";
            return false;
        }

        // Upon successful validation of all constituent fields, the instantiation of the 
        // domain object is delegated to the MenuItemFactory. This adheres to the Factory 
        // Method pattern, decoupling the parsing mechanism from the specific construction 
        // logic of concrete MenuItem derivatives and preserving the Open/Closed Principle.
        product = MenuItemFactory.Create(category, price, preparationMinutes);
        return true;
    }
}