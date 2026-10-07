// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;

[TestClass]
public class PreciseNumberParseCultureTests
{
	// Hand-built formats keep these tests independent of the ICU data installed on the machine (and
	// of globalization-invariant mode, where no named culture but the invariant one exists).
	// CommaDecimal is shaped like de-DE, NarrowSpaceGroups like fr-FR.
	private static readonly NumberFormatInfo CommaDecimal = new()
	{
		NumberDecimalSeparator = ",",
		NumberGroupSeparator = ".",
		NegativeSign = "−",
		PositiveSign = "+",
	};

	private static readonly NumberFormatInfo NarrowSpaceGroups = new()
	{
		NumberDecimalSeparator = ",",
		NumberGroupSeparator = "\u202F",
		NegativeSign = "-",
	};

	private static readonly NumberFormatInfo MultiCharacterSeparator = new()
	{
		NumberDecimalSeparator = "<>",
		NumberGroupSeparator = " ",
		NegativeSign = "neg",
	};

	private static PreciseNumber P(string text) => PreciseNumber.Parse(text, CultureInfo.InvariantCulture);

	[TestMethod]
	public void Parse_UsesTheProvidersDecimalSeparator()
	{
		Assert.AreEqual(P("2.5"), PreciseNumber.Parse("2,5", CommaDecimal));
		Assert.IsTrue(PreciseNumber.TryParse("2,5", CommaDecimal, out PreciseNumber parsed));
		Assert.AreEqual(P("2.5"), parsed);
	}

	[TestMethod]
	public void Parse_UsesTheProvidersNegativeSign()
	{
		Assert.AreEqual(P("-0.25"), PreciseNumber.Parse("−0,25", CommaDecimal));
		Assert.AreEqual(P("-0.25"), PreciseNumber.Parse("neg0<>25", MultiCharacterSeparator));
	}

	[TestMethod]
	public void Parse_StillAcceptsTheAsciiHyphen()
	{
		Assert.AreEqual(P("-0.25"), PreciseNumber.Parse("-0,25", CommaDecimal));
	}

	[TestMethod]
	public void Parse_RoundTripsToStringForTheSameProvider()
	{
		string[] values = ["1.5", "-0.25", "123456789.000000001", "-42", "0.0001"];
		NumberFormatInfo[] formats = [CommaDecimal, MultiCharacterSeparator, NarrowSpaceGroups, NumberFormatInfo.InvariantInfo];

		foreach (NumberFormatInfo format in formats)
		{
			foreach (string value in values)
			{
				PreciseNumber number = P(value);
				string formatted = number.ToString(format);
				Assert.AreEqual(number, PreciseNumber.Parse(formatted, format), $"'{value}' formatted as '{formatted}'");
			}
		}
	}

	[TestMethod]
	public void Parse_HonoursLeadingSignWhitespaceAndThousandsUnderNumberStylesAny()
	{
		Assert.AreEqual(P("5"), PreciseNumber.Parse("+5", CultureInfo.InvariantCulture));
		Assert.AreEqual(P("5"), PreciseNumber.Parse("  5  ", CultureInfo.InvariantCulture));
		Assert.AreEqual(P("1234567.5"), PreciseNumber.Parse("1,234,567.5", CultureInfo.InvariantCulture));
		Assert.AreEqual(P("1234567.5"), PreciseNumber.Parse("1.234.567,5", CommaDecimal));
	}

	[TestMethod]
	public void Parse_RejectsWhatTheStyleDoesNotAllow()
	{
		Assert.IsFalse(PreciseNumber.TryParse(" 5", NumberStyles.None, CultureInfo.InvariantCulture, out _));
		Assert.IsFalse(PreciseNumber.TryParse("+5", NumberStyles.None, CultureInfo.InvariantCulture, out _));
		Assert.IsFalse(PreciseNumber.TryParse("1,000", NumberStyles.Float, CultureInfo.InvariantCulture, out _));
	}

	[TestMethod]
	public void Parse_RejectsASecondDecimalSeparator()
	{
		Assert.IsFalse(PreciseNumber.TryParse("1,2,3", CommaDecimal, out _));
	}

	[TestMethod]
	public void Parse_ReadsTheExponentSignWithTheProvider()
	{
		Assert.AreEqual(P("0.015"), PreciseNumber.Parse("1,5e−2", CommaDecimal));
	}

	[TestMethod]
	[DataRow("1.5", NumberStyles.Integer)]
	[DataRow("1e5", NumberStyles.Integer)]
	[DataRow("-5", NumberStyles.None)]
	[DataRow("1e 5", NumberStyles.Float)]
	[DataRow("1e5", NumberStyles.Number)]
	[DataRow("1.5", NumberStyles.None)]
	[DataRow("1.5", NumberStyles.Float)]
	[DataRow("-1.5e-3", NumberStyles.Float)]
	[DataRow("-1.5e-3", NumberStyles.Any)]
	[DataRow("1,234.5", NumberStyles.Any)]
	[DataRow("-5", NumberStyles.Integer)]
	[DataRow("1e+5", NumberStyles.Float)]
	public void TryParse_AcceptsWhatDecimalAccepts(string text, NumberStyles style)
	{
		bool decimalParsed = decimal.TryParse(text, style, CultureInfo.InvariantCulture, out decimal expected);
		bool parsed = PreciseNumber.TryParse(text, style, CultureInfo.InvariantCulture, out PreciseNumber actual);

		Assert.AreEqual(decimalParsed, parsed, $"PreciseNumber and decimal disagree on whether \"{text}\" parses under {style}.");
		if (parsed)
		{
			Assert.AreEqual(P(expected.ToString(CultureInfo.InvariantCulture)), actual);
		}
	}

	[TestMethod]
	public void Parse_ThrowsForWhatTheStyleDoesNotAllow()
	{
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse("1.5", NumberStyles.Integer, CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse("1e5", NumberStyles.Integer, CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse("-5", NumberStyles.None, CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse("1e 5", NumberStyles.Float, CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void Parse_WithoutAStyleStillReadsFractionsExponentsAndSigns()
	{
		Assert.AreEqual(P("1.5"), PreciseNumber.Parse("1.5", CultureInfo.InvariantCulture));
		Assert.AreEqual(P("-0.0015"), PreciseNumber.Parse("-1.5e-3", CultureInfo.InvariantCulture));
		Assert.AreEqual(P("1234.5"), PreciseNumber.Parse("1,234.5", CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void Parse_ReadsACulturesNegativeSignOnlyWhenTheStyleAllowsIt()
	{
		Assert.AreEqual(P("-2.5"), PreciseNumber.Parse("−2,5", NumberStyles.Float, CommaDecimal));
		Assert.IsFalse(PreciseNumber.TryParse("−2", NumberStyles.None, CommaDecimal, out _));
		Assert.IsFalse(PreciseNumber.TryParse("-2", NumberStyles.None, CommaDecimal, out _));
	}
}
