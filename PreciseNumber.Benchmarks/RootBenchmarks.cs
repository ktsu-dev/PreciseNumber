// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures the square, cube and n-th roots, and the hypotenuse built on them.
/// </summary>
/// <remarks>
/// A root is the most expensive thing this type does, because it iterates and each iteration
/// divides. Read it against <see cref="ArithmeticBenchmarks"/>'s division, which is the operation
/// inside the loop: a root that is not several times a division is not iterating enough to be
/// correct, and one that is many times it is iterating more than it needs to.
/// <para>
/// The <c>Digits</c> axis drives both the operand and the digits asked of the answer, since the
/// default precision follows the operand. That makes the first two rows of the axis meet at
/// <c>MinimumDivisionPrecision</c> and cost the same, and it is why the perfect square is the
/// slowest case at 200 digits rather than the fastest: its operand carries twice the digits its
/// root does, so it asks for twice the precision. The degree matters as much as the digit count,
/// so the n-th root is measured at a degree well above the cube root's.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class RootBenchmarks
{
	private PreciseNumber value = PreciseNumber.Zero;
	private PreciseNumber other = PreciseNumber.Zero;
	private PreciseNumber perfectSquare = PreciseNumber.Zero;

	/// <summary>
	/// Gets or sets the number of significant digits in the operand, and so in the root.
	/// </summary>
	[Params(8, 30, 200)]
	public int Digits { get; set; }

	/// <summary>
	/// Prepares the operands.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		value = Operands.Number(Digits, -10);
		other = Operands.Number(Digits, -14, offset: 7);
		perfectSquare = value.Squared();
	}

	/// <summary>The square root.</summary>
	/// <returns>The root.</returns>
	[Benchmark(Baseline = true)]
	public PreciseNumber Sqrt() => PreciseNumber.Sqrt(value);

	/// <summary>The square root of a value that has an exact one, which returns without rounding.</summary>
	/// <returns>The root.</returns>
	[Benchmark]
	public PreciseNumber SqrtOfAPerfectSquare() => PreciseNumber.Sqrt(perfectSquare);

	/// <summary>The cube root.</summary>
	/// <returns>The root.</returns>
	[Benchmark]
	public PreciseNumber Cbrt() => PreciseNumber.Cbrt(value);

	/// <summary>A root of a degree high enough that raising the estimate dominates the division.</summary>
	/// <returns>The root.</returns>
	[Benchmark]
	public PreciseNumber RootN() => PreciseNumber.RootN(value, 17);

	/// <summary>The hypotenuse, which is a square root over an exact sum of squares.</summary>
	/// <returns>The hypotenuse.</returns>
	[Benchmark]
	public PreciseNumber Hypot() => PreciseNumber.Hypot(value, other);
}
