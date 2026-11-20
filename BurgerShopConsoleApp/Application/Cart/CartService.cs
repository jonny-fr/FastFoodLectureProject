using System.Globalization;
using BurgerShopConsoleApp.Application.Menu;
using BurgerShopConsoleApp.Application.Parsing;
using BurgerShopConsoleApp.Application.Receipts;
using BurgerShopConsoleApp.Application.Time;
using BurgerShopConsoleApp.Domain.Cart;
using BurgerShopConsoleApp.Domain.Time;

namespace BurgerShopConsoleApp.Application.Cart;

/// <summary>
///     Represents an Application Service responsible for orchestrating the shopping cart
///     use cases. In the context of Domain-Driven Design, this class acts as a coordinator
///     that manages transactions, enforces application-level business rules (such as
///     operating hours), and facilitates the translation between domain entities and
///     application-level read models (DTOs). It remains entirely agnostic to the underlying
///     infrastructure, relying on injected abstractions for temporal and catalog concerns.
/// </summary>
public sealed class CartService
{
    // The domain aggregate is instantiated internally, as the CartService is the 
    // designated owner of the cart's lifecycle within this specific application context.
    private readonly ShoppingCart _cart = new();

    // Infrastructure and domain dependencies are injected via the constructor, 
    // adhering to the Dependency Inversion Principle. This decouples the service 
    // from concrete implementations, enabling deterministic unit testing through 
    // the injection of mock clocks and stub catalogs.
    private readonly IShopClock _clock;
    private readonly IMenuCatalog _menuCatalog;
    private readonly OpeningHours _openingHours;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CartService" /> class with the
    ///     required temporal and catalog dependencies.
    /// </summary>
    /// <param name="clock">The abstracted time provider for deterministic temporal operations.</param>
    /// <param name="menuCatalog">The catalog abstraction for product resolution.</param>
    /// <param name="openingHours">
    ///     An optional domain value object defining operational constraints. If omitted,
    ///     the canonical <see cref="OpeningHours.Standard" /> configuration is utilized,
    ///     providing a sensible default while maintaining the flexibility for custom
    ///     operational schedules (e.g., holiday hours).
    /// </param>
    public CartService(
        IShopClock clock,
        IMenuCatalog menuCatalog,
        OpeningHours? openingHours = null)
    {
        // Guard clauses enforce the strict precondition that core dependencies 
        // must be provided, preventing deferred NullReferenceExceptions and 
        // failing fast upon invalid composition.
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(menuCatalog);

        _clock = clock;
        _menuCatalog = menuCatalog;

        // The null-coalescing operator ensures a valid operational constraint 
        // object is always assigned, preserving the invariant that opening hours 
        // are never null during execution.
        _openingHours = openingHours ?? OpeningHours.Standard;
    }

    /// <summary>
    ///     Exposes the current line items of the underlying shopping cart.
    /// </summary>
    /// <remarks>
    ///     This property acts as a read-only facade, delegating directly to the domain
    ///     aggregate. It prevents external mutation while allowing the presentation
    ///     layer to enumerate the cart's contents.
    /// </remarks>
    public IReadOnlyList<CartLine> Lines => _cart.Lines;

    /// <summary>
    ///     Indicates whether the cart currently contains no items.
    /// </summary>
    public bool IsEmpty => _cart.IsEmpty;

    /// <summary>
    ///     Calculates the aggregate financial subtotal of the cart.
    /// </summary>
    public decimal Subtotal => _cart.Subtotal;

    /// <summary>
    ///     Gets or sets the optional textual note associated with the entire cart.
    /// </summary>
    /// <remarks>
    ///     The property delegates state mutation to the underlying aggregate, ensuring
    ///     that the domain object remains the single source of truth for the cart's state.
    /// </remarks>
    public string Note
    {
        get => _cart.Note;
        set => _cart.Note = value;
    }

    /// <summary>
    ///     Adds a specified quantity of a product to the cart using a strongly typed integer.
    /// </summary>
    /// <param name="productName">The textual identifier of the product.</param>
    /// <param name="quantity">The discrete, positive quantity to add.</param>
    /// <exception cref="CartOperationException">
    ///     Thrown when the quantity is invalid or the product cannot be resolved in the catalog.
    /// </exception>
    public void AddProduct(string productName, int quantity)
    {
        // Application-level validation is performed prior to delegating to the domain. 
        // This ensures that untrusted boundary inputs are sanitized and translated into 
        // domain-specific exceptions before they reach the aggregate.
        if (quantity <= 0)
            throw new CartOperationException(
                FormatInvalidQuantity(quantity.ToString(CultureInfo.InvariantCulture)));

        if (!_menuCatalog.TryGetProduct(productName, out var product))
            throw new CartOperationException(FormatUnknownProduct(productName));

        _cart.Add(product, quantity);
    }

    /// <summary>
    ///     Adds a product to the cart by parsing a textual representation of the quantity.
    /// </summary>
    /// <param name="productName">The textual identifier of the product.</param>
    /// <param name="quantityText">The unparsed textual quantity.</param>
    /// <exception cref="CartOperationException">Thrown if parsing fails or the product is unknown.</exception>
    public void AddProduct(string productName, string quantityText)
    {
        // This overload bridges the gap between untrusted textual input and the 
        // strongly typed domain model. It delegates to the non-throwing TryAddProduct 
        // method and translates a false return value into a domain-specific exception, 
        // maintaining a consistent exception-based control flow for the calling boundary.
        if (!TryAddProduct(productName, quantityText, out var errorMessage))
            throw new CartOperationException(errorMessage);
    }

    /// <summary>
    ///     Attempts to add a product to the cart using textual input, employing the
    ///     standard .NET "Try" pattern to handle expected parsing failures gracefully.
    /// </summary>
    /// <param name="productName">The textual identifier of the product.</param>
    /// <param name="quantityText">The unparsed textual quantity.</param>
    /// <param name="errorMessage">
    ///     When this method returns <c>false</c>, contains a diagnostic message detailing
    ///     the failure; otherwise, contains an empty string.
    /// </param>
    /// <returns><c>true</c> if the product was successfully added; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     The "Try" pattern is explicitly provided for scenarios where invalid input is
    ///     an expected occurrence (e.g., user typos in a CLI or UI). By avoiding exceptions
    ///     for these expected failures, the application prevents the performance overhead
    ///     and control-flow disruption associated with exception handling, adhering to the
    ///     principle that exceptions should represent truly exceptional, unrecoverable states.
    /// </remarks>
    public bool TryAddProduct(string productName, string quantityText, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!InputParser.TryParseQuantity(quantityText, out var quantity))
        {
            errorMessage = FormatInvalidQuantity(quantityText);
            return false;
        }

        if (!_menuCatalog.TryGetProduct(productName, out var product))
        {
            errorMessage = FormatUnknownProduct(productName);
            return false;
        }

        _cart.Add(product, quantity);
        return true;
    }

    /// <summary>
    ///     Removes a product line from the cart based on the product name.
    /// </summary>
    /// <param name="productName">The name of the product to remove.</param>
    /// <exception cref="CartOperationException">Thrown if the product is not present in the cart.</exception>
    public void RemoveProduct(string productName)
    {
        // The underlying aggregate returns a boolean indicating success. The application 
        // service translates a failure (product not found) into a specific domain exception, 
        // ensuring that the calling boundary receives explicit feedback regarding the 
        // violation of the user's intent.
        if (!_cart.RemoveLine(productName))
            throw new CartOperationException($"Product '{productName}' is not in the cart.");
    }

    /// <summary>
    ///     Empties the cart, resetting it to its initial state.
    /// </summary>
    public void Clear()
    {
        _cart.Clear();
    }

    /// <summary>
    ///     Finalizes the current shopping cart transaction, validating business rules,
    ///     calculating temporal constraints, and materializing an immutable <see cref="Receipt" />.
    /// </summary>
    /// <returns>A fully populated, immutable <see cref="Receipt" /> representing the transaction.</returns>
    /// <exception cref="CartOperationException">
    ///     Thrown if the cart is empty or if the transaction violates operational constraints
    ///     (e.g., attempting to order outside of business hours).
    /// </exception>
    public Receipt Checkout()
    {
        // A guard clause enforces the business invariant that a transaction cannot be 
        // finalized without at least one item, preventing the generation of empty receipts.
        if (_cart.IsEmpty) throw new CartOperationException("Cannot checkout an empty cart.");

        // The current temporal state is retrieved from the injected clock abstraction. 
        // The DateTimeOffset is deconstructed into distinct DateOnly and TimeOnly structures 
        // to align with the domain's granular temporal modeling and eliminate timezone ambiguities.
        var localNow = _clock.LocalNow;
        var orderDate = DateOnly.FromDateTime(localNow.DateTime);
        var orderTime = TimeOnly.FromDateTime(localNow.DateTime);

        // Business rules regarding operational availability are delegated to the domain 
        // value object. This ensures that the logic defining "when" an order can be placed 
        // remains encapsulated within the domain layer, preserving the Single Responsibility 
        // Principle and preventing the leakage of business rules into the application service.
        if (!_openingHours.CanAcceptOrder(orderDate, orderTime))
            throw new CartOperationException(
                "The restaurant is closed. Orders are accepted Monday to Saturday from 10:00 to 21:45.");

        // The anticipated readiness time is calculated by querying the domain for the 
        // maximum preparation time among all items, ensuring the customer is provided 
        // with a realistic fulfillment estimate.
        var preparationMinutes = _cart.GetLongestPreparationMinutes();
        var readyTime = _openingHours.GetReadyTime(orderTime, preparationMinutes);

        // The domain entities (CartLine) are projected into application-level read models 
        // (ReceiptLine). The LINQ query explicitly sorts the output by category and then 
        // by name. This deterministic ordering is a critical architectural decision for 
        // presentation and testing; it guarantees that the receipt output is predictable 
        // and consistent across multiple executions, regardless of the underlying 
        // collection's insertion order.
        var lines = _cart.Lines
            .OrderBy(line => line.Item.Category)
            .ThenBy(line => line.Item.Name, StringComparer.Ordinal)
            .Select(line => new ReceiptLine(
                line.Item.Name,
                line.Quantity,
                line.Item.Price,
                line.Total))
            .ToArray();

        // The immutable receipt read model is materialized using the extracted and 
        // calculated data.
        var receipt = new Receipt(
            orderDate,
            orderTime,
            readyTime,
            lines,
            _cart.Subtotal);

        // The cart state is cleared only after the receipt has been successfully 
        // materialized. This sequence ensures that if an exception occurs during 
        // receipt generation, the cart's state is preserved, preventing data loss 
        // and maintaining transactional integrity.
        _cart.Clear();

        return receipt;
    }

    /// <summary>
    ///     Attempts to finalize the transaction, translating domain exceptions into a
    ///     boolean result and an error message.
    /// </summary>
    /// <param name="receipt">
    ///     When this method returns <c>true</c>, contains the materialized <see cref="Receipt" />;
    ///     otherwise, contains <c>null</c>.
    /// </param>
    /// <param name="errorMessage">
    ///     When this method returns <c>false</c>, contains the diagnostic message from the
    ///     caught exception; otherwise, contains an empty string.
    /// </param>
    /// <returns><c>true</c> if the checkout succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    ///     This method serves as an exception-translation boundary. While the domain and
    ///     application layers utilize exceptions to signal business rule violations, the
    ///     presentation boundary often prefers the "Try" pattern for expected user errors
    ///     (e.g., attempting to checkout an empty cart). By catching the specific
    ///     <see cref="CartOperationException" />, the service isolates expected business
    ///     failures from unexpected system faults, providing a clean API for the UI layer.
    /// </remarks>
    public bool TryCheckout(out Receipt? receipt, out string errorMessage)
    {
        receipt = null;
        errorMessage = string.Empty;

        try
        {
            receipt = Checkout();
            return true;
        }
        catch (CartOperationException exception)
        {
            // The specific exception type is caught to ensure that only anticipated 
            // business rule violations are translated into the Try pattern. Unexpected 
            // infrastructure exceptions (e.g., OutOfMemoryException) will continue to 
            // propagate up the call stack, preserving the fail-fast paradigm for 
            // critical system failures.
            errorMessage = exception.Message;
            return false;
        }
    }

    /// <summary>
    ///     Formats a standardized error message for invalid quantity inputs.
    /// </summary>
    private static string FormatInvalidQuantity(string quantityText)
    {
        // Centralizing error message formatting ensures consistency across the 
        // application and prevents the duplication of string literals, adhering 
        // to the Don't Repeat Yourself (DRY) principle.
        return $"Invalid quantity '{quantityText}'. Use a positive whole number.";
    }

    /// <summary>
    ///     Formats a standardized error message for unresolved product names.
    /// </summary>
    private static string FormatUnknownProduct(string productName)
    {
        return $"Product '{productName}' is not on the menu.";
    }
}