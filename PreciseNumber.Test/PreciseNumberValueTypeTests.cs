// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Globalization;

[TestClass]
public class PreciseNumberValueTypeTests
{
	private const int Iterations = 1000;

	[TestMethod]
	public void PreciseNumberIsAValueType()
	{
		Assert.IsTrue(typeof(PreciseNumber).IsValueType, "PreciseNumber should be a value type");
	}

	[TestMethod]
	public void DefaultEqualsZero()
	{
		PreciseNumber value = default;

		Assert.AreEqual(PreciseNumber.Zero, value);
		Assert.AreEqual(PreciseNumber.Zero.GetHashCode(), value.GetHashCode());
		Assert.AreEqual(PreciseNumber.Zero.Exponent, value.Exponent);
		Assert.AreEqual(PreciseNumber.Zero.Significand, value.Significand);
		Assert.AreEqual(PreciseNumber.Zero.SignificantDigits, value.SignificantDigits);
	}

	[TestMethod]
	public void DefaultBehavesAsZero()
	{
		PreciseNumber value = default;

		Assert.IsTrue(PreciseNumber.IsZero(value), "default should be zero");
		Assert.AreEqual("0", value.ToString());
		Assert.AreEqual(PreciseNumber.One, value + PreciseNumber.One);
		Assert.AreEqual(PreciseNumber.Zero, value * PreciseNumber.One);
		Assert.AreEqual(0, value.CompareTo(PreciseNumber.Zero));
	}

	[TestMethod]
	public void CompareToNullObjectIsGreater()
	{
		Assert.IsGreaterThan(0, PreciseNumber.One.CompareTo(null), "Any value should sort after null");
	}

	[TestMethod]
	public void SmallValueAdditionDoesNotAllocate()
	{
		PreciseNumber left = PreciseNumber.Parse("12.5", CultureInfo.InvariantCulture);
		PreciseNumber right = PreciseNumber.Parse("3.25", CultureInfo.InvariantCulture);

		long allocated = MeasureAllocations(() =>
		{
			PreciseNumber sum = PreciseNumber.Zero;
			for (int i = 0; i < Iterations; i++)
			{
				sum = left + right;
			}

			return sum;
		});

		Assert.AreEqual(0L, allocated);
	}

	[TestMethod]
	public void SmallValueSubtractionDoesNotAllocate()
	{
		PreciseNumber left = PreciseNumber.Parse("12.5", CultureInfo.InvariantCulture);
		PreciseNumber right = PreciseNumber.Parse("3.25", CultureInfo.InvariantCulture);

		long allocated = MeasureAllocations(() =>
		{
			PreciseNumber difference = PreciseNumber.Zero;
			for (int i = 0; i < Iterations; i++)
			{
				difference = left - right;
			}

			return difference;
		});

		Assert.AreEqual(0L, allocated);
	}

	[TestMethod]
	public void SmallValueMultiplicationDoesNotAllocate()
	{
		PreciseNumber left = PreciseNumber.Parse("12.5", CultureInfo.InvariantCulture);
		PreciseNumber right = PreciseNumber.Parse("3.25", CultureInfo.InvariantCulture);

		long allocated = MeasureAllocations(() =>
		{
			PreciseNumber product = PreciseNumber.Zero;
			for (int i = 0; i < Iterations; i++)
			{
				product = left * right;
			}

			return product;
		});

		Assert.AreEqual(0L, allocated);
	}

	[TestMethod]
	public void SmallValueComparisonDoesNotAllocate()
	{
		PreciseNumber left = PreciseNumber.Parse("12.5", CultureInfo.InvariantCulture);
		PreciseNumber right = PreciseNumber.Parse("12.25", CultureInfo.InvariantCulture);

		long allocated = MeasureAllocations(() =>
		{
			int greater = 0;
			for (int i = 0; i < Iterations; i++)
			{
				if (left > right && left != right && left.CompareTo(right) > 0)
				{
					greater++;
				}
			}

			return greater;
		});

		Assert.AreEqual(0L, allocated);
	}

	/// <summary>
	/// Runs an operation once to warm it up, so that JIT compilation and one-off static
	/// initialization are not counted, then reports what a second run allocates.
	/// </summary>
	private static long MeasureAllocations<T>(Func<T> operation)
	{
		_ = operation();

		long before = GC.GetAllocatedBytesForCurrentThread();
		_ = operation();
		return GC.GetAllocatedBytesForCurrentThread() - before;
	}
}
