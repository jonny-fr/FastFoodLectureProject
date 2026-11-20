using BurgerShopConsoleApp.Domain.Cart;
using BurgerShopConsoleApp.Domain.Products;
using FluentAssertions;

namespace BurgerShopConsoleAppTests.Domain;

/// <summary>
///     Encapsulates the automated verification suite for the <see cref="ShoppingCart" />
///     aggregate root. This test class validates the core business invariants, state
///     transitions, and derived computations of the cart entity.
/// </summary>
/// <remarks>
///     In the xUnit testing framework, a new instance of the test class is instantiated
///     for the execution of each individual test method. This lifecycle management
///     guarantees strict test isolation, ensuring that state mutations in one test
///     do not leak into subsequent tests. Consequently, the System Under Test (SUT)
///     is declared as an instance field rather than a static field.
/// </remarks>
public sealed class ShoppingCartTests
{
    /// <summary>
    ///     Defines an immutable test fixture for a burger product.
    /// </summary>
    /// <remarks>
    ///     The utilization of <c>static readonly</c> ensures that the test data is
    ///     instantiated exactly once per test assembly execution, optimizing memory
    ///     allocation. The immutability guarantees that the shared fixture cannot be
    ///     accidentally modified by any test, preserving the integrity of the test data.
    /// </remarks>
    private static readonly Burger TestBurger = new("Burger", 5.90m, 8);

    /// <summary>
    ///     Defines an immutable test fixture for a fries product, providing a distinct
    ///     categorical and financial baseline for aggregate computation tests.
    /// </summary>
    private static readonly Fries TestFries = new("Fries", 2.80m, 5);

    /// <summary>
    ///     Represents the System Under Test (SUT).
    /// </summary>
    /// <remarks>
    ///     Declared as an instance field to leverage the xUnit test lifecycle, which
    ///     provisions a fresh, unmodified <see cref="ShoppingCart" /> instance for every
    ///     test method. This enforces test independence and prevents cross-test contamination.
    /// </remarks>
    private readonly ShoppingCart _sut = new();

    /// <summary>
    ///     Validates the aggregate's internal logic for handling duplicate product additions.
    /// </summary>
    /// <remarks>
    ///     This test verifies that the cart preserves object identity for identical products
    ///     by combining their quantities into a single <see cref="CartLine" />, rather than
    ///     appending redundant line items to the internal collection. This ensures the
    ///     structural integrity and semantic correctness of the cart's contents.
    /// </remarks>
    [Fact]
    public void Add_same_product_twice_combines_lines()
    {
        _sut.Add(TestBurger, 1);

        _sut.Add(TestBurger, 2);

        // Asserts that the internal collection contains exactly one line item, 
        // confirming that duplicate additions are merged.
        _sut.Lines.Should().ContainSingle();
        // Asserts that the quantity of the merged line reflects the cumulative sum 
        // of the individual addition operations.
        _sut.Lines[0].Quantity.Should().Be(3);
    }

    /// <summary>
    ///     Validates the correctness of the aggregate's derived financial and quantitative properties.
    /// </summary>
    /// <remarks>
    ///     Derived properties must remain strictly consistent with the underlying state of the
    ///     internal collection. This test ensures that the aggregation logic correctly computes
    ///     the total monetary value and the total physical quantity across multiple distinct line items.
    /// </remarks>
    [Fact]
    public void Subtotal_sums_all_lines()
    {
        _sut.Add(TestBurger, 2);
        _sut.Add(TestFries, 1);

        // Verifies the exact decimal computation of the aggregate subtotal, 
        // reinforcing the requirement for base-10 precision in financial calculations.
        _sut.Subtotal.Should().Be(14.60m);
        // Verifies the cumulative physical quantity of all items in the cart.
        _sut.TotalQuantity.Should().Be(3);
    }

    /// <summary>
    ///     Validates the state mutation and invariant transition when an existing product is removed.
    /// </summary>
    /// <remarks>
    ///     This test confirms that the removal operation correctly modifies the internal collection
    ///     and that the aggregate accurately reflects its new empty state. The utilization of a
    ///     lowercase string ("burger") also implicitly validates the case-insensitive matching
    ///     logic inherent in the removal mechanism.
    /// </remarks>
    [Fact]
    public void Remove_existing_line_removes_it()
    {
        _sut.Add(TestBurger, 1);

        _sut.RemoveLine("burger").Should().BeTrue();
        _sut.IsEmpty.Should().BeTrue();
    }

    /// <summary>
    ///     Validates the boundary condition of the removal operation when targeting a non-existent product.
    /// </summary>
    /// <remarks>
    ///     The aggregate must handle invalid removal requests gracefully without throwing exceptions
    ///     or corrupting its state. This test verifies the fail-safe boolean return mechanism,
    ///     ensuring the caller receives explicit feedback regarding the operation's failure.
    /// </remarks>
    [Fact]
    public void Remove_unknown_line_returns_false()
    {
        _sut.RemoveLine("Fries").Should().BeFalse();
    }

    /// <summary>
    ///     Validates the aggregate's ability to compute a global constraint across all constituent entities.
    /// </summary>
    /// <remarks>
    ///     This test verifies that the cart correctly evaluates the maximum preparation time required
    ///     for the entire order. This derived metric is critical for downstream processes, such as
    ///     estimating fulfillment deadlines, and must accurately reflect the most time-consuming
    ///     item within the current cart state.
    /// </remarks>
    [Fact]
    public void Longest_preparation_time_is_taken_from_all_lines()
    {
        _sut.Add(TestFries, 1);
        _sut.Add(TestBurger, 1);

        _sut.GetLongestPreparationMinutes().Should().Be(8);
    }

    /// <summary>
    ///     Validates the comprehensive reset mechanism of the aggregate root.
    /// </summary>
    /// <remarks>
    ///     The <c>Clear</c> operation must revert the aggregate to its exact initial state.
    ///     This test ensures that both the internal collection of entities and the aggregate-level
    ///     metadata (the order note) are thoroughly purged, preventing any state leakage into
    ///     subsequent operational phases.
    /// </remarks>
    [Fact]
    public void Clear_removes_all_lines_and_note()
    {
        _sut.Add(TestBurger, 1);
        _sut.Note = "No ketchup";

        _sut.Clear();

        _sut.IsEmpty.Should().BeTrue();
        _sut.Note.Should().BeEmpty();
    }
}