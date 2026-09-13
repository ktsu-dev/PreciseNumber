// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures the four arithmetic operators plus the modulus.
/// </summary>
/// <remarks>
/// Each case uses operands with different exponents, which is the general path. The separate
/// wide-gap multiply exists because aligning exponents before multiplying used to make the cost
/// depend on how far apart the exponents were rather than on the operand sizes.
/// </remarks>
[MemoryDiagnoser]
public class ArithmeticBenchmarks
{
	private PreciseNumber left = PreciseNumber.Zero;
	private PreciseNumber right = PreciseNumber.Zero;
	private PreciseNumber wideGap = PreciseNumber.Zero;

	/// <summary>
	/// Gets or sets the number of significant digits in the operands.
	/// </summary>
	[Params(8, 30, 200)]
	public int Digits { get; set; }

	/// <summary>
	/// Prepares the operands.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		left = Operands.Number(Digits, -10);
		right = Operands.Number(Digits, -14, offset: 7);
		wideGap = Operands.Number(Digits, -400, offset: 11);
	}

	/// <summary>Addition.</summary>
	/// <returns>The sum.</returns>
	[Benchmark(Baseline = true)]
	public PreciseNumber Add() => left + right;

	/// <summary>Subtraction.</summary>
	/// <returns>The difference.</returns>
	[Benchmark]
	public PreciseNumber Subtract() => left - right;

	/// <summary>Multiplication.</summary>
	/// <returns>The product.</returns>
	[Benchmark]
	public PreciseNumber Multiply() => left * right;

	/// <summary>Multiplication where the operands' exponents are hundreds of decades apart.</summary>
	/// <returns>The product.</returns>
	[Benchmark]
	public PreciseNumber MultiplyWideExponentGap() => left * wideGap;

	/// <summary>Division.</summary>
	/// <returns>The quotient.</returns>
	[Benchmark]
	public PreciseNumber Divide() => left / right;

	/// <summary>Modulus.</summary>
	/// <returns>The remainder.</returns>
	[Benchmark]
	public PreciseNumber Mod() => left % right;

	/// <summary>Negation.</summary>
	/// <returns>The negated value.</returns>
	[Benchmark]
	public PreciseNumber Negate() => -left;

	/// <summary>Squaring, which is multiplication by self.</summary>
	/// <returns>The square.</returns>
	[Benchmark]
	public PreciseNumber Squared() => left.Squared();
}
