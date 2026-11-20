using BurgerShopConsoleApp.Application.Menu;
using BurgerShopConsoleApp.Domain.Products;
using FluentAssertions;

namespace BurgerShopConsoleAppTests.Application;

/// <summary>
///     Encapsulates the automated verification suite for the <see cref="MenuCatalog" />
///     application service. This test class validates the integrity of the product
///     lookup mechanisms, the enforcement of structural invariants (such as product
///     uniqueness), and the robustness of the boundary translation logic.
/// </summary>
public sealed class MenuCatalogTests
{
    /// <summary>
    ///     Validates the robustness of the product lookup mechanism by verifying that
    ///     the catalog abstracts away superficial syntactic variations in the search key.
    /// </summary>
    /// <remarks>
    ///     This test confirms that the underlying canonical key normalization strategy
    ///     successfully establishes a uniform equivalence relation. By accepting inputs
    ///     with extraneous whitespace and inconsistent casing, the catalog ensures that
    ///     user-provided search queries are reliably mapped to the correct domain entity,
    ///     thereby enhancing the resilience of the application boundary to unstructured input.
    /// </remarks>
    [Fact]
    public void Product_lookup_is_case_insensitive_and_ignores_spaces()
    {
        // The catalog is materialized using the standard factory, providing a known 
        // baseline state for the lookup operation.
        var catalog = MenuCatalogFactory.CreateStandard();

        // The lookup is executed with a syntactically malformed key containing 
        // leading/trailing whitespace and lowercase characters.
        catalog.TryGetProduct(" burger ", out var product).Should().BeTrue();

        // The assertion verifies that the resolved entity is the canonical instance, 
        // confirming that the internal normalization logic correctly maps the 
        // unstructured input to the standardized domain representation.
        product!.Name.Should().Be("Burger");
    }

    /// <summary>
    ///     Validates the fail-safe behavior of the lookup mechanism when queried with
    ///     an unrecognized product identifier.
    /// </summary>
    /// <remarks>
    ///     This test ensures that the catalog adheres to the standard .NET "Try" pattern.
    ///     By returning a boolean false and a null output parameter instead of throwing
    ///     an exception, the catalog prevents the disruption of the control flow for
    ///     expected negative outcomes, such as a user querying for an unavailable item.
    /// </remarks>
    [Fact]
    public void Unknown_product_is_not_found()
    {
        var catalog = MenuCatalogFactory.CreateStandard();

        // The assertion confirms that querying for a non-existent product yields 
        // a predictable, non-exceptional failure state, preserving the stability 
        // of the calling application service.
        catalog.TryGetProduct("Pizza", out _).Should().BeFalse();
    }

    /// <summary>
    ///     Validates the enforcement of the structural invariant that prohibits the
    ///     registration of duplicate products within the catalog.
    /// </summary>
    /// <remarks>
    ///     This test critically verifies that the catalog's uniqueness constraint is
    ///     evaluated against the normalized canonical key rather than the raw textual name.
    ///     By attempting to register two burgers with syntactically distinct but
    ///     semantically identical names ("Burger" and " burger "), the test ensures that
    ///     the normalization logic correctly identifies the collision. Furthermore, it
    ///     confirms that the aggregate constructor employs a fail-fast mechanism,
    ///     immediately rejecting the invalid configuration via an
    ///     <see cref="InvalidOperationException" /> to prevent the construction of a
    ///     corrupted domain state.
    /// </remarks>
    [Fact]
    public void Duplicate_products_are_rejected()
    {
        // Two distinct instances are created with syntactically different names 
        // that resolve to the same canonical key.
        var first = new Burger("Burger", 5.90m, 8);
        var second = new Burger(" burger ", 6.00m, 8);

        // The instantiation of the catalog is deferred within a lambda expression. 
        // This is strictly required by the assertion framework to accurately capture 
        // and evaluate the exception thrown during the construction phase.
        var act = () => new MenuCatalog(new[] { first, second });

        // The assertion verifies that the structural invariant violation is 
        // immediately detected and signaled via the appropriate exception type.
        act.Should().Throw<InvalidOperationException>();
    }
}