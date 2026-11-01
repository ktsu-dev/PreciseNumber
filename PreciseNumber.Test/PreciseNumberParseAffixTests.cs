// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;

/// <summary>
/// Covers the parts of a number <see cref="NumberStyles"/> allows around its digits: parentheses, a
/// trailing sign, and a currency symbol. Each case is checked against <see cref="decimal"/> parsing
/// the same text with the same style and format, so the expectation is the BCL's rather than ours.
/// </summary>
[TestClass]
public class PreciseNumberParseAffixTests
{
	// Hand-built formats keep these tests independent of the ICU data installed on the machine.
	// Dollars is shaped like en-US, Euros like de-DE, which writes the symbol after a no-break space.
	private static readonly NumberFormatInfo Dollars = new()
	{
		CurrencySymbol = "$",
		CurrencyDecimalSeparator = ".",
		CurrencyGroupSeparator = ",",
		NumberDecimalSeparator = ".",
		NumberGroupSeparator = ",",
	};

	private static readonly NumberFormatInfo Euros = new()
	{
		CurrencySymbol = "€",
		CurrencyDecimalSeparator = ",",
		CurrencyGroupSeparator = ".",
		NumberDecimalSeparator = ",",
		NumberGroupSeparator = ".",
	};

	private static NumberFormatInfo Format(string name) => name switch
	{
		"Dollars" => Dollars,
		"Euros" => Euros,
		_ => NumberFormatInfo.InvariantInfo,
	};

	[TestMethod]
	[DataRow("(1.5)", NumberStyles.Any, "Invariant")]
	[DataRow("(1.5)", NumberStyles.AllowParentheses | NumberStyles.AllowDecimalPoint, "Invariant")]
	[DataRow("1.5-", NumberStyles.Any, "Invariant")]
	[DataRow("1.5-", NumberStyles.AllowTrailingSign | NumberStyles.AllowDecimalPoint, "Invariant")]
	[DataRow("1.5+", NumberStyles.Any, "Invariant")]
	[DataRow("1.5E2-", NumberStyles.Any, "Invariant")]
	[DataRow("¤1.5", NumberStyles.Any, "Invariant")]
	[DataRow("$1,234.50", NumberStyles.Currency, "Dollars")]
	[DataRow("-$1,234.50", NumberStyles.Any, "Dollars")]
	[DataRow("$-1,234.50", NumberStyles.Any, "Dollars")]
	[DataRow("($1,234.50)", NumberStyles.Any, "Dollars")]
	[DataRow("1.234,50 €", NumberStyles.Currency, "Euros")]
	[DataRow("-1.234,50 €", NumberStyles.Currency, "Euros")]
	[DataRow("1.234,50 €-", NumberStyles.Currency, "Euros")]
	public void TestParseAcceptsWhatDecimalAccepts(string text, NumberStyles style, string format)
	{
		NumberFormatInfo numberFormat = Format(format);
		PreciseNumber expected = decimal.Parse(text, style, numberFormat).ToPreciseNumber();

		Assert.AreEqual(expected, PreciseNumber.Parse(text, style, numberFormat), $"Parse(\"{text}\", {style}) is wrong");
		Assert.IsTrue(PreciseNumber.TryParse(text, style, numberFormat, out PreciseNumber parsed), $"TryParse(\"{text}\", {style}) failed");
		Assert.AreEqual(expected, parsed, $"TryParse(\"{text}\", {style}) is wrong");
	}

	[TestMethod]
	public void TestParseAcceptsANoBreakSpaceBeforeACurrencySymbol()
	{
		// Windows' de-DE and fr-FR write "1.234,50 €" with U+00A0 before the symbol. decimal only takes
		// that from a culture whose own separators use U+00A0, but a copied amount should parse the same
		// whether its space is typed or rendered, as group separators already do here.
		Assert.AreEqual(PreciseNumber.Parse("-1234.5", CultureInfo.InvariantCulture), PreciseNumber.Parse("-1.234,50\u00A0€", NumberStyles.Currency, Euros));
		Assert.AreEqual(PreciseNumber.Parse("1.5", CultureInfo.InvariantCulture), PreciseNumber.Parse("€\u00A01,5", NumberStyles.Currency, Euros));
	}

	[TestMethod]
	[DataRow("(1.5)", NumberStyles.Float, "Invariant")]
	[DataRow("1.5-", NumberStyles.Float, "Invariant")]
	[DataRow("$1.5", NumberStyles.Number, "Dollars")]
	[DataRow("1,5 €", NumberStyles.Number, "Euros")]
	[DataRow("(-1.5)", NumberStyles.Any, "Invariant")]
	[DataRow("-1.5-", NumberStyles.Any, "Invariant")]
	[DataRow("$1.5$", NumberStyles.Any, "Dollars")]
	[DataRow("()", NumberStyles.Any, "Invariant")]
	[DataRow("$", NumberStyles.Any, "Dollars")]
	[DataRow("-", NumberStyles.Any, "Invariant")]
	public void TestParseRejectsWhatDecimalRejects(string text, NumberStyles style, string format)
	{
		NumberFormatInfo numberFormat = Format(format);
		Assert.IsFalse(decimal.TryParse(text, style, numberFormat, out _), $"decimal accepts \"{text}\" under {style}, so the case is wrong");

		Assert.IsFalse(PreciseNumber.TryParse(text, style, numberFormat, out _), $"TryParse(\"{text}\", {style}) accepted it");
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse(text, style, numberFormat), $"Parse(\"{text}\", {style}) accepted it");
	}
}
