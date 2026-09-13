// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using System.Globalization;
using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures converting between a number and its decimal text.
/// </summary>
/// <remarks>
/// Formatting is split by the sign of the exponent because a negative exponent is the case that
/// has to place a decimal separator and pad with leading zeros, which is where the work is.
/// </remarks>
[MemoryDiagnoser]
public class TextBenchmarks
{
	private PreciseNumber integral = PreciseNumber.Zero;
	private PreciseNumber fractional = PreciseNumber.Zero;
	private PreciseNumber smallMagnitude = PreciseNumber.Zero;
	private string text = string.Empty;
	private char[] buffer = [];

	/// <summary>
	/// Gets or sets the number of significant digits in the operand.
	/// </summary>
	[Params(8, 30, 200)]
	public int Digits { get; set; }

	/// <summary>
	/// Prepares the operands.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		integral = Operands.Number(Digits, 4);
		fractional = Operands.Number(Digits, -4);

		// Exponent consumes every digit, so formatting has to pad with leading zeros.
		smallMagnitude = Operands.Number(Digits, -(Digits + 10));

		text = Operands.Text(Digits, -12);
		buffer = new char[(Digits * 3) + 32];
	}

	/// <summary>Formats a value whose exponent is positive.</summary>
	/// <returns>The formatted text.</returns>
	[Benchmark(Baseline = true)]
	public string ToStringIntegral() => integral.ToString(CultureInfo.InvariantCulture);

	/// <summary>Formats a value with a fractional part.</summary>
	/// <returns>The formatted text.</returns>
	[Benchmark]
	public string ToStringFractional() => fractional.ToString(CultureInfo.InvariantCulture);

	/// <summary>Formats a value smaller than one, which needs leading zero padding.</summary>
	/// <returns>The formatted text.</returns>
	[Benchmark]
	public string ToStringSmallMagnitude() => smallMagnitude.ToString(CultureInfo.InvariantCulture);

	/// <summary>Formats straight into a caller-supplied buffer.</summary>
	/// <returns>The number of characters written.</returns>
	[Benchmark]
	public int TryFormat()
	{
		_ = fractional.TryFormat(buffer, out int charsWritten, "G".AsSpan(), CultureInfo.InvariantCulture);
		return charsWritten;
	}

	/// <summary>Parses decimal text in scientific notation.</summary>
	/// <returns>The parsed number.</returns>
	[Benchmark]
	public PreciseNumber Parse() => PreciseNumber.Parse(text, CultureInfo.InvariantCulture);
}
