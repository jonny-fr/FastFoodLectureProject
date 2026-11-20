namespace BurgerShopConsoleApp.Application.Receipts;

/// <summary>
///     Represents an immutable, value-semantic data structure encapsulating the
///     financial and descriptive details of a single line item on a customer receipt.
/// </summary>
/// <remarks>
///     The type declaration synthesizes three critical C# modifiers to optimize both
///     memory management and semantic correctness for high-volume reporting scenarios:
///     <list type="bullet">
///         <item>
///             <term>
///                 <c>struct</c>
///             </term>
///             <description>
///                 Designates the type as a value type. This ensures that instances
///                 are typically allocated on the call stack rather than the managed heap,
///                 thereby minimizing Garbage Collection (GC) pressure when generating receipts
///                 containing numerous line items.
///             </description>
///         </item>
///         <item>
///             <term>
///                 <c>record</c>
///             </term>
///             <description>
///                 Synthesizes value-based (structural) equality rather than
///                 reference-based equality. Consequently, two distinct <see cref="ReceiptLine" />
///                 instances containing identical data are evaluated as equal. It also provides
///                 compiler-generated formatting for the <c>ToString()</c> method.
///             </description>
///         </item>
///         <item>
///             <term>
///                 <c>readonly</c>
///             </term>
///             <description>
///                 Enforces strict immutability across all members. This guarantees
///                 that once a receipt line is instantiated, its state cannot be mutated,
///                 preserving the historical and financial integrity of the generated receipt.
///             </description>
///         </item>
///     </list>
/// </remarks>
public readonly record struct ReceiptLine(
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);