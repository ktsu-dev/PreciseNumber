// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System;
using System.Globalization;
using System.Threading.Tasks;

/// <summary>
/// Covers the functions that add or subtract one, or a reciprocal, against an intermediate whose
/// exponent lies far from zero: <c>ExpM1</c>, <c>LogP1</c>, <c>Sinh</c>, <c>Cosh</c>, <c>Asinh</c>
/// and <c>Acosh</c>.
/// </summary>
/// <remarks>
/// Exact addition aligns both operands to the smaller exponent, so <c>e^-1e7 - 1</c> used to build a
/// four-million-digit significand for an answer that rounds to <c>-1</c>, and <c>ExpM1(-1e10)</c>
/// overflowed the exponent of <c>e^-1e10</c> before it got that far. The pinned digits below were
/// produced by the exact path before it was bounded, so they show the bounded path rounds the same.
/// </remarks>
[TestClass]
public class PreciseNumberLargeArgumentTests
{
	/// <summary>
	/// Longer than any of these calls takes once bounded, and far shorter than any took before.
	/// </summary>
	private static readonly TimeSpan Budget = TimeSpan.FromSeconds(5);

	private static PreciseNumber Parse(string text) =>
		PreciseNumber.Parse(text, CultureInfo.InvariantCulture);

	/// <summary>
	/// Runs a call on the thread pool and fails as soon as it overruns <see cref="Budget"/>, rather
	/// than holding the test run until a call that takes minutes finishes.
	/// </summary>
	private static PreciseNumber WithinBudget(Func<PreciseNumber> call, string description)
	{
		Task<PreciseNumber> task = Task.Run(call);
		Assert.IsTrue(task.Wait(Budget), $"{description} took longer than {Budget.TotalSeconds} seconds");
		return task.Result;
	}

	[TestMethod]
	public void TestExpM1OfAVeryNegativeArgumentIsMinusOne()
	{
		Assert.AreEqual(PreciseNumber.NegativeOne, PreciseNumber.ExpM1(Parse("-1E10")), "ExpM1(-1e10) is not -1");
		Assert.AreEqual(PreciseNumber.NegativeOne, PreciseNumber.Exp2M1(Parse("-1E10")), "Exp2M1(-1e10) is not -1");
		Assert.AreEqual(PreciseNumber.NegativeOne, PreciseNumber.Exp10M1(Parse("-1E10")), "Exp10M1(-1e10) is not -1");
		Assert.AreEqual(PreciseNumber.NegativeOne, PreciseNumber.ExpM1(Parse("-1E10"), 200), "ExpM1(-1e10) at 200 digits is not -1");
	}

	[TestMethod]
	public void TestExpM1OfALargeArgumentFinishesQuickly()
	{
		Assert.AreEqual(PreciseNumber.NegativeOne, WithinBudget(() => PreciseNumber.ExpM1(Parse("-1E7")), "ExpM1(-1e7)"), "ExpM1(-1e7) is not -1");
		Assert.AreEqual(PreciseNumber.NegativeOne, WithinBudget(() => PreciseNumber.Exp10M1(Parse("-1E7")), "Exp10M1(-1e7)"), "Exp10M1(-1e7) is not -1");

		PreciseNumber expected = PreciseNumber.Exp(Parse("1E6"));
		PreciseNumber actual = WithinBudget(() => PreciseNumber.ExpM1(Parse("1E6")), "ExpM1(1e6)");
		Assert.AreEqual(expected, actual, "ExpM1(1e6) differs from Exp(1e6), though the one is far below its last digit");
	}

	[TestMethod]
	public void TestLogP1OfAHugeArgumentFinishesQuickly()
	{
		PreciseNumber x = Parse("1E1000000");
		Assert.AreEqual(PreciseNumber.Log(x), WithinBudget(() => PreciseNumber.LogP1(x), "LogP1(1e1000000)"), "LogP1(1e1000000) differs from Log(1e1000000)");
	}

	[TestMethod]
	public void TestSinhAndCoshOfALargeArgumentFinishQuickly()
	{
		PreciseNumber sinh = WithinBudget(() => PreciseNumber.Sinh(Parse("1E7")), "Sinh(1e7)");
		PreciseNumber cosh = WithinBudget(() => PreciseNumber.Cosh(Parse("-1E7")), "Cosh(-1e7)");

		// At this size e^-x is millions of digits below e^x, so both are e^x / 2 to every digit kept.
		Assert.AreEqual(sinh, cosh, "Sinh(1e7) and Cosh(-1e7) disagree");
		Assert.AreEqual(-sinh, WithinBudget(() => PreciseNumber.Sinh(Parse("-1E7")), "Sinh(-1e7)"), "Sinh is not odd at 1e7");
	}

	[TestMethod]
	public void TestAsinhAndAcoshOfAHugeArgumentFinishQuickly()
	{
		PreciseNumber x = Parse("1E1000000");

		// Both are ln(2x) once 1/x is far below the last digit kept.
		PreciseNumber expected = PreciseNumber.Log(Parse("2E1000000"));
		Assert.AreEqual(expected, WithinBudget(() => PreciseNumber.Asinh(x), "Asinh(1e1000000)"), "Asinh(1e1000000) is not ln(2e1000000)");
		Assert.AreEqual(-expected, WithinBudget(() => PreciseNumber.Asinh(-x), "Asinh(-1e1000000)"), "Asinh(-1e1000000) is not -ln(2e1000000)");
		Assert.AreEqual(expected, WithinBudget(() => PreciseNumber.Acosh(x), "Acosh(1e1000000)"), "Acosh(1e1000000) is not ln(2e1000000)");
	}

	[TestMethod]
	public void TestBoundedSumsKeepTheDigitsTheExactSumsGave()
	{
		Assert.AreEqual(Parse("-0.99999999999999999999999999999999999999999996279924"), PreciseNumber.ExpM1(Parse("-100"), 50), "ExpM1(-100) changed");
		Assert.AreEqual(Parse("1.9424263952412559365842088360176992193662086219516E130"), PreciseNumber.ExpM1(Parse("300"), 50), "ExpM1(300) changed");
		Assert.AreEqual(Parse("3.1622776601683793319988935444327185337195551393252E100"), PreciseNumber.Exp10M1(Parse("100.5"), 50), "Exp10M1(100.5) changed");
		Assert.AreEqual(Parse("692.08386071786388396574754062165634523662942268717"), PreciseNumber.LogP1(Parse("3.7E300"), 50), "LogP1(3.7e300) changed");
		Assert.AreEqual(Parse("9.712131976206279682921044180088496096831043109758E129"), PreciseNumber.Sinh(Parse("300"), 50), "Sinh(300) changed");
		Assert.AreEqual(Parse("1.60125985730181641832516084312451811087207753341E130"), PreciseNumber.Cosh(Parse("-300.5"), 50), "Cosh(-300.5) changed");
		Assert.AreEqual(Parse("-692.3849658106478055799981957385354499198560479429"), PreciseNumber.Asinh(Parse("-2.5E300"), 50), "Asinh(-2.5e300) changed");
		Assert.AreEqual(Parse("693.44967654764023392316347631621299731774945936819"), PreciseNumber.Acosh(Parse("7.25E300"), 50), "Acosh(7.25e300) changed");
		Assert.AreEqual(Parse("92.796550900321772670136890308832744872119559679511"), PreciseNumber.Acosh(Parse("1E40"), 50), "Acosh(1e40) changed");
	}
}
