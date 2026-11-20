namespace BurgerShopConsoleApp.Application.Cart;

/// <summary>
///     Represents a specialized exception type designated for signaling domain-specific
///     fault conditions occurring during shopping cart manipulations.
/// </summary>
/// <remarks>
///     The introduction of a custom exception type is a fundamental practice in
///     robust application architecture. It enables calling code to distinguish between
///     general system failures (e.g., <see cref="NullReferenceException" />,
///     <see cref="IOException" />) and specific business rule violations or application-level
///     constraints. By catching this specific type, the control flow can implement targeted
///     recovery or user-feedback mechanisms without inadvertently masking unrelated infrastructure faults.
///     The class is declared as <c>sealed</c> to preclude further inheritance. In modern
///     .NET architecture, custom exceptions are typically sealed unless a complex, multi-tiered
///     exception hierarchy is strictly required. Sealing the class enforces a flat exception
///     model, simplifies the type hierarchy, and yields marginal performance optimizations
///     during runtime type resolution.
///     The utilization of the C# primary constructor syntax provides a concise mechanism
///     for passing the exception message directly to the base <see cref="Exception" /> class
///     constructor, eliminating the need for boilerplate constructor definitions while
///     maintaining full compatibility with the standard exception infrastructure.
/// </remarks>
public sealed class CartOperationException(string message) : Exception(message);