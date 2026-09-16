// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using System.Globalization;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

/// <summary>
/// Measures what this type costs against the same arithmetic on a bare <see cref="double"/>.
/// </summary>
/// <remarks>
/// <para>
/// Every other class here answers "how long does this operation take", which is only readable next
/// to something. This one supplies the something: the primitive a caller would otherwise have
/// used. The same class, with the same loops and the same methodology, is in ktsu.SignificantNumber
/// and ktsu.Semantics, so the three answers are comparable with each other as well as with
/// <see cref="double"/>.
/// </para>
/// <para>
/// The bare method is the BenchmarkDotNet baseline, so the answer is the <c>Ratio</c> column rather
/// than two rows divided by hand. Unlike the wrapper types built on this one, a ratio here is not
/// expected to be 1.00 and is not a defect when it is not: arbitrary precision is a cost paid for
/// something a <see cref="double"/> cannot do at all. What the number is for is watching that cost
/// across releases.
/// </para>
/// <para>
/// <b>Why these are loops.</b> A single operation over operands that do not change is
/// loop-invariant, and the JIT hoists it out of the measurement entirely — for
/// <see cref="double"/> that leaves a method indistinguishable from an empty one, and a ratio
/// against an empty method means nothing. Here each iteration feeds the next, so there is nothing
/// to hoist and both sides are measurable.
/// </para>
/// <para>
/// <b>Which way the loop biases the answer.</b> Both sides pay the same counter and branch, and it
/// is a dependency chain, so most of that overlaps the arithmetic; whatever does not is added
/// equally to numerator and denominator and pulls the ratio toward 1.00. A ratio here is therefore
/// a floor on the real cost rather than the whole of it.
/// </para>
/// <para>
/// <b>Why the operands stay bounded, and why they are short.</b> This type carries as many digits
/// as the arithmetic produces, so a chain that compounded its operand would measure that growth
/// rather than the operation; both loops accumulate instead. The operands are also chosen to be
/// values a <see cref="double"/> can hold, so the two sides are doing the same arithmetic on the
/// same numbers rather than being handed different problems. How the cost grows with digits is a
/// different question, and <see cref="ArithmeticBenchmarks"/> answers it across its
/// <c>Digits</c> axis.
/// </para>
/// </remarks>
[MemoryDiagnoser]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class AbstractionCostBenchmarks
{
	/// <summary>
	/// Operations per invocation. Enough that the loop's own cost is a small share of the work,
	/// few enough that the arbitrary-precision side still finishes an iteration promptly.
	/// </summary>
	private const int Operations = 256;

	private const string SeedText = "1234.5678901234";
	private const string StepText = "0.0009765625";
	private const string OtherText = "3.14159265358979";

	private double bareSeed;
	private double bareStep;
	private double bareOther;

	// Assigned in GlobalSetup before anything is measured. Initialised here because this type
	// was a class before 2.0, where an unassigned field is a null reference the compiler
	// rejects; from 2.0 it is a struct and this is simply its default. The backfill measures
	// those releases too, so the file has to compile against both shapes.
	private PreciseNumber preciseSeed = default!;
	private PreciseNumber preciseStep = default!;
	private PreciseNumber preciseOther = default!;

	/// <summary>
	/// Prepares the operands, parsed from the same text on both sides.
	/// </summary>
	[GlobalSetup]
	public void Setup()
	{
		bareSeed = double.Parse(SeedText, CultureInfo.InvariantCulture);
		bareStep = double.Parse(StepText, CultureInfo.InvariantCulture);
		bareOther = double.Parse(OtherText, CultureInfo.InvariantCulture);

		preciseSeed = PreciseNumber.Parse(SeedText, CultureInfo.InvariantCulture);
		preciseStep = PreciseNumber.Parse(StepText, CultureInfo.InvariantCulture);
		preciseOther = PreciseNumber.Parse(OtherText, CultureInfo.InvariantCulture);
	}

	/// <summary>Adds along a chain, on a bare double.</summary>
	/// <returns>The accumulated value.</returns>
	[BenchmarkCategory("Add")]
	[Benchmark(Baseline = true, OperationsPerInvoke = Operations)]
	public double BareAdd()
	{
		double accumulator = bareSeed;

		for (int i = 0; i < Operations; i++)
		{
			accumulator += bareStep;
		}

		return accumulator;
	}

	/// <summary>Adds along the same chain, on this type.</summary>
	/// <returns>The accumulated value.</returns>
	[BenchmarkCategory("Add")]
	[Benchmark(OperationsPerInvoke = Operations)]
	public PreciseNumber PreciseAdd()
	{
		PreciseNumber accumulator = preciseSeed;

		for (int i = 0; i < Operations; i++)
		{
			accumulator += preciseStep;
		}

		return accumulator;
	}

	/// <summary>Multiplies and accumulates, on a bare double.</summary>
	/// <returns>The accumulated value.</returns>
	[BenchmarkCategory("Multiply")]
	[Benchmark(Baseline = true, OperationsPerInvoke = Operations)]
	public double BareMultiply()
	{
		double accumulator = 0d;
		double value = bareSeed;

		for (int i = 0; i < Operations; i++)
		{
			accumulator += value * bareOther;
			value += bareStep;
		}

		return accumulator;
	}

	/// <summary>Multiplies and accumulates over the same values, on this type.</summary>
	/// <returns>The accumulated value.</returns>
	[BenchmarkCategory("Multiply")]
	[Benchmark(OperationsPerInvoke = Operations)]
	public PreciseNumber PreciseMultiply()
	{
		PreciseNumber accumulator = PreciseNumber.Zero;
		PreciseNumber value = preciseSeed;

		for (int i = 0; i < Operations; i++)
		{
			accumulator += value * preciseOther;
			value += preciseStep;
		}

		return accumulator;
	}
}
