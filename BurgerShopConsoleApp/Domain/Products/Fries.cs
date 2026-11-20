namespace BurgerShopConsoleApp.Domain.Products;

/// <summary>
///     Represents a concrete, specialized domain entity modeling a fries menu item.
/// </summary>
/// <remarks>
///     The <c>sealed</c> modifier is applied to preclude further inheritance. In domain-driven
///     design, concrete entities are frequently sealed to preserve the integrity of their
///     behavioral contracts, prevent unintended state mutations by derived types, and enable
///     specific compiler optimizations (such as the devirtualization of method calls).
///     The class utilizes C# primary constructor syntax. This modern language feature
///     concisely declares the instantiation parameters and routes them directly to the
///     base class constructor, eliminating boilerplate field assignments while maintaining
///     strict encapsulation and ensuring that base-level validation logic is executed
///     prior to object materialization.
/// </remarks>
public sealed class Fries(string name, decimal price, int preparationMinutes)
    : MenuItem(name, ProductCategory.Fries, price, preparationMinutes)
{
    /// <summary>
    ///     Provides the specific, immutable descriptive text for the fries entity.
    /// </summary>
    /// <remarks>
    ///     This property fulfills the abstract contract mandated by the <see cref="MenuItem" />
    ///     base class. The expression-bodied member syntax (<c>=&gt;</c>) is employed to
    ///     define a concise, read-only property that evaluates to a constant string,
    ///     ensuring that the descriptive state remains strictly invariant throughout the
    ///     object's lifecycle without requiring a dedicated backing field.
    /// </remarks>
    public override string Description => "Golden fries";
}