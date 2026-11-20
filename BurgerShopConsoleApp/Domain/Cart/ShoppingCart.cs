using BurgerShopConsoleApp.Domain.Products;

namespace BurgerShopConsoleApp.Domain.Cart;

/// <summary>
///     Represents a shopping cart aggregate root, managing a collection of cart lines
///     and associated metadata. The class is sealed to prevent inheritance, thereby
///     preserving class invariants and ensuring predictable encapsulation of its
///     internal state.
/// </summary>
public sealed class ShoppingCart
{
    /// <summary>
    ///     The internal collection of cart lines.
    ///     The readonly modifier ensures the reference to the list cannot be reassigned
    ///     after initialization, enforcing a stable internal structure. The use of the
    ///     empty collection literal [] is a modern C# feature for concise initialization.
    /// </summary>
    private readonly List<CartLine> _lines = [];

    /// <summary>
    ///     Exposes the cart lines as a read-only collection.
    ///     Returning an IReadOnlyList&lt;T&gt; via the AsReadOnly() wrapper enforces
    ///     encapsulation by preventing external consumers from mutating the internal
    ///     state of the aggregate, while still allowing iteration and indexing.
    /// </summary>
    public IReadOnlyList<CartLine> Lines => _lines.AsReadOnly();

    /// <summary>
    ///     Represents an optional note associated with the entire shopping cart.
    ///     Initialized to an empty string to prevent null reference exceptions and
    ///     establish a known default state.
    /// </summary>
    public string Note { get; set; } = string.Empty;

    /// <summary>
    ///     A derived property indicating whether the cart contains no items.
    ///     Evaluating the Count property provides an O(1) time complexity check
    ///     for the emptiness invariant.
    /// </summary>
    public bool IsEmpty => _lines.Count == 0;

    /// <summary>
    ///     Calculates the aggregate subtotal of all cart lines.
    ///     This computed property ensures the value is always consistent with the
    ///     current state of the internal collection, avoiding the need for manual
    ///     state synchronization or caching mechanisms.
    /// </summary>
    public decimal Subtotal => _lines.Sum(line => line.Total);

    /// <summary>
    ///     Calculates the total quantity of all items within the cart.
    ///     Similar to Subtotal, this derived property evaluates on demand using
    ///     LINQ to guarantee accuracy relative to the current collection state.
    /// </summary>
    public int TotalQuantity => _lines.Sum(line => line.Quantity);

    /// <summary>
    ///     Adds a specified quantity of a menu item to the cart.
    /// </summary>
    /// <param name="item">The menu item to be added.</param>
    /// <param name="quantity">The quantity of the item to add.</param>
    /// <returns>The existing or newly created CartLine instance.</returns>
    public CartLine Add(MenuItem item, int quantity)
    {
        // Defensive programming: Guard clauses enforce preconditions immediately, 
        // adhering to the fail-fast principle. This prevents invalid state mutations 
        // and provides clear, immediate feedback to the caller regarding contract violations.
        ArgumentNullException.ThrowIfNull(item);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        // Identifies an existing line using a case-insensitive string comparison. 
        // This approach ensures robust matching regardless of input formatting variations.
        var existing = _lines.FirstOrDefault(line =>
            line.Item.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            // Mutates the existing line's state to reflect the additional quantity, 
            // preserving object identity and preventing duplicate entries for the same item.
            existing.AddQuantity(quantity);
            return existing;
        }

        // Instantiates and registers a new cart line when no matching item exists.
        var newLine = new CartLine(item, quantity);
        _lines.Add(newLine);
        return newLine;
    }

    /// <summary>
    ///     Removes a cart line matching the specified product name.
    /// </summary>
    /// <param name="productName">The name of the product to remove.</param>
    /// <returns>True if the line was found and successfully removed; otherwise, false.</returns>
    public bool RemoveLine(string productName)
    {
        // Guard clause ensuring the input string is neither null, empty, nor whitespace, 
        // maintaining method precondition integrity.
        ArgumentException.ThrowIfNullOrWhiteSpace(productName);

        // Locates the target line using a trimmed, case-insensitive comparison 
        // to maximize the probability of a successful match despite minor input discrepancies.
        var line = _lines.FirstOrDefault(line =>
            line.Item.Name.Equals(productName.Trim(), StringComparison.OrdinalIgnoreCase));

        // Attempts removal and returns the boolean result of the operation. 
        // This provides the caller with explicit feedback regarding the operation's success.
        return line is not null && _lines.Remove(line);
    }

    /// <summary>
    ///     Resets the shopping cart to its initial, empty state.
    ///     This method ensures both the collection and associated aggregate metadata
    ///     are returned to their default values, maintaining a consistent baseline state.
    /// </summary>
    public void Clear()
    {
        _lines.Clear();
        Note = string.Empty;
    }

    /// <summary>
    ///     Determines the maximum preparation time required among all items in the cart.
    /// </summary>
    /// <returns>The highest preparation time in minutes, or zero if the cart is empty.</returns>
    public int GetLongestPreparationMinutes()
    {
        // A conditional check is required prior to invoking LINQ's Max() method. 
        // Invoking Max() on an empty sequence would result in an InvalidOperationException. 
        // This pattern safely handles the edge case of an empty cart while providing 
        // a semantically appropriate default value of 0.
        return _lines.Count == 0
            ? 0
            : _lines.Max(line => line.Item.PreparationMinutes);
    }
}