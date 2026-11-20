using BurgerShopConsoleApp.Domain.Products;

namespace BurgerShopConsoleApp.Application.Menu;

/// <summary>
///     Represents a centralized instantiation mechanism implementing the Factory Method design pattern.
///     By encapsulating the creation logic of concrete <see cref="MenuItem" /> derivatives, this static
///     class decouples the client code from specific class implementations. This promotes adherence
///     to the Dependency Inversion Principle, as consuming components depend solely on the
///     <see cref="MenuItem" /> abstraction rather than concrete types, thereby enhancing modularity
///     and testability.
/// </summary>
public static class MenuItemFactory
{
    /// <summary>
    ///     Acts as the public entry point for product instantiation, utilizing polymorphic dispatch
    ///     to return the appropriate concrete domain object based on the specified category.
    /// </summary>
    /// <param name="category">The categorical enumeration dictating the specific product type to instantiate.</param>
    /// <param name="price">The monetary value assigned to the product.</param>
    /// <param name="preparationMinutes">The temporal duration required for product preparation.</param>
    /// <returns>A concrete <see cref="MenuItem" /> instance corresponding to the provided category.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when the provided <paramref name="category" /> does not map to a known concrete implementation,
    ///     enforcing the fail-fast paradigm.
    /// </exception>
    public static MenuItem Create(ProductCategory category, decimal price, int preparationMinutes)
    {
        // The switch expression provides a concise, declarative mapping between the 
        // ProductCategory enumeration and the corresponding concrete domain classes. 
        // This construct leverages modern C# pattern matching to ensure exhaustive evaluation.
        return category switch
        {
            // The utilization of category.ToString() to derive the product name is a deliberate 
            // architectural choice. It guarantees strict referential integrity between the 
            // categorical identifier and the textual representation, eliminating manual string 
            // duplication and mitigating the risk of typographical inconsistencies.
            ProductCategory.Burger => new Burger(category.ToString(), price, preparationMinutes),
            ProductCategory.Fries => new Fries(category.ToString(), price, preparationMinutes),
            ProductCategory.Shake => new Shake(category.ToString(), price, preparationMinutes),

            // The discard pattern (_) serves as a critical exhaustive fallback mechanism. 
            // By throwing an ArgumentOutOfRangeException for any unhandled enumeration value, 
            // the implementation enforces the fail-fast principle. This guarantees that if the 
            // ProductCategory enumeration is extended in the future without a corresponding 
            // update to this factory, the system will immediately terminate with a clear 
            // diagnostic, thereby preventing silent failures or the instantiation of invalid 
            // domain objects. The inclusion of the actual invalid value in the exception 
            // payload facilitates rapid diagnostic resolution.
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown product category.")
        };
    }
}