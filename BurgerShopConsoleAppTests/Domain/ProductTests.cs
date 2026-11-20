using BurgerShopConsoleApp.Domain.Products;
using FluentAssertions;

namespace BurgerShopConsoleAppTests.Domain;

/// <summary>
///     Encapsulates the automated verification suite for the concrete <see cref="MenuItem" />
///     derivatives within the domain layer. The class is declared as <c>sealed</c> to preclude
///     inheritance, ensuring that the test lifecycle remains strictly isolated.
/// </summary>
/// <remarks>
///     The test methodology leverages the xUnit framework for test execution and discovery,
///     utilizing FluentAssertions to provide a highly readable, domain-specific language for
///     expressing behavioral expectations. The tests strictly adhere to the Arrange-Act-Assert
///     (AAA) paradigm, ensuring that each test case maintains a single, verifiable responsibility
///     and remains entirely independent of external state or execution order.
/// </remarks>
public sealed class ProductTests
{
    /// <summary>
    ///     Verifies the structural integrity and correct initialization of the <see cref="Burger" />
    ///     domain entity upon instantiation.
    /// </summary>
    /// <remarks>
    ///     The <c>[Fact]</c> attribute designates this method as a parameterless, deterministic test case.
    ///     It validates that the constructor correctly maps the provided arguments to their respective
    ///     properties and ensures that derived or default attributes (such as <see cref="MenuItem.Category" />
    ///     and <see cref="MenuItem.Description" />) are accurately assigned by the concrete implementation.
    /// </remarks>
    [Fact]
    public void Burger_has_expected_properties()
    {
        // Arrange: The domain object is instantiated with explicit, known parameters.
        var burger = new Burger("Burger", 5.90m, 8);

        // Assert: FluentAssertions is utilized to verify that each property reflects 
        // the expected state, confirming both direct parameter mapping and internal 
        // initialization logic (e.g., hardcoded descriptions and category assignments).
        burger.Name.Should().Be("Burger");
        burger.Category.Should().Be(ProductCategory.Burger);
        burger.Price.Should().Be(5.90m);
        burger.PreparationMinutes.Should().Be(8);
        burger.Description.Should().Be("Grilled burger");
    }

    /// <summary>
    ///     Validates the polymorphic behavior of the <see cref="MenuItem" /> hierarchy by
    ///     verifying that distinct concrete implementations yield their specific, hardcoded descriptions.
    /// </summary>
    [Fact]
    public void Fries_and_shake_have_distinct_descriptions()
    {
        // This test ensures that the inheritance hierarchy correctly overrides or 
        // initializes the Description property, preventing unintended state leakage 
        // or default value sharing between distinct product categories.
        new Fries("Fries", 2.80m, 5).Description.Should().Be("Golden fries");
        new Shake("Shake", 3.50m, 4).Description.Should().Be("Cold shake");
    }

    /// <summary>
    ///     Verifies the mathematical correctness of the line total calculation logic,
    ///     ensuring that the aggregate financial value strictly adheres to the defined
    ///     domain invariant (Unit Price × Quantity).
    /// </summary>
    [Fact]
    public void Line_total_is_price_times_quantity()
    {
        var fries = new Fries("Fries", 2.80m, 5);

        // The assertion validates the pure computational function, ensuring that 
        // the decimal multiplication yields the exact expected base-10 fractional 
        // result without precision loss.
        fries.CalculateLineTotal(3).Should().Be(8.40m);
    }

    /// <summary>
    ///     Validates the textual serialization format of the domain entity, ensuring
    ///     that the <c>ToString()</c> override conforms to the required presentation
    ///     contract, specifically regarding currency formatting.
    /// </summary>
    [Fact]
    public void ToString_formats_price_in_eur()
    {
        var burger = new Burger("Burger", 5.90m, 8);

        // This assertion guarantees that the string representation is deterministic 
        // and strictly adheres to the specified format, which is critical for 
        // consistent output generation across different execution environments.
        burger.ToString().Should().Be("Burger (5.90 EUR)");
    }

    /// <summary>
    ///     Validates the fail-fast mechanism of the constructor's guard clauses by
    ///     verifying that invalid monetary values are immediately rejected.
    /// </summary>
    /// <remarks>
    ///     The <c>[Theory]</c> attribute, in conjunction with <c>[InlineData]</c>,
    ///     facilitates parameterized testing. This approach allows a single test
    ///     definition to be executed multiple times with distinct input values,
    ///     significantly reducing code duplication while expanding the boundary
    ///     condition coverage.
    /// </remarks>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Price_must_be_positive(int invalidPrice)
    {
        // The Arrange phase defines the execution context as a lambda expression. 
        // This defers the execution of the constructor until the assertion phase, 
        // which is strictly required by FluentAssertions to accurately capture 
        // and evaluate the thrown exception.
        var act = () => new Burger("Burger", invalidPrice, 8);

        // The Assert phase verifies that the specific exception type is thrown, 
        // confirming that the domain invariant enforcing positive financial values 
        // is strictly maintained at the object boundary.
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    ///     Validates the constructor's guard clauses regarding the temporal preparation
    ///     duration, ensuring that non-positive integer values are rejected.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Preparation_minutes_must_be_positive(int invalidMinutes)
    {
        // The lambda expression defers the instantiation, allowing the assertion 
        // framework to intercept and verify the exception thrown during construction.
        var act = () => new Burger("Burger", 5.90m, invalidMinutes);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    ///     Validates the method-level guard clauses for the line total calculation,
    ///     ensuring that the domain logic rejects invalid physical quantities.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Quantity_for_line_total_must_be_positive(int invalidQuantity)
    {
        var burger = new Burger("Burger", 5.90m, 8);

        var act = () => burger.CalculateLineTotal(invalidQuantity);

        // This test isolates the method-level validation from the constructor-level 
        // validation, confirming that the fail-fast paradigm is consistently applied 
        // across all public entry points of the domain entity.
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}