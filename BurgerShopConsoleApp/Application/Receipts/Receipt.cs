namespace BurgerShopConsoleApp.Application.Receipts;

/// <summary>
///     Represents an immutable read model encapsulating the finalized state of a customer transaction.
/// </summary>
/// <remarks>
///     The synthesis of the <c>sealed</c> and <c>record</c> modifiers is a deliberate architectural choice
///     for data transfer objects (DTOs) and read models. The <c>record</c> keyword provides compiler-generated
///     value-based (structural) equality, ensuring that two receipts with identical data are treated as equal,
///     alongside automatic immutability for its properties. The <c>sealed</c> modifier precludes inheritance,
///     which guarantees that the structural contract of the receipt cannot be altered by derived types,
///     thereby preserving predictable serialization, equality semantics, and runtime type resolution.
/// </remarks>
public sealed record Receipt(
    DateOnly OrderDate,
    TimeOnly OrderTime,
    TimeOnly ReadyTime,
    IReadOnlyList<ReceiptLine> Lines,
    decimal Total);