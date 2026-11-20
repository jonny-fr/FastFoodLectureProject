using System.Globalization;
using BurgerShopConsoleApp.Application.Cart;
using BurgerShopConsoleApp.Application.Menu;
using BurgerShopConsoleApp.Application.Parsing;
using BurgerShopConsoleApp.Application.Receipts;
using BurgerShopConsoleApp.Application.Time;
using FakeItEasy;
using FluentAssertions;
using Reqnroll;

namespace BurgerShopConsoleAppAcceptanceTests.StepDefinitions;

/// <summary>
///     Represents the SpecFlow step definition binding for the shopping cart acceptance tests.
/// </summary>
/// <remarks>
///     In Behavior-Driven Development (BDD), step definition classes act as the translation layer
///     between the ubiquitous language expressed in Gherkin feature files and the executable
///     application code. The <c>[Binding]</c> attribute instructs the SpecFlow runtime to
///     discover and register this class's methods as executable step implementations.
///     The SpecFlow runtime instantiates the binding class once per scenario; therefore,
///     instance fields are utilized to maintain state continuity across the sequential
///     execution of Given, When, and Then steps within a single test scenario.
/// </remarks>
[Binding]
public sealed class ShoppingCartSteps
{
    // The temporal dependency is mocked to inject deterministic time values, 
    // ensuring that time-dependent business rules (e.g., opening hours) are 
    // evaluated consistently regardless of the physical execution environment.
    private readonly IShopClock _clock = A.Fake<IShopClock>();

    // State variables to capture the outcome of the operation under test. 
    // Separating the exception capture from the immediate assertion allows the 
    // BDD scenario to explicitly define failure expectations in the 'Then' phase.
    private Exception? _error;
    private Receipt? _receipt;

    // The application service is instantiated during the 'Given' phase, 
    // serving as the System Under Test (SUT) for the subsequent 'When' actions.
    private CartService? _service;

    /// <summary>
    ///     Establishes the deterministic temporal context for the scenario.
    /// </summary>
    /// <remarks>
    ///     The regular expression capture group <c>(.*)</c> extracts the temporal string
    ///     from the Gherkin step. The parsed <see cref="DateTimeOffset" /> is configured
    ///     as the return value for the mocked clock's <c>LocalNow</c> property, effectively
    ///     freezing time for the duration of the scenario.
    /// </remarks>
    [Given("""
           the shop time is "(.*)"
           """)]
    public void GivenTheShopTimeIs(string dateTimeText)
    {
        // The input string is parsed using the application's boundary parser to ensure 
        // that the test setup adheres to the exact same canonical format expected by the system.
        InputParser.TryParseLocalDateTime(dateTimeText, out var localNow)
            .Should().BeTrue($"test time '{dateTimeText}' must be parseable");

        A.CallTo(() => _clock.LocalNow).Returns(localNow);
    }

    /// <summary>
    ///     Materializes the System Under Test (SUT) with a standard, fully populated catalog.
    /// </summary>
    [Given("the standard menu is available")]
    public void GivenTheStandardMenuIsAvailable()
    {
        _service = new CartService(_clock, MenuCatalogFactory.CreateStandard());
    }

    /// <summary>
    ///     Executes the addition of a product using a strongly typed integer quantity.
    /// </summary>
    /// <remarks>
    ///     The regular expression <c>(\d+)</c> captures the numerical quantity, while <c>(.*)</c>
    ///     captures the product name. The action is delegated to the <c>Execute</c> wrapper
    ///     to intercept potential domain exceptions, preventing immediate test failure and
    ///     allowing the scenario to verify the error in a subsequent 'Then' step.
    /// </remarks>
    [When("""
          I add (\d+) "(.*)"
          """)]
    public void WhenIAddD(int quantity, string product)
    {
        Execute(() => _service!.AddProduct(product, quantity));
    }

    /// <summary>
    ///     Executes the addition of a product using a textual representation of the quantity.
    /// </summary>
    [When("""I add "(.*)" "(.*)" as text""")]
    public void WhenIAddAsText(string quantityText, string product)
    {
        Execute(() => _service!.AddProduct(product, quantityText));
    }

    /// <summary>
    ///     Executes the removal of a product from the cart.
    /// </summary>
    [When("""
          I remove "(.*)"
          """)]
    public void WhenIRemove(string product)
    {
        Execute(() => _service!.RemoveProduct(product));
    }

    /// <summary>
    ///     Executes the finalization of the shopping cart transaction.
    /// </summary>
    [When("I checkout")]
    public void WhenICheckout()
    {
        Execute(() => _receipt = _service!.Checkout());
    }

    /// <summary>
    ///     Validates that the preceding 'When' action completed without raising an exception.
    /// </summary>
    [Then("the operation should succeed")]
    public void ThenTheOperationShouldSucceed()
    {
        _error.Should().BeNull();
    }

    /// <summary>
    ///     Validates the financial outcome of the transaction.
    /// </summary>
    /// <remarks>
    ///     The expected monetary value is extracted from the feature file as a string and
    ///     parsed into a <see cref="decimal" /> using the invariant culture. This ensures
    ///     that the assertion compares exact base-10 fractional values, preventing precision
    ///     anomalies that could arise from locale-specific decimal separators.
    /// </remarks>
    [Then("""the receipt total should be "(.*)" EUR""")]
    public void ThenTheReceiptTotalShouldBeEur(string expectedText)
    {
        decimal.TryParse(expectedText, NumberStyles.Number, CultureInfo.InvariantCulture, out var expected)
            .Should().BeTrue($"expected total '{expectedText}' must be parseable");

        _receipt!.Total.Should().Be(expected);
    }

    /// <summary>
    ///     Validates the calculated readiness timestamp of the transaction.
    /// </summary>
    [Then("""
          the receipt ready time should be "(.*)"
          """)]
    public void ThenTheReceiptReadyTimeShouldBe(string expectedText)
    {
        InputParser.TryParseTime(expectedText, out var expected)
            .Should().BeTrue($"expected time '{expectedText}' must be parseable");

        _receipt!.ReadyTime.Should().Be(expected);
    }

    /// <summary>
    ///     Validates the state reset invariant of the cart following a successful checkout.
    /// </summary>
    [Then("the cart should be empty")]
    public void ThenTheCartShouldBeEmpty()
    {
        _service!.IsEmpty.Should().BeTrue();
    }

    /// <summary>
    ///     Validates that the preceding 'When' action failed with a specific diagnostic message.
    /// </summary>
    /// <remarks>
    ///     This step asserts against the exception captured by the <c>Execute</c> wrapper.
    ///     Utilizing a partial message match ensures that the test verifies the semantic intent
    ///     of the error without tightly coupling the acceptance criteria to the exact string
    ///     literal, thereby reducing test fragility during minor copywriting adjustments.
    /// </remarks>
    [Then("""
          the operation should fail with message containing "(.*)"
          """)]
    public void ThenTheOperationShouldFailWithMessageContaining(string expectedMessage)
    {
        _error.Should().NotBeNull();
        _error!.Message.Should().Contain(expectedMessage);
    }

    /// <summary>
    ///     Acts as an exception-handling boundary for the 'When' steps.
    /// </summary>
    /// <remarks>
    ///     In BDD scenarios, a failed operation is often the expected outcome being tested.
    ///     If an exception were to propagate immediately, the test runner would mark the
    ///     scenario as failed before reaching the 'Then' assertions. By catching specific,
    ///     anticipated exception types (such as <see cref="CartOperationException" />), the
    ///     method stores the fault state in the <c>_error</c> field. This enables the
    ///     subsequent 'Then' steps to assert against the exception's presence and message,
    ///     thereby cleanly separating the execution of the action from the verification of its failure.
    /// </remarks>
    private void Execute(Action action)
    {
        _error = null;

        try
        {
            action();
        }
        catch (Exception exception) when (
            exception is CartOperationException
                or FormatException
                or ArgumentOutOfRangeException)
        {
            _error = exception;
        }
    }
}