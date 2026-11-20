using BurgerShopConsoleApp.Domain.Products;

namespace BurgerShopConsoleApp.Domain.Cart;

/// <summary>
///     Represents a domain entity modeling a single line item within a shopping cart aggregate.
///     This class encapsulates the association between a specific menu item and its requested
///     quantity, while providing derived financial calculations.
/// </summary>
/// <remarks>
///     The class is declared as <c>sealed</c> to preclude inheritance. This ensures that the
///     internal state management and business invariants of the cart line cannot be subverted
///     or altered by derived types, preserving predictable behavior and enabling potential
///     compiler optimizations such as devirtualization.
/// </remarks>
public sealed class CartLine
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="CartLine" /> class, establishing the
    ///     initial state and enforcing strict domain invariants at the boundary of object creation.
    /// </summary>
    /// <param name="item">The specific menu item being added to the cart.</param>
    /// <param name="quantity">The discrete, positive count of the item to be included.</param>
    public CartLine(MenuItem item, int quantity)
    {
        // Guard clauses are utilized to enforce the fail-fast principle. By validating 
        // preconditions immediately, the constructor prevents the instantiation of an 
        // object in an invalid state, thereby eliminating the risk of deferred 
        // NullReferenceExceptions or logical errors during subsequent operations.
        ArgumentNullException.ThrowIfNull(item);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        Item = item;
        Quantity = quantity;
    }

    /// <summary>
    ///     Gets the menu item associated with this cart line.
    /// </summary>
    /// <remarks>
    ///     The property exposes a public getter with no setter, establishing strict immutability
    ///     for the item reference. This ensures that once a cart line is created, its fundamental
    ///     identity (the product it represents) cannot be swapped or altered, preserving the
    ///     integrity of the transaction record.
    /// </remarks>
    public MenuItem Item { get; }

    /// <summary>
    ///     Gets or sets the quantity of the menu item in this line.
    /// </summary>
    /// <remarks>
    ///     The inclusion of a <c>private set</c> accessor is a critical encapsulation mechanism.
    ///     It restricts state mutation to the internal scope of the class, ensuring that the
    ///     quantity can only be modified through controlled, intention-revealing methods (such
    ///     as <see cref="AddQuantity" />), which can enforce additional business rules and
    ///     validation logic prior to mutation.
    /// </remarks>
    public int Quantity { get; private set; }

    /// <summary>
    ///     Gets the aggregate financial value for this specific line item.
    /// </summary>
    /// <remarks>
    ///     This is a computed property representing derived state. Rather than storing the
    ///     total as a mutable field, it calculates the value on demand by delegating to the
    ///     <see cref="MenuItem.CalculateLineTotal" /> method. This architectural choice adheres
    ///     to the Single Responsibility Principle by ensuring that pricing logic remains
    ///     encapsulated within the <see cref="MenuItem" /> entity, while guaranteeing that the
    ///     total is always perfectly synchronized with the current quantity and item price.
    /// </remarks>
    public decimal Total => Item.CalculateLineTotal(Quantity);

    /// <summary>
    ///     Increments the quantity of the menu item in this line by the specified amount.
    /// </summary>
    /// <param name="quantity">The positive integer amount to add to the existing quantity.</param>
    /// <remarks>
    ///     This method facilitates state mutation while preserving the object's identity.
    ///     Instead of creating a new <see cref="CartLine" /> instance when additional quantities
    ///     of the same item are requested, this method updates the existing instance. The
    ///     guard clause ensures that the additive amount is strictly positive, preventing
    ///     accidental decrements or invalid state transitions.
    /// </remarks>
    public void AddQuantity(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        Quantity += quantity;
    }
}