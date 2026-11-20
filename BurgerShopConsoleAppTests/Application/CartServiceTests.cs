using BurgerShopConsoleApp.Application.Cart;
using BurgerShopConsoleApp.Application.Menu;
using BurgerShopConsoleApp.Application.Time;
using FakeItEasy;
using FluentAssertions;

namespace BurgerShopConsoleAppTests.Application;

/// <summary>
///     Encapsulates the automated verification suite for the <see cref="CartService" />
///     application service. This test class validates the orchestration of domain entities,
///     the enforcement of business invariants (such as operating hours and product availability),
///     and the correct interaction with injected infrastructure dependencies.
/// </summary>
/// <remarks>
///     By testing the application service, the suite verifies the translation of boundary
///     inputs into domain operations and the subsequent materialization of read models.
///     The methodology strictly isolates the service from non-deterministic infrastructure
///     components, ensuring that test outcomes are entirely reproducible.
/// </remarks>
public sealed class CartServiceTests
{
    /// <summary>
    ///     Defines a deterministic temporal fixture representing a valid operational window
    ///     (Saturday at noon).
    /// </summary>
    /// <remarks>
    ///     The explicit declaration of temporal baselines as <c>static readonly</c> fields
    ///     eliminates reliance on the physical system clock. This guarantees that time-dependent
    ///     business rules are evaluated against a fixed, predictable reference point, ensuring
    ///     test determinism across varying execution environments and times of day.
    /// </remarks>
    private static readonly DateTimeOffset OpenSaturdayNoon =
        new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    ///     Defines a temporal fixture representing a closed operational window
    ///     (Saturday late night), utilized to validate the enforcement of closing time constraints.
    /// </summary>
    private static readonly DateTimeOffset ClosedSaturdayNight =
        new(2026, 11, 7, 23, 0, 0, TimeSpan.Zero);

    /// <summary>
    ///     Defines a temporal fixture representing a Sunday, utilized to validate the
    ///     enforcement of weekly calendrical closure constraints.
    /// </summary>
    private static readonly DateTimeOffset SundayNoon =
        new(2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    ///     Acts as a test factory method that materializes the System Under Test (SUT)
    ///     alongside a mocked temporal dependency.
    /// </summary>
    /// <param name="localNow">The deterministic time to be returned by the mocked clock.</param>
    /// <returns>A tuple containing the configured <see cref="CartService" /> and its mocked <see cref="IShopClock" />.</returns>
    /// <remarks>
    ///     The utilization of a mocking framework to substitute the <see cref="IShopClock" />
    ///     interface demonstrates the practical application of the Dependency Inversion Principle.
    ///     By injecting a test double, the application service is decoupled from the physical
    ///     system clock, enabling the precise manipulation of temporal state required to
    ///     deterministically test time-dependent business logic. The use of a C# tuple return
    ///     type facilitates clean variable deconstruction in the test methods, minimizing boilerplate.
    /// </remarks>
    private static (CartService Service, IShopClock Clock) CreateService(DateTimeOffset localNow)
    {
        // A fake implementation of the IShopClock interface is generated. 
        // This test double intercepts calls to the LocalNow property, returning 
        // the deterministic value provided by the test context.
        var clock = A.Fake<IShopClock>();

        A.CallTo(() => clock.LocalNow).Returns(localNow);

        // The application service is instantiated with the mocked clock and a 
        // standard, fully populated menu catalog, establishing a known baseline state.
        var service = new CartService(clock, MenuCatalogFactory.CreateStandard());

        return (service, clock);
    }

    /// <summary>
    ///     Validates the successful state mutation of the cart when a product is added
    ///     using a strongly typed integer quantity.
    /// </summary>
    [Fact]
    public void AddProduct_with_integer_quantity_adds_line()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);

        service.AddProduct("Burger", 2);

        // The assertion verifies both the presence of the line item and the exact 
        // quantitative state resulting from the addition operation.
        service.Lines.Should().ContainSingle().Which.Quantity.Should().Be(2);
    }

    /// <summary>
    ///     Validates the boundary translation mechanism by verifying that a textual
    ///     representation of a quantity is correctly parsed and applied to the cart state.
    /// </summary>
    [Fact]
    public void AddProduct_with_string_quantity_parses_text()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);

        service.AddProduct("Fries", "3");

        service.Lines.Should().ContainSingle().Which.Quantity.Should().Be(3);
    }

    /// <summary>
    ///     Validates the fail-fast mechanism of the application service when an invalid
    ///     integer quantity is provided.
    /// </summary>
    /// <remarks>
    ///     The invocation of the method is deferred within a lambda expression. This
    ///     structural requirement is mandated by the assertion framework to accurately
    ///     intercept and evaluate the exception thrown during execution. Furthermore,
    ///     the use of wildcard matching in the message assertion verifies the semantic
    ///     intent of the error without tightly coupling the test to the exact string literal,
    ///     thereby reducing test fragility during minor copywriting adjustments.
    /// </remarks>
    [Fact]
    public void AddProduct_with_zero_quantity_fails()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);

        var act = () => service.AddProduct("Burger", 0);

        act.Should().Throw<CartOperationException>().WithMessage("*quantity*");
    }

    /// <summary>
    ///     Validates the rejection of unparseable textual quantity inputs, ensuring that
    ///     boundary translation failures are correctly escalated into domain-specific exceptions.
    /// </summary>
    [Fact]
    public void AddProduct_with_invalid_quantity_text_fails()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);

        var act = () => service.AddProduct("Burger", "abc");

        act.Should().Throw<CartOperationException>().WithMessage("*quantity*");
    }

    /// <summary>
    ///     Validates the enforcement of product availability invariants by verifying that
    ///     attempts to add unrecognized products are rejected.
    /// </summary>
    [Fact]
    public void AddProduct_with_unknown_product_fails()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);

        var act = () => service.AddProduct("Pizza", 1);

        act.Should().Throw<CartOperationException>().WithMessage("*not on the menu*");
    }

    /// <summary>
    ///     Validates the business invariant prohibiting the finalization of an empty transaction.
    /// </summary>
    [Fact]
    public void Checkout_empty_cart_fails()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);

        var act = () => service.Checkout();

        act.Should().Throw<CartOperationException>().WithMessage("*empty*");
    }

    /// <summary>
    ///     Validates the enforcement of daily temporal boundaries by verifying that checkout
    ///     operations are rejected outside the designated operational window.
    /// </summary>
    [Fact]
    public void Checkout_outside_opening_hours_fails()
    {
        var (service, _) = CreateService(ClosedSaturdayNight);
        service.AddProduct("Burger", 1);

        var act = () => service.Checkout();

        act.Should().Throw<CartOperationException>().WithMessage("*closed*");
    }

    /// <summary>
    ///     Validates the enforcement of weekly calendrical boundaries by verifying that
    ///     checkout operations are rejected on designated closure days.
    /// </summary>
    [Fact]
    public void Checkout_on_sunday_fails()
    {
        var (service, _) = CreateService(SundayNoon);
        service.AddProduct("Burger", 1);

        var act = () => service.Checkout();

        act.Should().Throw<CartOperationException>().WithMessage("*closed*");
    }

    /// <summary>
    ///     Validates the comprehensive success path of the checkout operation, verifying
    ///     the correct materialization of the read model, the enforcement of state reset,
    ///     and the precise interaction with injected dependencies.
    /// </summary>
    /// <remarks>
    ///     This test serves as a critical integration point within the application layer.
    ///     It verifies that the service correctly queries the temporal dependency, delegates
    ///     business rule evaluation to the domain, projects domain entities into an immutable
    ///     read model, and finally resets the aggregate state. The explicit verification of
    ///     the mock's interaction history ensures that the service relies on the injected
    ///     abstraction rather than bypassing it.
    /// </remarks>
    [Fact]
    public void Checkout_during_opening_hours_creates_receipt_and_clears_cart()
    {
        var (service, clock) = CreateService(OpenSaturdayNoon);

        service.AddProduct("Burger", 2);
        service.AddProduct("Fries", 1);

        var receipt = service.Checkout();

        // Verifies the accurate extraction and formatting of temporal data into the read model.
        receipt.OrderDate.Should().Be(new DateOnly(2026, 11, 7));
        receipt.OrderTime.Should().Be(new TimeOnly(12, 0));

        // Verifies the correct calculation of the readiness timestamp based on domain rules.
        receipt.ReadyTime.Should().Be(new TimeOnly(12, 8));

        // Verifies the precise financial aggregation.
        receipt.Total.Should().Be(14.60m);
        receipt.Lines.Should().HaveCount(2);

        // Verifies the state reset invariant: the cart must be empty post-checkout.
        service.IsEmpty.Should().BeTrue();

        // Verifies the interaction with the mocked dependency, ensuring the service 
        // explicitly queried the current time exactly once during the operation.
        A.CallTo(() => clock.LocalNow).MustHaveHappenedOnceExactly();
    }

    /// <summary>
    ///     Validates the non-exception-based control flow paradigm provided by the "Try"
    ///     pattern for handling expected boundary input failures.
    /// </summary>
    /// <remarks>
    ///     This test confirms that the application service provides an alternative API
    ///     surface for scenarios where invalid input is an expected occurrence. By returning
    ///     a boolean result and populating an output parameter with a diagnostic message,
    ///     the service avoids the performance overhead of exception generation for routine
    ///     user errors, adhering to the principle that exceptions should represent truly
    ///     exceptional states.
    /// </remarks>
    [Fact]
    public void TryAddProduct_reports_invalid_quantity_through_out_parameter()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);

        var success = service.TryAddProduct("Burger", "0", out var errorMessage);

        success.Should().BeFalse();
        errorMessage.Should().Contain("quantity");
    }

    /// <summary>
    ///     Validates the "Try" pattern implementation for the checkout operation, ensuring
    ///     that business rule violations are translated into structured feedback rather than
    ///     control-flow disruptions.
    /// </summary>
    [Fact]
    public void TryCheckout_reports_empty_cart_through_out_parameter()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);

        var success = service.TryCheckout(out var receipt, out var errorMessage);

        success.Should().BeFalse();
        receipt.Should().BeNull();
        errorMessage.Should().Contain("empty");
    }

    /// <summary>
    ///     Validates the state mutation and invariant transition when an existing product
    ///     is explicitly removed from the cart.
    /// </summary>
    [Fact]
    public void RemoveProduct_removes_existing_line()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);
        service.AddProduct("Burger", 1);

        service.RemoveProduct("Burger");

        service.IsEmpty.Should().BeTrue();
    }

    /// <summary>
    ///     Validates the enforcement of state consistency by verifying that attempts to
    ///     remove a non-existent product are rejected as operational failures.
    /// </summary>
    [Fact]
    public void RemoveProduct_for_unknown_product_fails()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);

        var act = () => service.RemoveProduct("Fries");

        act.Should().Throw<CartOperationException>().WithMessage("*not in the cart*");
    }

    /// <summary>
    ///     Validates the lifecycle and scope of aggregate-level metadata, ensuring that
    ///     the order note is correctly maintained and appropriately purged during state reset.
    /// </summary>
    [Fact]
    public void Note_can_be_set_and_is_cleared_with_cart()
    {
        var (service, _) = CreateService(OpenSaturdayNoon);

        service.Note = "No ketchup";

        service.Note.Should().Be("No ketchup");

        service.Clear();

        // Verifies that the comprehensive reset mechanism purges both the line items 
        // and the associated aggregate metadata, preventing state leakage.
        service.Note.Should().BeEmpty();
    }
}