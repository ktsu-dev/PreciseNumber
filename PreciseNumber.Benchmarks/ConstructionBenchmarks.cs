// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using System.Numerics;
using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures building a <see cref="PreciseNumber"/>, which every other operation pays for because
/// each result is a new instance.
/// </summary>
/// <remarks>
/// The constructor counts significant digits and, unless told not to, strips trailing zeros. Both
/// scale with the digit count, so <see cref="Digits"/> is the parameter that matters here.
/// </remarks>
[MemoryDiagnoser]
public class ConstructionBenchmarks
{
	private BigInteger significand;
	private BigInteger significandWithTrailingZeros;

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
		significand = Operands.Significand(Digits);
		significandWithTrailingZeros = significand * BigInteger.Pow(10, Digits);
	}

	/// <summary>Builds a number, stripping trailing zeros. There are none to strip here.</summary>
	/// <returns>The constructed number.</returns>
	[Benchmark(Baseline = true)]
	public PreciseNumber Sanitizing() => PreciseNumber.CreateFromComponents(-4, significand);

	/// <summary>Builds a number whose significand is half trailing zeros.</summary>
	/// <returns>The constructed number.</returns>
	[Benchmark]
	public PreciseNumber SanitizingTrailingZeros() => PreciseNumber.CreateFromComponents(-4, significandWithTrailingZeros);

	/// <summary>Builds a number without stripping trailing zeros, so only the digit count is computed.</summary>
	/// <returns>The constructed number.</returns>
	[Benchmark]
	public PreciseNumber Unsanitized() => PreciseNumber.CreateFromComponents(-4, significand, sanitize: false);
}
