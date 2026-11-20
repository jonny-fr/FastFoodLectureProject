using System.Diagnostics.CodeAnalysis;
using BurgerShopConsoleApp.Domain.Products;

namespace BurgerShopConsoleApp.Application.Menu;

/// <summary>
///     Represents a concrete, in-memory implementation of the <see cref="IMenuCatalog" /> interface.
///     This class acts as an application-level service that materializes the product catalog,
///     enforcing domain invariants such as product uniqueness and valid state upon instantiation.
///     The class is sealed to prevent inheritance, ensuring that the internal data structure
///     and validation logic cannot be subverted by derived types.
/// </summary>
public sealed class MenuCatalog : IMenuCatalog
{
    /// <summary>
    ///     The internal data structure utilized for O(1) average-time complexity product retrieval.
    ///     The dictionary maps normalized product keys to their corresponding <see cref="MenuItem" /> instances.
    /// </summary>
    private readonly Dictionary<string, MenuItem> _products;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuCatalog" /> class, materializing the catalog
    ///     from a provided sequence of menu items.
    /// </summary>
    /// <param name="products">The sequence of menu items to populate the catalog.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided sequence is null.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the sequence contains null elements or duplicate product
    ///     definitions.
    /// </exception>
    public MenuCatalog(IEnumerable<MenuItem> products)
    {
        // A guard clause enforces the precondition that the input sequence must not be null,
        // adhering to the fail-fast principle to prevent deferred NullReferenceExceptions.
        ArgumentNullException.ThrowIfNull(products);

        // The dictionary is explicitly initialized with StringComparer.Ordinal. Because the keys
        // are pre-normalized by the ProductKey utility (which applies invariant casing and removes whitespace),
        // an ordinal comparison is both semantically correct and computationally optimal, avoiding
        // the overhead of culture-sensitive or case-insensitive comparisons during lookup operations.
        _products = new Dictionary<string, MenuItem>(StringComparer.Ordinal);

        foreach (var product in products)
        {
            // Iteration over the input sequence enforces element-level validation.
            // A guard clause ensures that no null references are introduced into the catalog,
            // maintaining the integrity of the internal collection.
            ArgumentNullException.ThrowIfNull(product);

            // The product name is transformed into its canonical representation to serve as the dictionary key.
            // This delegates the normalization strategy to a dedicated utility, promoting separation of concerns.
            var key = ProductKey.Normalize(product.Name);

            // The TryAdd method attempts to insert the key-value pair. If the key already exists,
            // it indicates a violation of the domain invariant that product names must be unique.
            // Throwing an InvalidOperationException immediately prevents the construction of an invalid catalog state.
            if (!_products.TryAdd(key, product))
                throw new InvalidOperationException($"Product '{product.Name}' is defined more than once.");
        }
    }

    /// <summary>
    ///     Exposes the collection of all available menu items.
    /// </summary>
    /// <remarks>
    ///     The property returns the dictionary's Values collection, which inherently implements
    ///     <see cref="IReadOnlyCollection{T}" />. This ensures that consumers can enumerate and
    ///     count the items without possessing the ability to mutate the underlying dictionary,
    ///     thereby preserving encapsulation.
    /// </remarks>
    public IReadOnlyCollection<MenuItem> Products => _products.Values;

    /// <summary>
    ///     Attempts to retrieve a menu item based on its name, applying necessary normalization
    ///     to ensure case-insensitive and whitespace-tolerant matching.
    /// </summary>
    /// <param name="productName">The raw textual name of the product to locate.</param>
    /// <param name="product">
    ///     When this method returns <c>true</c>, contains the located <see cref="MenuItem" />;
    ///     otherwise, contains <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if the product was found; otherwise, <c>false</c>.</returns>
    public bool TryGetProduct(string productName, [NotNullWhen(true)] out MenuItem? product)
    {
        // Explicit initialization of the out parameter satisfies the compiler's definite
        // assignment requirements and establishes a known default state for the failure path.
        product = null;

        // The method first validates that the input is not null or whitespace. This short-circuits
        // the evaluation, preventing unnecessary normalization and dictionary lookups for invalid inputs.
        // If the input is valid, it is normalized using the canonical key strategy and passed to
        // TryGetValue. The integration of the NotNullWhen(true) attribute from the interface contract
        // informs the compiler's static analysis that a true return value guarantees a non-null product.
        return !string.IsNullOrWhiteSpace(productName) &&
               _products.TryGetValue(ProductKey.Normalize(productName), out product);
    }
}