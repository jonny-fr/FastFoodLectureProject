using System.Globalization;

namespace BurgerShopConsoleApp.Domain.Products;

/// <summary>
///     Represents the abstract base class for all domain entities modeling menu items.
/// </summary>
/// <remarks>
///     In Object-Oriented Programming, the <c>abstract</c> modifier precludes direct
///     instantiation of this class, enforcing that only concrete, specialized derivatives
///     (e.g., <c>Burger</c>, <c>Fries</c>) can be materialized. This architectural choice
///     establishes a foundational contract, ensuring that all menu items share a common
///     structural baseline and behavioral interface while allowing for category-specific
///     polymorphic extensions.
/// </remarks>
public abstract class MenuItem
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuItem" /> class, enforcing
    ///     strict domain invariants at the boundary of object creation.
    /// </summary>
    /// <remarks>
    ///     The <c>protected</c> access modifier restricts constructor invocation exclusively
    ///     to derived classes, preventing external consumers from bypassing specialized
    ///     factory logic or instantiating an incomplete base type. The inclusion of guard
    ///     clauses adheres to the fail-fast principle, immediately rejecting invalid state
    ///     transitions before any internal assignment occurs.
    /// </remarks>
    protected MenuItem(string name, ProductCategory category, decimal price, int preparationMinutes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(price);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(preparationMinutes);

        Name = name.Trim();
        Category = category;
        Price = price;
        PreparationMinutes = preparationMinutes;
    }

    /// <summary>
    ///     Gets the canonical name of the menu item.
    /// </summary>
    /// <remarks>
    ///     The property exposes a public getter with no setter, establishing strict
    ///     immutability. The input is sanitized via the <c>Trim()</c> method during
    ///     construction to eliminate extraneous whitespace, ensuring data consistency
    ///     throughout the object's lifecycle.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    ///     Gets the categorical classification of the menu item.
    /// </summary>
    public ProductCategory Category { get; }

    /// <summary>
    ///     Gets the monetary value of a single unit of the menu item.
    /// </summary>
    /// <remarks>
    ///     The <see cref="decimal" /> type is explicitly utilized to guarantee exact
    ///     base-10 fractional representation, mitigating the precision loss inherent
    ///     in binary floating-point types during financial calculations.
    /// </remarks>
    public decimal Price { get; }

    /// <summary>
    ///     Gets the temporal duration, in minutes, required to prepare the menu item.
    /// </summary>
    public int PreparationMinutes { get; }

    /// <summary>
    ///     Gets the descriptive text associated with the specific menu item.
    /// </summary>
    /// <remarks>
    ///     The <c>abstract</c> modifier mandates that all non-abstract derived classes
    ///     must provide a concrete implementation of this property. This enforces a
    ///     strict behavioral contract, ensuring that every concrete menu item possesses
    ///     a defined description without dictating the specific text at the base level.
    /// </remarks>
    public abstract string Description { get; }

    /// <summary>
    ///     Computes the aggregate financial value for a specified quantity of the menu item.
    /// </summary>
    /// <param name="quantity">The discrete, positive count of items to be priced.</param>
    /// <returns>The total monetary value for the specified quantity.</returns>
    /// <remarks>
    ///     The <c>virtual</c> modifier permits derived classes to override this default
    ///     computational logic (e.g., to implement category-specific volume discounts).
    ///     The method includes a guard clause to enforce the precondition that the
    ///     quantity must be strictly positive, preserving the mathematical integrity
    ///     of the calculation.
    /// </remarks>
    public virtual decimal CalculateLineTotal(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        return Price * quantity;
    }

    /// <summary>
    ///     Returns a deterministic, human-readable string representation of the menu item.
    /// </summary>
    /// <remarks>
    ///     This method overrides the default <see cref="object.ToString" /> behavior.
    ///     The utilization of <see cref="string.Create" /> in conjunction with
    ///     <see cref="CultureInfo.InvariantCulture" /> ensures high-performance,
    ///     allocation-efficient string formatting that remains entirely decoupled
    ///     from the host system's regional localization settings, thereby guaranteeing
    ///     consistent output across diverse execution environments.
    /// </remarks>
    public override string ToString()
    {
        return string.Create(CultureInfo.InvariantCulture, $"{Name} ({Price:F2} EUR)");
    }
}