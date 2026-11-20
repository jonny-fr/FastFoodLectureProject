namespace BurgerShopConsoleApp.Domain.Products;

/// <summary>
///     Represents a closed, finite set of mutually exclusive categorical values
///     defining the distinct product types available within the domain model.
/// </summary>
/// <remarks>
///     The utilization of an enumeration (<c>enum</c>) rather than primitive string
///     literals is a fundamental practice in robust domain modeling. It provides
///     strict type safety, preventing invalid states caused by typographical errors
///     or arbitrary string values at compile time. Furthermore, it serves as a single
///     source of truth for categorical definitions. Should the business domain expand
///     to include new product categories, the C# compiler will enforce exhaustive
///     handling of the new value across all pattern matching and switch expressions,
///     thereby preventing silent logical failures and ensuring architectural consistency.
/// </remarks>
public enum ProductCategory
{
    /// <summary>
    ///     Represents a primary savory menu item, typically characterized by a
    ///     cooked patty served within a sliced bread roll.
    /// </summary>
    Burger,

    /// <summary>
    ///     Represents a supplementary side dish consisting of fried potato strips.
    /// </summary>
    Fries,

    /// <summary>
    ///     Represents a blended, chilled beverage, typically composed of milk,
    ///     ice cream, and flavorings.
    /// </summary>
    Shake
}