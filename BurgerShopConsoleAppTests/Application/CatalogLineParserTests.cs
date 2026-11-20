using BurgerShopConsoleApp.Application.Menu;
using BurgerShopConsoleApp.Domain.Products;
using FluentAssertions;

namespace BurgerShopConsoleAppTests.Application;

/// <summary>
///     Encapsulates the automated verification suite for the <see cref="CatalogLineParser" />
///     boundary translation mechanism. This test class validates the parser's ability to
///     deserialize structured textual records into strongly typed domain objects while
///     rigorously enforcing structural and semantic invariants.
/// </summary>
/// <remarks>
///     Testing at the parsing boundary is critical for ensuring that untrusted or malformed
///     external data is systematically rejected before it can corrupt the internal state of
///     the domain model. The suite employs the standard .NET "Try" pattern verification,
///     ensuring that expected parsing failures are handled via boolean returns and diagnostic
///     messages rather than exception-driven control flow. This approach optimizes performance
///     and maintains clean execution paths for expected negative outcomes.
/// </remarks>
public sealed class CatalogLineParserTests
{
    /// <summary>
    ///     Validates the successful deserialization and polymorphic instantiation of a
    ///     domain entity from a correctly formatted textual record.
    /// </summary>
    /// <remarks>
    ///     This test verifies the complete success path of the parsing pipeline. It confirms
    ///     that the structural decomposition and semantic validation succeed. Furthermore, it
    ///     asserts that the parser correctly delegates object creation to the underlying factory
    ///     mechanism, resulting in the instantiation of the specific concrete type
    ///     (<see cref="Burger" />) rather than a generic base type, thereby preserving the
    ///     domain's polymorphic integrity and ensuring that category-specific behaviors are
    ///     correctly materialized.
    /// </remarks>
    [Fact]
    public void Valid_line_creates_product()
    {
        // The parsing operation is executed, capturing both the success indicator and 
        // the resulting domain object or error message.
        var success = CatalogLineParser.TryParse("Burger;5.90;8", out var product, out var errorMessage);

        // Asserts the primary control flow outcome: the operation succeeded.
        success.Should().BeTrue();

        // Asserts the mutual exclusivity of the success and failure states: 
        // upon success, no diagnostic error message should be generated.
        errorMessage.Should().BeNull();

        // Validates the polymorphic resolution: the parser must not merely return a 
        // generic MenuItem, but the specific concrete implementation dictated by the 
        // category token in the input string.
        product.Should().BeOfType<Burger>();

        // Validates the accurate mapping of the parsed primitive values to the 
        // strongly typed domain properties, ensuring no data loss or truncation 
        // occurred during the boundary translation.
        product.Price.Should().Be(5.90m);
        product.PreparationMinutes.Should().Be(8);
    }

    /// <summary>
    ///     Validates the fail-fast mechanisms of the parser by verifying the rejection
    ///     of structurally and semantically invalid input records.
    /// </summary>
    /// <remarks>
    ///     The utilization of the <c>[Theory]</c> attribute facilitates comprehensive
    ///     boundary value analysis and equivalence partitioning. The selected test cases
    ///     systematically target distinct failure modes within the parsing pipeline:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Unknown category: Validates semantic rejection of unmapped enumeration values.</description>
    ///         </item>
    ///         <item>
    ///             <description>Invalid price: Validates the strict enforcement of positive financial constraints.</description>
    ///         </item>
    ///         <item>
    ///             <description>Invalid preparation time: Validates the rejection of non-positive physical quantities.</description>
    ///         </item>
    ///         <item>
    ///             <description>Incorrect field count: Validates structural integrity checks prior to semantic parsing.</description>
    ///         </item>
    ///         <item>
    ///             <description>Empty string: Validates the handling of null or whitespace-only inputs.</description>
    ///         </item>
    ///     </list>
    ///     By asserting that the product is null and the error message is populated, the test
    ///     ensures that the parser provides clear, actionable diagnostic feedback without
    ///     leaking partially constructed or invalid domain objects into the application layer.
    /// </remarks>
    [Theory]
    [InlineData("Pizza;5.90;8")]
    [InlineData("Burger;abc;8")]
    [InlineData("Burger;5.90;0")]
    [InlineData("Burger;5.90")]
    [InlineData("")]
    public void Invalid_line_fails(string line)
    {
        // The parsing operation is executed against the malformed input.
        var success = CatalogLineParser.TryParse(line, out var product, out var errorMessage);

        // Asserts the primary control flow outcome: the operation failed as expected.
        success.Should().BeFalse();

        // Asserts the fail-safe guarantee: upon failure, no domain object is returned, 
        // preventing the propagation of invalid state into subsequent processing stages.
        product.Should().BeNull();

        // Asserts that a descriptive diagnostic message is provided, ensuring that the 
        // calling boundary has sufficient context to report the specific nature of the 
        // parsing failure to the end user or system log.
        errorMessage.Should().NotBeNullOrWhiteSpace();
    }
}