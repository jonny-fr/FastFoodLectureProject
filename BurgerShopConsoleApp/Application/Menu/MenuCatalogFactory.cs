using BurgerShopConsoleApp.Domain.Products;

namespace BurgerShopConsoleApp.Application.Menu;

/// <summary>
///     Represents a specialized factory construct responsible for the instantiation
///     and initial population of the <see cref="MenuCatalog" />. By isolating the
///     catalog construction and data ingestion logic, this class adheres to the
///     Single Responsibility Principle. It ensures that the <see cref="MenuCatalog" />
///     domain model remains strictly focused on its core business behaviors,
///     remaining entirely agnostic to the specific formats or sources of its
///     initial configuration data.
/// </summary>
public static class MenuCatalogFactory
{
    /// <summary>
    ///     Generates a canonical, pre-configured <see cref="MenuCatalog" /> instance
    ///     utilizing a hardcoded set of standard product definitions.
    /// </summary>
    /// <returns>A fully populated <see cref="MenuCatalog" /> containing the standard baseline products.</returns>
    /// <remarks>
    ///     This method serves as a deterministic entry point for application initialization.
    ///     By embedding the default seed data directly within the factory, it guarantees that
    ///     a valid, operational baseline state is always available without requiring external
    ///     configuration files, database connections, or user input, thereby simplifying
    ///     deployment and local development environments.
    /// </remarks>
    public static MenuCatalog CreateStandard()
    {
        // The seed data is defined as a localized array of strings, representing 
        // the canonical configuration for the standard menu. This approach keeps 
        // the default state transparent and easily auditable within the source code.
        var standardLines = new[]
        {
            "Burger;5.90;8",
            "Fries;2.80;5",
            "Shake;3.50;4"
        };

        return CreateFromLines(standardLines);
    }

    /// <summary>
    ///     Materializes a <see cref="MenuCatalog" /> by parsing, validating, and aggregating
    ///     a sequence of delimited textual records into concrete domain objects.
    /// </summary>
    /// <param name="lines">The sequence of raw textual records to be processed.</param>
    /// <returns>A fully populated and validated <see cref="MenuCatalog" /> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the provided sequence of lines is null.</exception>
    /// <exception cref="FormatException">Thrown if any line in the sequence fails to parse into a valid product definition.</exception>
    private static MenuCatalog CreateFromLines(IEnumerable<string> lines)
    {
        // A defensive guard clause enforces the precondition that the input sequence 
        // must be initialized, preventing deferred NullReferenceExceptions during iteration.
        ArgumentNullException.ThrowIfNull(lines);

        // A temporary collection is utilized to accumulate the successfully parsed 
        // domain objects before they are passed to the final aggregate constructor.
        var products = new List<MenuItem>();

        foreach (var line in lines)
        {
            // Although the underlying CatalogLineParser utilizes the non-exception-based 
            // 'Try' pattern to gracefully handle expected runtime user input, the ingestion 
            // of configuration or seed data represents a critical initialization phase. 
            // In this architectural context, malformed configuration data constitutes an 
            // unrecoverable system state. Throwing a FormatException enforces a strict 
            // fail-fast paradigm during startup, preventing the application from executing 
            // with an incomplete, degraded, or silently corrupted domain model.
            if (!CatalogLineParser.TryParse(line, out var product, out var errorMessage))
                throw new FormatException($"Invalid catalog line '{line}'. {errorMessage}");

            products.Add(product);
        }

        // The fully validated collection of domain objects is passed to the MenuCatalog 
        // constructor. The constructor will perform its own internal validation (such as 
        // enforcing product uniqueness), ensuring that the final returned object is 
        // guaranteed to be in a strictly valid and consistent state.
        return new MenuCatalog(products);
    }
}