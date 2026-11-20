using System.Diagnostics.CodeAnalysis;
using BurgerShopConsoleApp.Domain.Products;

namespace BurgerShopConsoleApp.Application.Menu;

/// <summary>
///     Defines the architectural contract for accessing the domain's product catalog.
///     By abstracting the data retrieval mechanism behind an interface, the application
///     adheres to the Dependency Inversion Principle. This decoupling ensures that the
///     domain and application layers remain entirely agnostic to the underlying
///     persistence or data source implementation (e.g., in-memory collections,
///     relational databases, or external APIs), thereby facilitating testability and
///     future architectural evolution.
/// </summary>
public interface IMenuCatalog
{
    /// <summary>
    ///     Exposes the complete enumeration of available menu items.
    /// </summary>
    /// <remarks>
    ///     The return type is strictly defined as <see cref="IReadOnlyCollection{T}" />.
    ///     This specific interface choice is didactically significant for two reasons:
    ///     first, it guarantees that the collection's <c>Count</c> can be evaluated in
    ///     O(1) time complexity, unlike <see cref="IEnumerable{T}" /> which may require
    ///     full enumeration; second, it explicitly prohibits external consumers from
    ///     mutating the underlying data structure, preserving the encapsulation and
    ///     integrity of the catalog's internal state.
    /// </remarks>
    IReadOnlyCollection<MenuItem> Products { get; }

    /// <summary>
    ///     Attempts to retrieve a specific menu item by its designated name, employing
    ///     the standard .NET "Try" pattern.
    /// </summary>
    /// <param name="productName">The textual identifier used as the search key.</param>
    /// <param name="product">
    ///     When this method returns <c>true</c>, contains the resolved <see cref="MenuItem" />
    ///     instance; otherwise, contains <c>null</c>.
    /// </param>
    /// <returns>
    ///     <c>true</c> if the product was successfully located within the catalog;
    ///     otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    ///     The utilization of the "Try" pattern avoids the performance overhead and
    ///     control-flow disruption associated with throwing exceptions for expected
    ///     negative outcomes (e.g., a product not being found). It provides a clean,
    ///     fail-safe mechanism for lookup operations.
    /// </remarks>
    bool TryGetProduct(
        string productName,
        [NotNullWhen(true)] out MenuItem? product);
}

// Didactical Note on the [NotNullWhen(true)] attribute:
// The System.Diagnostics.CodeAnalysis.NotNullWhenAttribute is a critical component 
// of C#'s Nullable Reference Types (NRT) static analysis. By applying this attribute 
// to the 'out' parameter, the compiler's flow analysis is instructed to recognize 
// that if the method returns 'true', the 'product' variable is guaranteed to be 
// non-null. Conversely, if the method returns 'false', the variable may be null. 
// This eliminates the need for redundant null checks in the calling code and 
// prevents potential NullReferenceExceptions by shifting null-safety validation 
// from runtime to compile time.