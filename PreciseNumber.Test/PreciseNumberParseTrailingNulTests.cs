// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;

/// <summary>
/// Covers the run of trailing <c>'\0'</c> characters the BCL parsers ignore, which text read out of a
/// fixed-size or NUL-padded buffer ends with. Each case is checked against <see cref="decimal"/>
/// parsing the same text with the same style, so the expectation is the BCL's rather than ours.
/// </summary>
[TestClass]
public class PreciseNumberParseTrailingNulTests
{
	[TestMethod]
	[DataRow("5\0", NumberStyles.None)]
	[DataRow("5\0", NumberStyles.Float)]
	[DataRow("1.5\0\0\0", NumberStyles.Float)]
	[DataRow("1e3 \0", NumberStyles.Float)]
	[DataRow("12345\0\0\0\0\0\0\0\0", NumberStyles.Integer)]
	[DataRow("-0.25\0", NumberStyles.Number)]
	[DataRow("3e1\0", NumberStyles.Float)]
	[DataRow("3e1\0\0", NumberStyles.Float)]
	public void ParseIgnoresTrailingNulsAsDecimalDoes(string text, NumberStyles style)
	{
		CultureInfo culture = CultureInfo.InvariantCulture;
		Assert.IsTrue(decimal.TryParse(text, style, culture, out decimal expected), "The case must be one decimal accepts.");
		PreciseNumber expectedNumber = expected.ToPreciseNumber();

		Assert.AreEqual(expectedNumber, PreciseNumber.Parse(text, style, culture));
		Assert.AreEqual(expectedNumber, PreciseNumber.Parse(text.AsSpan(), style, culture));

		Assert.IsTrue(PreciseNumber.TryParse(text, style, culture, out PreciseNumber fromString));
		Assert.AreEqual(expectedNumber, fromString);

		Assert.IsTrue(PreciseNumber.TryParse(text.AsSpan(), style, culture, out PreciseNumber fromSpan));
		Assert.AreEqual(expectedNumber, fromSpan);
	}

	[TestMethod]
	[DataRow("1.5\0 ", NumberStyles.Float)]
	[DataRow("\0 5", NumberStyles.Float)]
	[DataRow("\0", NumberStyles.Float)]
	[DataRow("\0\0\0", NumberStyles.Float)]
	[DataRow("5 \0", NumberStyles.None)]
	[DataRow("5\0" + "1", NumberStyles.Float)]
	public void ParseStillRejectsNulsThatAreNotTheLastCharacters(string text, NumberStyles style)
	{
		CultureInfo culture = CultureInfo.InvariantCulture;
		Assert.IsFalse(decimal.TryParse(text, style, culture, out _), "The case must be one decimal rejects.");

		Assert.IsFalse(PreciseNumber.TryParse(text, style, culture, out _));
		Assert.IsFalse(PreciseNumber.TryParse(text.AsSpan(), style, culture, out _));
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse(text, style, culture));
	}

	[TestMethod]
	[DataRow("3e1\0 ", NumberStyles.Float)]
	[DataRow("3E0\0 ", NumberStyles.Any)]
	[DataRow("3e1\0-", NumberStyles.Any)]
	[DataRow("3e1\0\0-", NumberStyles.Any)]
	[DataRow("3e1\0$", NumberStyles.Any)]
	public void ParseRejectsANulInsideTheExponent(string text, NumberStyles style)
	{
		// Invariant, but with en-US's currency symbol, so "$" is read as one.
		NumberFormatInfo culture = (NumberFormatInfo)NumberFormatInfo.InvariantInfo.Clone();
		culture.CurrencySymbol = "$";
		Assert.IsFalse(decimal.TryParse(text, style, culture, out _), "The case must be one decimal rejects.");
		Assert.IsFalse(double.TryParse(text, style, culture, out _), "The case must be one double rejects.");

		Assert.IsFalse(PreciseNumber.TryParse(text, style, culture, out _));
		Assert.IsFalse(PreciseNumber.TryParse(text.AsSpan(), style, culture, out _));
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse(text, style, culture));
	}
}
