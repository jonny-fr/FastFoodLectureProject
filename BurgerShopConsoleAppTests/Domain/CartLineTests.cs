using BurgerShopConsoleApp.Domain.Cart;
using BurgerShopConsoleApp.Domain.Products;
using FluentAssertions;

namespace BurgerShopConsoleAppTests.Domain;

/// <summary>
///     Encapsulates the automated verification suite for the <see cref="CartLine" />
///     domain entity. The class is declared as <c>sealed</c> to enforce strict test
///     isolation, preventing the inheritance of test fixtures which could lead to
///     unpredictable execution orders and shared mutable state.
/// </summary>
/// <remarks>
///     A shared, immutable test dependency is defined as a <c>static readonly</c> field.
///     This pattern optimizes memory allocation by instantiating the dependency exactly
///     once per test assembly execution, while the <c>readonly</c> modifier guarantees
///     that the test fixture remains immutable, thereby eliminating the risk of
///     cross-test contamination through accidental state mutation.
/// </remarks>
public sealed class CartLineTests
{
    private static readonly Burger TestBurger = new("Burger", 5.90m, 8);

    /// <summary>
    ///     Verifies the mathematical correctness of the derived <c>Total</c>
    ///     property, ensuring that the aggregate financial value strictly adheres to the
    ///     defined domain invariant (Unit Price × Quantity).
    /// </summary>
    /// <remarks>
    ///     This test validates the computational logic of the entity. The assertion
    ///     explicitly checks for an exact decimal value, reinforcing the
    ///     architectural mandate that financial calculations must utilize base-10
    ///     fractional representation to prevent the precision loss inherent in binary
    ///     floating-point types.
    /// </remarks>
    [Fact]
    public void Total_is_price_times_quantity()
    {
        var line = new CartLine(TestBurger, 3);

        line.Total.Should().Be(17.70m);
    }

    /// <summary>
    ///     Validates the behavioral state transition of the entity by verifying that
    ///     the <c>AddQuantity</c> method correctly mutates the internal
    ///     state while preserving object identity.
    /// </summary>
    /// <remarks>
    ///     Unlike value objects, entities and aggregate roots often possess a lifecycle
    ///     characterized by state mutations. This test ensures that the internal
    ///     invariants are correctly maintained across state transitions, confirming that
    ///     the quantity is cumulatively updated rather than overwritten.
    /// </remarks>
    [Fact]
    public void AddQuantity_increases_existing_quantity()
    {
        var line = new CartLine(TestBurger, 1);

        line.AddQuantity(2);

        line.Quantity.Should().Be(3);
    }

    /// <summary>
    ///     Validates the fail-fast mechanism of the constructor's guard clauses by
    ///     verifying that invalid physical quantities are immediately rejected at the
    ///     object creation boundary.
    /// </summary>
    /// <remarks>
    ///     The utilization of the <c>[Theory]</c> attribute in conjunction with
    ///     <c>[InlineData]</c> facilitates parameterized testing. This approach allows
    ///     a single test definition to be executed multiple times with distinct boundary
    ///     values (zero and negative integers), significantly reducing code duplication
    ///     while expanding the edge-case coverage. The lambda expression defers execution,
    ///     enabling the assertion framework to accurately capture the thrown exception.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Initial_quantity_must_be_positive(int invalidQuantity)
    {
        var act = () => new CartLine(TestBurger, invalidQuantity);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    ///     Validates the method-level guard clauses for the state mutation operation,
    ///     ensuring that the domain logic rejects invalid physical quantities during
    ///     the entity's lifecycle.
    /// </summary>
    /// <remarks>
    ///     This test isolates the method-level validation from the constructor-level
    ///     validation. It confirms that the fail-fast paradigm is consistently applied
    ///     not only at the instantiation boundary but also across all subsequent public
    ///     entry points that modify the entity's state, thereby preserving the integrity
    ///     of the internal invariants throughout the object's entire lifecycle.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Added_quantity_must_be_positive(int invalidQuantity)
    {
        var line = new CartLine(TestBurger, 1);

        var act = () => line.AddQuantity(invalidQuantity);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}