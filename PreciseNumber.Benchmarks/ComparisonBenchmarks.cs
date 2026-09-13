// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using BenchmarkDotNet.Attributes;

/// <summary>
/// Measures ordering and equality.
/// </summary>
/// <remarks>
/// Comparison is the operation most likely to sit inside a caller's inner loop, in a sort or a
/// search, so it is the one where per-call allocation hurts most. Operands whose exponents differ
/// are separated out because aligning them is the expensive half of the work.
/// </remarks>
[MemoryDiagnoser]
public class ComparisonBenchmarks
{
	private PreciseNumber left = PreciseNumber.Zero;
	private PreciseNumber right = PreciseNumber.Zero;
	private PreciseNumber sameExponent = PreciseNumber.Zero;
	private PreciseNumber differentDecade = PreciseNumber.Zero;

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
		right = Operands.Number(Digits, -40, offset: 7);
		sameExponent = Operands.Number(Digits, -10, offset: 3);

		// Far enough apart that the two cannot overlap, which a comparison can settle without
		// looking at the significands at all.
		differentDecade = Operands.Number(Digits, 400);
	}

	/// <summary>Equality where the operands already share an exponent.</summary>
	/// <returns>Whether the operands are equal.</returns>
	[Benchmark(Baseline = true)]
	public bool EqualsSameExponent() => left == sameExponent;

	/// <summary>Equality where the operands have to be aligned first.</summary>
	/// <returns>Whether the operands are equal.</returns>
	[Benchmark]
	public bool EqualsDifferentExponent() => left == right;

	/// <summary>Equality where the operands are orders of magnitude apart.</summary>
	/// <returns>Whether the operands are equal.</returns>
	[Benchmark]
	public bool EqualsDifferentDecade() => left == differentDecade;

	/// <summary>Ordering with the less-than operator.</summary>
	/// <returns>Whether the left operand is smaller.</returns>
	[Benchmark]
	public bool LessThan() => left < right;

	/// <summary>Ordering through <see cref="IComparable{T}.CompareTo(T)"/>.</summary>
	/// <returns>The relative order of the operands.</returns>
	[Benchmark]
	public int CompareTo() => left.CompareTo(right);

	/// <summary>Ordering through <see cref="PreciseNumber.Max"/>.</summary>
	/// <returns>The larger operand.</returns>
	[Benchmark]
	public PreciseNumber Max() => PreciseNumber.Max(left, right);

	/// <summary>Hashing, which callers pay alongside equality in a dictionary or set.</summary>
	/// <returns>The hash code.</returns>
	[Benchmark]
	public int GetHashCodeBenchmark() => left.GetHashCode();
}
