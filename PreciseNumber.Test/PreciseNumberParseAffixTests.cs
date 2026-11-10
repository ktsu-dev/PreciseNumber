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

	// Mixed and Swiss separate money differently from other numbers, as en-DE and fr-CH do. A style
	// that allows a currency symbol reads the currency separators first, and the number separators
	// too unless a symbol comes ahead of the digits.
	private static readonly NumberFormatInfo Mixed = new()
	{
		CurrencySymbol = "€",
		CurrencyDecimalSeparator = ".",
		CurrencyGroupSeparator = ",",
		NumberDecimalSeparator = ",",
		NumberGroupSeparator = ".",
	};

	private static readonly NumberFormatInfo Swiss = new()
	{
		CurrencySymbol = "CHF",
		CurrencyDecimalSeparator = ".",
		CurrencyGroupSeparator = " ",
		NumberDecimalSeparator = ",",
		NumberGroupSeparator = " ",
	};

	private static NumberFormatInfo Format(string name) => name switch
	{
		"Dollars" => Dollars,
		"Euros" => Euros,
		"Mixed" => Mixed,
		"Swiss" => Swiss,
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
	[DataRow("1.5", NumberStyles.Any, "Mixed")]
	[DataRow("1.5", NumberStyles.Currency, "Mixed")]
	[DataRow("1.25", NumberStyles.Any, "Mixed")]
	[DataRow("1.25", NumberStyles.Currency, "Mixed")]
	[DataRow("1,5 €", NumberStyles.Any, "Mixed")]
	[DataRow("1,5 €", NumberStyles.Currency, "Mixed")]
	[DataRow("1.234,5", NumberStyles.Number, "Mixed")]
	[DataRow("1.5", NumberStyles.Any, "Swiss")]
	[DataRow("1.5", NumberStyles.Currency, "Swiss")]
	[DataRow("1 234.5", NumberStyles.Any, "Swiss")]
	[DataRow("1 234.5", NumberStyles.Currency, "Swiss")]
	[DataRow("1 234,50 CHF", NumberStyles.Any, "Swiss")]
	[DataRow("1 234,50 CHF", NumberStyles.Currency, "Swiss")]
	[DataRow("CHF 1 234.50", NumberStyles.Currency, "Swiss")]
	[DataRow("€1,5", NumberStyles.Any, "Mixed")]
	public void TestParseAcceptsWhatDecimalAccepts(string text, NumberStyles style, string format)
	{
		NumberFormatInfo numberFormat = Format(format);
		PreciseNumber expected = decimal.Parse(text, style, numberFormat).ToPreciseNumber();

		Assert.AreEqual(expected, PreciseNumber.Parse(text, style, numberFormat), $"Parse(\"{text}\", {style}) is wrong");
		Assert.IsTrue(PreciseNumber.TryParse(text, style, numberFormat, out PreciseNumber parsed), $"TryParse(\"{text}\", {style}) failed");
		Assert.AreEqual(expected, parsed, $"TryParse(\"{text}\", {style}) is wrong");
	}

	[TestMethod]
	[DataRow("1.25", "Mixed")]
	[DataRow("1.5", "Swiss")]
	public void TestDefaultParseReadsCurrencySeparatorsAsDecimalDoes(string text, string format)
	{
		// Parse(s, provider) uses NumberStyles.Any, which allows a currency symbol, so a format whose
		// money is written with other separators than its numbers (en-DE, fr-CH) reads both, as decimal does.
		NumberFormatInfo numberFormat = Format(format);
		PreciseNumber expected = decimal.Parse(text, NumberStyles.Any, numberFormat).ToPreciseNumber();

		Assert.AreEqual(expected, PreciseNumber.Parse(text, numberFormat), $"Parse(\"{text}\", {format}) is wrong");
		Assert.IsTrue(PreciseNumber.TryParse(text, numberFormat, out PreciseNumber parsed), $"TryParse(\"{text}\", {format}) failed");
		Assert.AreEqual(expected, parsed, $"TryParse(\"{text}\", {format}) is wrong");
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
	[DataRow("CHF 1,5", NumberStyles.Currency, "Swiss")]
	public void TestParseRejectsWhatDecimalRejects(string text, NumberStyles style, string format)
	{
		NumberFormatInfo numberFormat = Format(format);
		Assert.IsFalse(decimal.TryParse(text, style, numberFormat, out _), $"decimal accepts \"{text}\" under {style}, so the case is wrong");

		Assert.IsFalse(PreciseNumber.TryParse(text, style, numberFormat, out _), $"TryParse(\"{text}\", {style}) accepted it");
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse(text, style, numberFormat), $"Parse(\"{text}\", {style}) accepted it");
	}
}
