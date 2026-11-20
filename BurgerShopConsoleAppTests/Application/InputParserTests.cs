using BurgerShopConsoleApp.Application.Parsing;
using FluentAssertions;

namespace BurgerShopConsoleAppTests.Application;

/// <summary>
///     Encapsulates the automated verification suite for the <see cref="InputParser" />
///     boundary translation utility. The test methodology systematically employs
///     equivalence partitioning and boundary value analysis to validate the parser's
///     strict adherence to canonical data formats and domain invariants. By isolating
///     these parsing mechanics, the suite ensures that unstructured textual inputs are
///     rigorously sanitized before entering the strongly typed domain layer.
/// </summary>
public sealed class InputParserTests
{
    /// <summary>
    ///     Validates the successful parsing of textual representations into strictly
    ///     positive integers.
    /// </summary>
    /// <remarks>
    ///     The inclusion of a whitespace-padded input (" 12 ") verifies the parser's
    ///     resilience to minor formatting inconsistencies typical of unstructured user
    ///     input, ensuring that extraneous whitespace does not induce parsing failures.
    /// </remarks>
    [Theory]
    [InlineData("1", 1)]
    [InlineData(" 12 ", 12)]
    public void Valid_quantity_text_is_parsed(string text, int expected)
    {
        InputParser.TryParseQuantity(text, out var quantity).Should().BeTrue();

        quantity.Should().Be(expected);
    }

    /// <summary>
    ///     Validates the rejection of inputs that violate the domain invariant requiring
    ///     physical quantities to be strictly positive.
    /// </summary>
    /// <remarks>
    ///     The test cases encompass zero, negative values, non-numeric strings, and empty
    ///     inputs. This ensures comprehensive boundary coverage and confirms that the
    ///     fail-fast mechanism correctly prevents the propagation of invalid quantitative
    ///     data into the application layer.
    /// </remarks>
    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData(" ")]
    public void Invalid_quantity_text_is_rejected(string text)
    {
        InputParser.TryParseQuantity(text, out _).Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the accurate parsing of textual monetary values into the
    ///     <see cref="decimal" /> type.
    /// </summary>
    /// <remarks>
    ///     The explicit assertion of the <c>5.90m</c> literal reinforces the architectural
    ///     mandate for base-10 fractional representation in financial computations,
    ///     ensuring that exact currency values are preserved without binary floating-point
    ///     precision loss.
    /// </remarks>
    [Fact]
    public void Valid_money_text_is_parsed()
    {
        InputParser.TryParseMoney("5.90", out var amount).Should().BeTrue();

        amount.Should().Be(5.90m);
    }

    /// <summary>
    ///     Confirms that the parsing mechanism correctly tolerates and strips leading
    ///     and trailing whitespace artifacts without compromising the numerical integrity
    ///     of the parsed monetary value.
    /// </summary>
    [Fact]
    public void Valid_money_text_trims_spaces()
    {
        InputParser.TryParseMoney(" 2.80 ", out var amount).Should().BeTrue();

        amount.Should().Be(2.80m);
    }

    /// <summary>
    ///     Ensures that invalid monetary inputs, including non-positive values and
    ///     non-numeric strings, are strictly rejected at the application boundary.
    /// </summary>
    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("")]
    public void Invalid_money_text_is_rejected(string text)
    {
        InputParser.TryParseMoney(text, out _).Should().BeFalse();
    }

    /// <summary>
    ///     Validates the parsing of time strings strictly adhering to the 24-hour
    ///     <c>HH:mm</c> convention.
    /// </summary>
    /// <remarks>
    ///     This strict format enforcement ensures temporal data integrity by rejecting
    ///     ambiguous 12-hour formats or locale-specific time representations, thereby
    ///     maintaining a uniform canonical format across the application.
    /// </remarks>
    [Fact]
    public void Valid_time_text_is_parsed()
    {
        InputParser.TryParseTime("12:30", out var time).Should().BeTrue();

        time.Should().Be(new TimeOnly(12, 30));
    }

    /// <summary>
    ///     Verifies the rejection of malformed temporal inputs.
    /// </summary>
    /// <remarks>
    ///     The test cases include out-of-bounds hours (25:00), incorrect delimiters (12.30),
    ///     and empty strings. This confirms the strict enforcement of the exact format
    ///     specification, preventing invalid temporal states from being constructed.
    /// </remarks>
    [Theory]
    [InlineData("25:00")]
    [InlineData("12.30")]
    [InlineData("")]
    public void Invalid_time_text_is_rejected(string text)
    {
        InputParser.TryParseTime(text, out _).Should().BeFalse();
    }

    /// <summary>
    ///     Confirms the successful parsing of calendar dates strictly conforming to the
    ///     ISO 8601 standard (<c>yyyy-MM-dd</c>).
    /// </summary>
    /// <remarks>
    ///     The utilization of this standardized format prevents regional localization
    ///     ambiguities, ensuring that date representations are interpreted identically
    ///     regardless of the host system's cultural settings.
    /// </remarks>
    [Fact]
    public void Valid_date_text_is_parsed()
    {
        InputParser.TryParseDate("2026-11-07", out var date).Should().BeTrue();

        date.Should().Be(new DateOnly(2026, 11, 7));
    }

    /// <summary>
    ///     Validates the rejection of non-standard or regionally formatted date strings.
    /// </summary>
    /// <remarks>
    ///     By rejecting formats such as <c>dd.MM.yyyy</c> or <c>yyyy/MM/dd</c>, the
    ///     application boundary strictly enforces a uniform, culture-agnostic date
    ///     representation, mitigating the risk of misinterpreted temporal data.
    /// </remarks>
    [Theory]
    [InlineData("07.11.2026")]
    [InlineData("2026/11/07")]
    [InlineData("")]
    public void Invalid_date_text_is_rejected(string text)
    {
        InputParser.TryParseDate(text, out _).Should().BeFalse();
    }

    /// <summary>
    ///     Verifies the parsing of combined date and time representations.
    /// </summary>
    /// <remarks>
    ///     Crucially, the assertion validates that the resulting <see cref="DateTimeOffset" />
    ///     is explicitly assigned a zero offset (<see cref="TimeSpan.Zero" />). This establishes
    ///     a canonical UTC reference point, preventing timezone localization anomalies and
    ///     ensuring consistent temporal calculations across distributed or multi-timezone environments.
    /// </remarks>
    [Fact]
    public void Valid_local_date_time_text_is_parsed()
    {
        InputParser.TryParseLocalDateTime("2026-11-07T12:30", out var value).Should().BeTrue();

        value.DateTime.Should().Be(new DateTime(2026, 11, 7, 12, 30, 0));
        value.Offset.Should().Be(TimeSpan.Zero);
    }

    /// <summary>
    ///     Ensures that structurally invalid combined temporal strings are strictly rejected.
    /// </summary>
    /// <remarks>
    ///     The test cases include inputs utilizing space delimiters instead of the ISO 8601
    ///     'T' separator, as well as strings containing out-of-bounds time values. This
    ///     guarantees that the parser adheres strictly to the defined structural contract.
    /// </remarks>
    [Theory]
    [InlineData("2026-11-07 12:30")]
    [InlineData("2026-11-07T25:30")]
    [InlineData("")]
    public void Invalid_local_date_time_text_is_rejected(string text)
    {
        InputParser.TryParseLocalDateTime(text, out _).Should().BeFalse();
    }
}