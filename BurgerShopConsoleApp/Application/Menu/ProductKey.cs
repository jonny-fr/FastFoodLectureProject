namespace BurgerShopConsoleApp.Application.Menu;

/// <summary>
///     Represents a stateless utility construct responsible for deriving canonical
///     string representations of product identifiers. By centralizing the normalization
///     logic, the application enforces a uniform equivalence relation across disparate
///     input variations, thereby ensuring consistent lookup and comparison operations
///     within the menu domain.
/// </summary>
public static class ProductKey
{
    /// <summary>
    ///     Transforms an arbitrary textual product name into a standardized, canonical key.
    /// </summary>
    /// <param name="productName">The raw textual input representing a product name.</param>
    /// <returns>A normalized string devoid of whitespace and formatted in invariant uppercase.</returns>
    /// <remarks>
    ///     This deterministic mapping guarantees that semantically identical product names,
    ///     despite superficial syntactic variations (e.g., casing, whitespace), resolve to
    ///     the exact same string value. This facilitates reliable dictionary-based lookups
    ///     and equality comparisons without requiring custom equality comparers.
    /// </remarks>
    public static string Normalize(string productName)
    {
        // A defensive guard clause enforces the strict precondition that the input 
        // must contain non-whitespace characters. Utilizing ArgumentException.ThrowIfNullOrWhiteSpace 
        // adheres to the fail-fast principle, immediately terminating execution upon 
        // invalid input and preventing the propagation of malformed data into 
        // subsequent computational steps.
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);

        // The normalization pipeline executes a sequence of deterministic string 
        // transformations to yield the canonical key:
        return productName
            // 1. Eliminates leading and trailing whitespace artifacts, which are 
            // frequently introduced by user input, copy-paste operations, or data ingestion.
            .Trim()

            // 2. Eradicates internal whitespace characters. The explicit specification 
            // of StringComparison.Ordinal ensures a high-performance, culture-agnostic 
            // byte-level comparison during the search phase, which is computationally 
            // optimal for simple ASCII space removal.
            .Replace(" ", string.Empty, StringComparison.Ordinal)

            // 3. Converts the resultant string to uppercase using the invariant culture. 
            // This is a critical architectural decision; utilizing invariant culture 
            // prevents locale-specific casing rules (such as the Turkish 'i' problem) 
            // from introducing subtle, environment-dependent bugs in product identification.
            .ToUpperInvariant();
    }
}