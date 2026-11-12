// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;

/// <summary>
/// Covers the exception every domain check throws for an argument outside its function's domain.
/// Its message renders the argument, and must stay short and not throw, however far the argument's
/// exponent reaches: a fixed-point rendering of <c>1e-2147483648</c> needs more characters than a
/// string can hold.
/// </summary>
[TestClass]
public class PreciseNumberDomainErrorTests
{
	/// <summary>The longest message a domain error may carry.</summary>
	private const int MessageLengthLimit = 1_000;

	private static PreciseNumber Parse(string text) => PreciseNumber.Parse(text, CultureInfo.InvariantCulture);

	private static PreciseNumber Call(string function, PreciseNumber x) => function switch
	{
		"Log" => PreciseNumber.Log(x),
		"Log2" => PreciseNumber.Log2(x),
		"Log10" => PreciseNumber.Log10(x),
		"LogInBaseTwo" => PreciseNumber.Log(x, Parse("2")),
		"LogOfTwoInBase" => PreciseNumber.Log(Parse("2"), x),
		"LogOfTwoInBaseToFiveDigits" => PreciseNumber.Log(Parse("2"), x, 5),
		"LogP1" => PreciseNumber.LogP1(x),
		"Acosh" => PreciseNumber.Acosh(x),
		"Atanh" => PreciseNumber.Atanh(x),
		"Sqrt" => PreciseNumber.Sqrt(x),
		"RootN4" => PreciseNumber.RootN(x, 4),
		"RootN-4" => PreciseNumber.RootN(x, -4),
		"Asin" => PreciseNumber.Asin(x),
		"Acos" => PreciseNumber.Acos(x),
		"PowOfNegativeTwo" => Parse("-2").Pow(x),
		_ => throw new ArgumentException($"No such function: {function}", nameof(function)),
	};

	[TestMethod]
	[DataRow("Log", "-1e2000000000")]
	[DataRow("Log", "-1e-2000000000")]
	[DataRow("Log", "-1e-2147483648")]
	[DataRow("Log", "-1e-200000000")]
	[DataRow("LogInBaseTwo", "-1e-2147483648")]
	[DataRow("LogInBaseTwo", "-1e-100000")]
	[DataRow("LogOfTwoInBase", "-1e-2147483648")]
	[DataRow("LogOfTwoInBaseToFiveDigits", "-1e2147483647")]
	[DataRow("Log2", "-1e2000000000")]
	[DataRow("Log2", "-1e-2147483648")]
	[DataRow("Log10", "-1e2000000000")]
	[DataRow("Log10", "-1e-2147483648")]
	[DataRow("LogP1", "-1e2000000000")]
	[DataRow("Acosh", "1e-2000000000")]
	[DataRow("Acosh", "1e-2147483648")]
	[DataRow("Acosh", "-1e2000000000")]
	[DataRow("Acosh", "-1e-2147483648")]
	[DataRow("Atanh", "1e2000000000")]
	[DataRow("Atanh", "-1e2000000000")]
	[DataRow("Sqrt", "-1e2000000000")]
	[DataRow("Sqrt", "-1e-2000000000")]
	[DataRow("Sqrt", "-1e-2147483648")]
	[DataRow("RootN4", "-1e2000000000")]
	[DataRow("RootN4", "-1e-2147483648")]
	[DataRow("RootN-4", "-1e2000000000")]
	[DataRow("RootN-4", "-1e-2147483648")]
	[DataRow("Asin", "1e2000000000")]
	[DataRow("Asin", "-1e2000000000")]
	[DataRow("Acos", "1e2000000000")]
	[DataRow("Acos", "-1e2000000000")]
	[DataRow("PowOfNegativeTwo", "1e-2000000000")]
	[DataRow("PowOfNegativeTwo", "1e-2147483648")]
	[DataRow("PowOfNegativeTwo", "-1e-2147483648")]
	public void TestDomainErrorMessageIsShortForAnExtremeExponent(string function, string argument)
	{
		PreciseNumber x = Parse(argument);

		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Call(function, x));

		string message = exception.Message;
		Assert.IsLessThan(MessageLengthLimit, message.Length, $"{function}({argument}) has a {message.Length}-character message");
	}

	[TestMethod]
	public void TestDomainErrorCarriesTheArgument()
	{
		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Sqrt(Parse("-12.5")));

		Assert.AreEqual("x", exception.ParamName);
		Assert.AreEqual("-1.25E+001", exception.ActualValue);
		Assert.Contains("-1.25E+001", exception.Message);
	}

	[TestMethod]
	public void TestDomainErrorKeepsThirtyFractionalDigitsOfALongArgument()
	{
		PreciseNumber x = -PreciseNumber.Pi;

		ArgumentOutOfRangeException exception = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.Log(x));

		Assert.AreEqual(x.ToString("E30", CultureInfo.InvariantCulture), exception.ActualValue);
	}
}
