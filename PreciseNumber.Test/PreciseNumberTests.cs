// Ignore Spelling: Commonized

namespace ktsu.PreciseNumber.Test;

using System.Globalization;
using System.Numerics;

[TestClass]
public class PreciseNumberTests
{
	[TestMethod]
	public void TestZeroCheck()
	{
		var zero = 0.ToPreciseNumber();
		Assert.IsTrue(PreciseNumber.IsZero(zero));
	}

	[TestMethod]
	public void TestOneCheck()
	{
		var one = 1.ToPreciseNumber();
		Assert.AreEqual(PreciseNumber.One, one);
	}

	[TestMethod]
	public void TestNegativeOneCheck()
	{
		var negativeOne = (-1).ToPreciseNumber();
		Assert.AreEqual(PreciseNumber.NegativeOne, negativeOne);
	}

	[TestMethod]
	public void TestAbs()
	{
		var negative = PreciseNumber.CreateFromComponents(2, new BigInteger(-12345));
		var positive = PreciseNumber.Abs(negative);
		Assert.AreEqual(PreciseNumber.CreateFromComponents(2, new BigInteger(12345)), positive);
	}

	[TestMethod]
	public void TestMax()
	{
		var left = PreciseNumber.CreateFromComponents(2, new BigInteger(200));
		var right = PreciseNumber.CreateFromComponents(1, new BigInteger(50));
		var result = PreciseNumber.Max(left, right);
		Assert.AreEqual(left, result);
	}

	[TestMethod]
	public void TestMin()
	{
		var left = PreciseNumber.CreateFromComponents(2, new BigInteger(200));
		var right = PreciseNumber.CreateFromComponents(1, new BigInteger(50));
		var result = PreciseNumber.Min(left, right);
		Assert.AreEqual(right, result);
	}

	[TestMethod]
	public void TestRound()
	{
		var value = 123.456.ToPreciseNumber();
		var result = value.Round(2);
		var expected = 123.46.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestClamp()
	{
		var value = PreciseNumber.CreateFromComponents(0, new BigInteger(5));
		var min = PreciseNumber.CreateFromComponents(0, new BigInteger(3));
		var max = PreciseNumber.CreateFromComponents(0, new BigInteger(7));
		var result = PreciseNumber.Clamp(value, min, max);
		Assert.AreEqual(value, result);
	}

	[TestMethod]
	public void TestClampLower()
	{
		var value = PreciseNumber.CreateFromComponents(0, new BigInteger(2));
		var min = PreciseNumber.CreateFromComponents(0, new BigInteger(3));
		var max = PreciseNumber.CreateFromComponents(0, new BigInteger(7));
		var result = PreciseNumber.Clamp(value, min, max);
		Assert.AreEqual(min, result);
	}

	[TestMethod]
	public void TestClampUpper()
	{
		var value = PreciseNumber.CreateFromComponents(0, new BigInteger(8));
		var min = PreciseNumber.CreateFromComponents(0, new BigInteger(3));
		var max = PreciseNumber.CreateFromComponents(0, new BigInteger(7));
		var result = PreciseNumber.Clamp(value, min, max);
		Assert.AreEqual(max, result);
	}

	[TestMethod]
	public void TestSquared()
	{
		var value = PreciseNumber.CreateFromComponents(0, new BigInteger(3));
		var result = value.Squared();
		var expected = PreciseNumber.CreateFromComponents(0, new BigInteger(9));
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestCubed()
	{
		var value = PreciseNumber.CreateFromComponents(0, new BigInteger(2));
		var result = value.Cubed();
		var expected = PreciseNumber.CreateFromComponents(0, new BigInteger(8));
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestNegate()
	{
		var value = PreciseNumber.CreateFromComponents(0, new BigInteger(10));
		var result = -value;
		var expected = PreciseNumber.CreateFromComponents(0, new BigInteger(-10));
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestSignificantDigits()
	{
		var value = PreciseNumber.CreateFromComponents(0, new BigInteger(12345));
		Assert.AreEqual(5, value.SignificantDigits);
	}

	[TestMethod]
	public void TestCountDecimalDigits()
	{
		var value = PreciseNumber.CreateFromComponents(-3, new BigInteger(123));
		Assert.AreEqual(3, value.CountDecimalDigits());
	}

	[TestMethod]
	public void TestAddWithCommonizedExponent()
	{
		var left = 1000.ToPreciseNumber();
		var right = 5.ToPreciseNumber();
		var result = left + right;
		var expected = 1005.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestSubtractWithCommonizedExponent()
	{
		var left = 1000.ToPreciseNumber();
		var right = 5.ToPreciseNumber();
		var result = left - right;
		var expected = 995.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestMultiplyWithCommonizedExponent()
	{
		var left = 2000.ToPreciseNumber();
		var right = 30.ToPreciseNumber();
		var result = left * right;
		var expected = 60000.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestDivideWithCommonizedExponent()
	{
		var left = 20000.ToPreciseNumber();
		var right = 40.ToPreciseNumber();
		var result = left / right;
		var expected = 500.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestZero()
	{
		var zero = PreciseNumber.Zero;
		Assert.AreEqual(0, zero.Significand);
		Assert.AreEqual(0, zero.Exponent);
	}

	[TestMethod]
	public void TestOne()
	{
		var one = PreciseNumber.One;
		Assert.AreEqual(1, one.Significand);
		Assert.AreEqual(0, one.Exponent);
	}

	[TestMethod]
	public void TestNegativeOne()
	{
		var negativeOne = PreciseNumber.NegativeOne;
		Assert.AreEqual(-1, negativeOne.Significand);
		Assert.AreEqual(0, negativeOne.Exponent);
	}

	[TestMethod]
	public void TestAdd()
	{
		var one = PreciseNumber.One;
		var result = one + one;
		Assert.AreEqual(2.ToPreciseNumber(), result);
	}

	[TestMethod]
	public void TestSubtract()
	{
		var one = PreciseNumber.One;
		var result = one - one;
		Assert.AreEqual(PreciseNumber.Zero, result);
	}

	[TestMethod]
	public void TestMultiply()
	{
		var one = PreciseNumber.One;
		var result = one * one;
		Assert.AreEqual(PreciseNumber.One, result);
	}

	[TestMethod]
	public void TestDivide()
	{
		var one = PreciseNumber.One;
		var result = one / one;
		Assert.AreEqual(PreciseNumber.One, result);
	}

	[TestMethod]
	public void TestToString()
	{
		var number = 0.0123.ToPreciseNumber();
		string str = number.ToString();
		Assert.AreEqual("0.0123", str);
	}

	[TestMethod]
	public void TestEquals()
	{
		var one = PreciseNumber.One;
		Assert.IsTrue(one.Equals(PreciseNumber.One));
		Assert.IsFalse(one.Equals(PreciseNumber.Zero));
	}

	[TestMethod]
	public void TestCompareTo()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		Assert.IsTrue(one.CompareTo(zero) > 0);
		Assert.IsTrue(zero.CompareTo(one) < 0);
		Assert.AreEqual(0, one.CompareTo(PreciseNumber.One));
		Assert.IsTrue(one.CompareTo(null) > 0);
	}

	// Tests for comparison operators
	[TestMethod]
	public void TestGreaterThan()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		Assert.IsTrue(one > zero);
		Assert.IsFalse(zero > one);
	}

	[TestMethod]
	public void TestGreaterThanOrEqual()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		Assert.IsTrue(one >= zero);
		Assert.IsTrue(one >= PreciseNumber.One);
		Assert.IsFalse(zero >= one);
	}

	[TestMethod]
	public void TestLessThan()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		Assert.IsTrue(zero < one);
		Assert.IsFalse(one < zero);
	}

	[TestMethod]
	public void TestLessThanOrEqual()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		Assert.IsTrue(zero <= one);
		Assert.IsTrue(one <= PreciseNumber.One);
		Assert.IsFalse(one <= zero);
	}

	[TestMethod]
	public void TestEquality()
	{
		var one = PreciseNumber.One;
		var anotherOne = 1.ToPreciseNumber();
		var zero = PreciseNumber.Zero;
		Assert.AreEqual(anotherOne, one);
		Assert.AreNotEqual(zero, one);
	}

	[TestMethod]
	public void TestInequality()
	{
		var one = PreciseNumber.One;
		var anotherOne = 1.ToPreciseNumber();
		var zero = PreciseNumber.Zero;
		Assert.AreNotEqual(zero, one);
		Assert.AreEqual(anotherOne, one);
	}

	[TestMethod]
	public void TestModulus()
	{
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.One % PreciseNumber.One);
		Assert.AreEqual(PreciseNumber.Zero, 5.ToPreciseNumber() % 5.ToPreciseNumber());
		Assert.AreEqual(PreciseNumber.Zero, 6.ToPreciseNumber() % 2.ToPreciseNumber());
		Assert.AreEqual(PreciseNumber.One, 7.ToPreciseNumber() % 2.ToPreciseNumber());
	}

	[TestMethod]
	public void TestDecrement()
	{
		var accumulator = 6.ToPreciseNumber();
		for (int i = 0; i < 3; ++i)
		{
			--accumulator;
		}

		Assert.AreEqual(3.ToPreciseNumber(), accumulator);
	}

	[TestMethod]
	public void TestIncrement()
	{
		var accumulator = PreciseNumber.Zero;
		for (int i = 0; i < 3; ++i)
		{
			++accumulator;
		}

		Assert.AreEqual(3.ToPreciseNumber(), accumulator);
	}

	// Test for unary + operator
	[TestMethod]
	public void TestUnaryPlus()
	{
		var one = PreciseNumber.One;
		var result = +one;
		Assert.AreEqual(PreciseNumber.One, result);
	}

	// Test for static Max method
	[TestMethod]
	public void TestStaticMax()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		var result = PreciseNumber.Max(one, zero);
		Assert.AreEqual(one, result);
	}

	// Test for static Min method
	[TestMethod]
	public void TestStaticMin()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		var result = PreciseNumber.Min(one, zero);
		Assert.AreEqual(zero, result);
	}

	// Test for static Clamp method
	[TestMethod]
	public void TestStaticClamp()
	{
		var value = 5.ToPreciseNumber();
		var min = 3.ToPreciseNumber();
		var max = 7.ToPreciseNumber();

		var result = PreciseNumber.Clamp(value, min, max);
		Assert.AreEqual(value, result);

		result = PreciseNumber.Clamp(2.ToPreciseNumber(), min, max);
		Assert.AreEqual(min, result);

		result = PreciseNumber.Clamp(8.ToPreciseNumber(), min, max);
		Assert.AreEqual(max, result);
	}

	// Test for static Round method
	[TestMethod]
	public void TestStaticRound()
	{
		var number = 1.2345.ToPreciseNumber();
		var result = PreciseNumber.Round(number, 2);
		Assert.AreEqual(1.24.ToPreciseNumber(), result);
	}

	[TestMethod]
	public void TestTryConvertFromChecked()
	{
		var one = PreciseNumber.One;
		Assert.ThrowsException<NotSupportedException>(() => PreciseNumber.TryConvertFromChecked(one, out var result));
	}

	[TestMethod]
	public void TestTryConvertFromSaturating()
	{
		var one = PreciseNumber.One;
		Assert.ThrowsException<NotSupportedException>(() => PreciseNumber.TryConvertFromSaturating(one, out var result));
	}

	[TestMethod]
	public void TestTryConvertFromTruncating()
	{
		var one = PreciseNumber.One;
		Assert.ThrowsException<NotSupportedException>(() => PreciseNumber.TryConvertFromTruncating(one, out var result));
	}

	[TestMethod]
	public void TestTryConvertToChecked()
	{
		var one = PreciseNumber.One;
		Assert.ThrowsException<NotSupportedException>(() => PreciseNumber.TryConvertToChecked(one, out PreciseNumber result));
	}

	[TestMethod]
	public void TestTryConvertToSaturating()
	{
		var one = PreciseNumber.One;
		Assert.ThrowsException<NotSupportedException>(() => PreciseNumber.TryConvertToSaturating(one, out PreciseNumber result));
	}

	[TestMethod]
	public void TestTryConvertToTruncating()
	{
		var one = PreciseNumber.One;
		Assert.ThrowsException<NotSupportedException>(() => PreciseNumber.TryConvertToTruncating(one, out PreciseNumber result));
	}

	[TestMethod]
	public void TestIsCanonical()
	{
		var one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsCanonical(one));
	}

	[TestMethod]
	public void TestIsComplexNumber()
	{
		var one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsComplexNumber(one));
	}

	[TestMethod]
	public void TestIsEvenInteger()
	{
		var two = 2.ToPreciseNumber();
		Assert.IsTrue(PreciseNumber.IsEvenInteger(two));

		var one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsEvenInteger(one));
	}

	[TestMethod]
	public void TestIsFinite()
	{
		var one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsFinite(one));
	}

	[TestMethod]
	public void TestIsImaginaryNumber()
	{
		var one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsImaginaryNumber(one));
	}

	[TestMethod]
	public void TestIsInfinity()
	{
		var one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsInfinity(one));
	}

	[TestMethod]
	public void TestIsInteger()
	{
		var one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsInteger(one));
	}

	[TestMethod]
	public void TestIsNaN()
	{
		var one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsNaN(one));
	}

	[TestMethod]
	public void TestIsNegative()
	{
		var negativeOne = PreciseNumber.NegativeOne;
		Assert.IsTrue(PreciseNumber.IsNegative(negativeOne));

		var one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsNegative(one));
	}

	[TestMethod]
	public void TestIsNegativeInfinity()
	{
		var negativeOne = PreciseNumber.NegativeOne;
		Assert.IsFalse(PreciseNumber.IsNegativeInfinity(negativeOne));
	}

	[TestMethod]
	public void TestIsNormal()
	{
		var one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsNormal(one));
	}

	[TestMethod]
	public void TestIsOddInteger()
	{
		var one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsOddInteger(one));

		var two = 2.ToPreciseNumber();
		Assert.IsFalse(PreciseNumber.IsOddInteger(two));
	}

	[TestMethod]
	public void TestIsPositive()
	{
		var one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsPositive(one));

		var negativeOne = PreciseNumber.NegativeOne;
		Assert.IsFalse(PreciseNumber.IsPositive(negativeOne));
	}

	[TestMethod]
	public void TestIsPositiveInfinity()
	{
		var one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsPositiveInfinity(one));
	}

	[TestMethod]
	public void TestIsRealNumber()
	{
		var one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsRealNumber(one));
	}

	[TestMethod]
	public void TestIsSubnormal()
	{
		var one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsSubnormal(one));
	}

	[TestMethod]
	public void TestIsZero()
	{
		var zero = PreciseNumber.Zero;
		Assert.IsTrue(PreciseNumber.IsZero(zero));

		var one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsZero(one));
	}

	[TestMethod]
	public void TestStaticMaxMagnitude()
	{
		var one = PreciseNumber.One;
		var negativeOne = PreciseNumber.NegativeOne;
		var result = PreciseNumber.MaxMagnitude(one, negativeOne);
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestStaticMaxMagnitudeNumber()
	{
		var one = PreciseNumber.One;
		var negativeOne = PreciseNumber.NegativeOne;
		var result = PreciseNumber.MaxMagnitudeNumber(one, negativeOne);
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestStaticMinMagnitude()
	{
		var one = PreciseNumber.One;
		var negativeOne = PreciseNumber.NegativeOne;
		var result = PreciseNumber.MinMagnitude(one, negativeOne);
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestStaticMinMagnitudeNumber()
	{
		var one = PreciseNumber.One;
		var negativeOne = PreciseNumber.NegativeOne;
		var result = PreciseNumber.MinMagnitudeNumber(one, negativeOne);
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestCompareToObject()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		object oneObject = PreciseNumber.One;
		object zeroObject = PreciseNumber.Zero;
		object intObject = 1;
		Assert.AreEqual(0, one.CompareTo(oneObject));
		Assert.IsTrue(one.CompareTo(zeroObject) > 0);
		Assert.IsTrue(zero.CompareTo(oneObject) < 0);
		Assert.ThrowsException<NotSupportedException>(() => one.CompareTo(intObject));
	}

	[TestMethod]
	public void TestCompareToPreciseNumber()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		var anotherOne = PreciseNumber.One;

		Assert.IsTrue(one.CompareTo(zero) > 0);
		Assert.IsTrue(zero.CompareTo(one) < 0);
		Assert.AreEqual(0, one.CompareTo(anotherOne));
	}

	[TestMethod]
	public void TestCompareToINumber()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		var anotherOne = PreciseNumber.One;

		Assert.IsTrue(one.CompareTo<PreciseNumber>(zero) > 0);
		Assert.IsTrue(zero.CompareTo<PreciseNumber>(one) < 0);
		Assert.AreEqual(0, one.CompareTo<PreciseNumber>(anotherOne));

		Assert.IsTrue(one.CompareTo(0) > 0);
		Assert.IsTrue(zero.CompareTo(1) < 0);
		Assert.AreEqual(0, one.CompareTo(1));

		Assert.IsTrue(one.CompareTo(0.0) > 0);
		Assert.IsTrue(zero.CompareTo(1.0) < 0);
		Assert.AreEqual(0, one.CompareTo(1.0));
	}

	[TestMethod]
	public void TestConstructorPositiveNumber()
	{
		var number = PreciseNumber.CreateFromComponents(2, 123);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(2, number.Exponent);
		Assert.AreEqual(3, number.SignificantDigits);
	}

	[TestMethod]
	public void TestConstructorNegativeNumber()
	{
		var number = PreciseNumber.CreateFromComponents(2, -123);
		Assert.AreEqual(-123, number.Significand);
		Assert.AreEqual(2, number.Exponent);
		Assert.AreEqual(3, number.SignificantDigits);
	}

	[TestMethod]
	public void TestConstructorZero()
	{
		var number = PreciseNumber.CreateFromComponents(2, 0);
		Assert.AreEqual(0, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(0, number.SignificantDigits);
	}

	[TestMethod]
	public void TestConstructorSanitizeTrue()
	{
		var number = PreciseNumber.CreateFromComponents(2, 12300, true);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(4, number.Exponent);
		Assert.AreEqual(3, number.SignificantDigits);
	}

	[TestMethod]
	public void TestConstructorSanitizeFalse()
	{
		var number = PreciseNumber.CreateFromComponents(2, 12300, false);
		Assert.AreEqual(12300, number.Significand);
		Assert.AreEqual(2, number.Exponent);
		Assert.AreEqual(5, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointPositiveNumber()
	{
		var number = PreciseNumber.CreateFromFloatingPoint(123000.45);
		Assert.AreEqual(12300045, number.Significand);
		Assert.AreEqual(-2, number.Exponent);
		Assert.AreEqual(8, number.SignificantDigits);

		var input = (Half)1000.0;
		number = PreciseNumber.CreateFromFloatingPoint(input);
		Assert.AreEqual(1, number.Significand);
		Assert.AreEqual(3, number.Exponent);
		Assert.AreEqual(1, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointNegativeNumber()
	{
		var number = PreciseNumber.CreateFromFloatingPoint(-123000.45);
		Assert.AreEqual(-12300045, number.Significand);
		Assert.AreEqual(-2, number.Exponent);
		Assert.AreEqual(8, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointOne()
	{
		var number = PreciseNumber.CreateFromFloatingPoint(1.0);
		Assert.AreEqual(1, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(1, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointNegativeOne()
	{
		var number = PreciseNumber.CreateFromFloatingPoint(-1.0);
		Assert.AreEqual(-1, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(1, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointZero()
	{
		var number = PreciseNumber.CreateFromFloatingPoint(0000.0);
		Assert.AreEqual(0, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(0, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromIntegerPositiveNumber()
	{
		var number = PreciseNumber.CreateFromInteger(123000);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(3, number.Exponent);
		Assert.AreEqual(3, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromIntegerNegativeNumber()
	{
		var number = PreciseNumber.CreateFromInteger(-123000);
		Assert.AreEqual(-123, number.Significand);
		Assert.AreEqual(3, number.Exponent);
		Assert.AreEqual(3, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromIntegerOne()
	{
		var number = PreciseNumber.CreateFromInteger(1);
		Assert.AreEqual(1, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(1, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromIntegerNegativeOne()
	{
		var number = PreciseNumber.CreateFromInteger(-1);
		Assert.AreEqual(-1, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(1, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromIntegerZero()
	{
		var number = PreciseNumber.CreateFromInteger(0000);
		Assert.AreEqual(0, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(0, number.SignificantDigits);
	}

	[TestMethod]
	public void TestMaximumBigInteger()
	{
		var maxBigInt = BigInteger.Parse("79228162514264337593543950335"); // Decimal.MaxValue
		var number = PreciseNumber.CreateFromComponents(0, maxBigInt);
		Assert.AreEqual(maxBigInt, number.Significand);
	}

	[TestMethod]
	public void TestMinimumBigInteger()
	{
		var minBigInt = BigInteger.Parse("-79228162514264337593543950335"); // Decimal.MinValue
		var number = PreciseNumber.CreateFromComponents(0, minBigInt);
		Assert.AreEqual(minBigInt, number.Significand);
	}

	[TestMethod]
	public void TestNegativeExponent()
	{
		var number = PreciseNumber.CreateFromComponents(-5, 12345);
		Assert.AreEqual(12345, number.Significand);
		Assert.AreEqual(-5, number.Exponent);
	}

	[TestMethod]
	public void TestTrailingZerosBoundary()
	{
		var number = PreciseNumber.CreateFromComponents(2, 123000, true);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(5, number.Exponent);
	}

	[TestMethod]
	public void TestToStringWithFormat()
	{
		var number = PreciseNumber.CreateFromComponents(2, 12345);
		string str = number.ToString("G");
		Assert.AreEqual("1234500", str);
	}

	[TestMethod]
	public void TestToStringWithDifferentCultureThrows()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsExactly<CultureNotFoundException>(() => number.ToString(CultureInfo.GetCultureInfo("fr-FR")));
	}

	[TestMethod]
	public void TestToStringWithInvariantCulture()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		string str = number.ToString(CultureInfo.InvariantCulture);
		Assert.AreEqual("123.45", str);
	}

	[TestMethod]
	public void TestAdditionWithLargeNumbers()
	{
		var largeNum1 = PreciseNumber.CreateFromInteger(BigInteger.Parse("79228162514264337593543950335"));
		var largeNum2 = PreciseNumber.CreateFromInteger(BigInteger.Parse("79228162514264337593543950335"));
		var result = largeNum1 + largeNum2;
		Assert.AreEqual(BigInteger.Parse("15845632502852867518708790067"), result.Significand);
		Assert.AreEqual(1, result.Exponent);
	}

	[TestMethod]
	public void TestSubtractionWithLargeNumbers()
	{
		var largeNum1 = PreciseNumber.CreateFromInteger(BigInteger.Parse("79228162514264337593543950335"));
		var largeNum2 = PreciseNumber.CreateFromInteger(BigInteger.Parse("39228162514264337593543950335"));
		var result = largeNum1 - largeNum2;
		Assert.AreEqual(4, result.Significand);
		Assert.AreEqual(28, result.Exponent);
	}

	[TestMethod]
	public void TestMultiplicationWithSmallNumbers()
	{
		var smallNum1 = PreciseNumber.CreateFromFloatingPoint(0.00001);
		var smallNum2 = PreciseNumber.CreateFromFloatingPoint(0.00002);
		var result = smallNum1 * smallNum2;
		Assert.AreEqual(2, result.Significand);
		Assert.AreEqual(-10, result.Exponent);
	}

	[TestMethod]
	public void TestDivisionWithSmallNumbers()
	{
		var smallNum1 = PreciseNumber.CreateFromFloatingPoint(0.00002);
		var smallNum2 = PreciseNumber.CreateFromFloatingPoint(0.00001);
		var result = smallNum1 / smallNum2;
		Assert.AreEqual(2, result.Significand);
		Assert.AreEqual(0, result.Exponent);
	}

	[TestMethod]
	public void TestRadix()
	{
		Assert.AreEqual(2, PreciseNumber.Radix);
	}

	[TestMethod]
	public void TestAdditiveIdentity()
	{
		var additiveIdentity = PreciseNumber.AdditiveIdentity;
		Assert.AreEqual(PreciseNumber.Zero, additiveIdentity);
	}

	[TestMethod]
	public void TestMultiplicativeIdentity()
	{
		var multiplicativeIdentity = PreciseNumber.MultiplicativeIdentity;
		Assert.AreEqual(PreciseNumber.One, multiplicativeIdentity);
	}

	[TestMethod]
	public void TestCreateRepeatingDigits()
	{
		var result = PreciseNumber.CreateRepeatingDigits(5, 3);
		Assert.AreEqual(new BigInteger(555), result);

		result = PreciseNumber.CreateRepeatingDigits(7, 0);
		Assert.AreEqual(new BigInteger(0), result);
	}

	[TestMethod]
	public void TestHasInfinitePrecision()
	{
		var number = PreciseNumber.One;
		Assert.IsTrue(number.HasInfinitePrecision);

		number = PreciseNumber.CreateFromComponents(0, new BigInteger(2));
		Assert.IsFalse(number.HasInfinitePrecision);
	}

	[TestMethod]
	public void TestLowestDecimalDigits()
	{
		var number1 = PreciseNumber.CreateFromComponents(-2, 12345);
		var number2 = PreciseNumber.CreateFromComponents(-3, 678);
		int result = PreciseNumber.LowestDecimalDigits(number1, number2);
		Assert.AreEqual(2, result);
	}

	[TestMethod]
	public void TestLowestSignificantDigits()
	{
		var number1 = PreciseNumber.CreateFromComponents(0, 12345);
		var number2 = PreciseNumber.CreateFromComponents(0, 678);
		int result = PreciseNumber.LowestSignificantDigits(number1, number2);
		Assert.AreEqual(3, result);
	}

	[TestMethod]
	public void TestReduceSignificance()
	{
		var number = PreciseNumber.CreateFromComponents(0, 12345);
		var result = number.ReduceSignificance(3);
		Assert.AreEqual(124, result.Significand);
		Assert.AreEqual(2, result.Exponent);
		Assert.AreEqual(number, number.ReduceSignificance(5));
	}

	[TestMethod]
	public void TestMakeCommonizedAndGetExponent()
	{
		var number1 = PreciseNumber.CreateFromComponents(1, 123);
		var number2 = PreciseNumber.CreateFromComponents(3, 456);
		var (common1, common2, result) = PreciseNumber.MakeCommonizedWithExponent(number1, number2);
		Assert.AreEqual(1, result);
		Assert.AreEqual(123, common1.Significand);
		Assert.AreEqual(45600, common2.Significand);
	}

	[TestMethod]
	public void TestAbsStatic()
	{
		var negative = PreciseNumber.NegativeOne;
		var result = PreciseNumber.Abs(negative);
		Assert.AreEqual(PreciseNumber.One, result);
	}

	[TestMethod]
	public void TestOperatorNegate()
	{
		var number = PreciseNumber.One;
		var result = -number;
		Assert.AreEqual(PreciseNumber.NegativeOne, result);
	}

	[TestMethod]
	public void TestOperatorAdd()
	{
		var number1 = PreciseNumber.CreateFromComponents(-2, 12345);
		var number2 = PreciseNumber.CreateFromComponents(-3, 678);
		var result = number1 + number2;
		Assert.AreEqual(124128, result.Significand);
		Assert.AreEqual(-3, result.Exponent);
	}

	[TestMethod]
	public void TestOperatorSubtract()
	{
		var number1 = PreciseNumber.CreateFromComponents(-2, 12345);
		var number2 = PreciseNumber.CreateFromComponents(-3, 678);
		var result = number1 - number2;
		Assert.AreEqual(122772, result.Significand);
		Assert.AreEqual(-3, result.Exponent);
	}

	[TestMethod]
	public void TestOperatorMultiply()
	{
		var number1 = PreciseNumber.CreateFromComponents(-2, 12345);
		var number2 = PreciseNumber.CreateFromComponents(-3, 678);
		var result = number1 * number2;
		Assert.AreEqual(836991, result.Significand);
		Assert.AreEqual(-4, result.Exponent);
	}

	[TestMethod]
	public void TestOperatorDivide()
	{
		var number1 = PreciseNumber.CreateFromComponents(-2, 12345);
		var number2 = PreciseNumber.CreateFromComponents(-3, 678);
		var result = number1 / number2;
		Assert.AreEqual(BigInteger.Parse("18207964601769911504"), result.Significand);
		Assert.AreEqual(-17, result.Exponent);
	}

	[TestMethod]
	public void TestOperatorGreaterThan()
	{
		var number1 = PreciseNumber.CreateFromComponents(0, 12345);
		var number2 = PreciseNumber.CreateFromComponents(0, 678);
		Assert.IsTrue(number1 > number2);
	}

	[TestMethod]
	public void TestOperatorLessThan()
	{
		var number1 = PreciseNumber.CreateFromComponents(0, 123);
		var number2 = PreciseNumber.CreateFromComponents(0, 678);
		Assert.IsTrue(number1 < number2);
	}

	[TestMethod]
	public void TestOperatorGreaterThanOrEqual()
	{
		var number1 = PreciseNumber.CreateFromComponents(0, 12345);
		var number2 = PreciseNumber.CreateFromComponents(0, 12345);
		Assert.IsTrue(number1 >= number2);
	}

	[TestMethod]
	public void TestOperatorLessThanOrEqual()
	{
		var number1 = PreciseNumber.CreateFromComponents(0, 123);
		var number2 = PreciseNumber.CreateFromComponents(0, 678);
		Assert.IsTrue(number1 <= number2);
	}

	[TestMethod]
	public void TestOperatorEqual()
	{
		var number1 = PreciseNumber.CreateFromComponents(0, 12345);
		var number2 = PreciseNumber.CreateFromComponents(0, 12345);
		Assert.AreEqual(number2, number1);
	}

	[TestMethod]
	public void TestOperatorNotEqual()
	{
		var number1 = PreciseNumber.CreateFromComponents(0, 12345);
		var number2 = PreciseNumber.CreateFromComponents(0, 678);
		Assert.AreNotEqual(number2, number1);
	}

	[TestMethod]
	public void TestGetHashCode()
	{
		var number1 = PreciseNumber.CreateFromComponents(2, 12345);
		var number2 = PreciseNumber.CreateFromComponents(2, 12345);
		var number3 = PreciseNumber.CreateFromComponents(3, 12345);

		// Test if the same values produce the same hash code
		Assert.AreEqual(number1.GetHashCode(), number2.GetHashCode());

		// Test if different values produce different hash codes
		Assert.AreNotEqual(number1.GetHashCode(), number3.GetHashCode());

		// Additional edge cases
		var zero = PreciseNumber.Zero;
		var one = PreciseNumber.One;
		var negativeOne = PreciseNumber.NegativeOne;

		Assert.AreEqual(zero.GetHashCode(), PreciseNumber.Zero.GetHashCode());
		Assert.AreEqual(one.GetHashCode(), PreciseNumber.One.GetHashCode());
		Assert.AreEqual(negativeOne.GetHashCode(), PreciseNumber.NegativeOne.GetHashCode());
	}

	[TestMethod]
	public void TestEqualsObjectSameInstance()
	{
		var number = PreciseNumber.One;
		Assert.IsTrue(number.Equals((object)number));
	}

	[TestMethod]
	public void TestEqualsObjectEquivalentInstance()
	{
		var number1 = PreciseNumber.One;
		var number2 = PreciseNumber.CreateFromComponents(0, 1);
		Assert.IsTrue(number1.Equals((object)number2));
	}

	[TestMethod]
	public void TestEqualsObjectDifferentInstance()
	{
		var number1 = PreciseNumber.One;
		var number2 = PreciseNumber.Zero;
		Assert.IsFalse(number1.Equals((object)number2));
	}

	[TestMethod]
	public void TestEqualsObjectNull()
	{
		var number = PreciseNumber.One;
		Assert.IsFalse(number.Equals(null));
	}

	[TestMethod]
	public void TestEqualsObjectDifferentType()
	{
		var number = PreciseNumber.One;
		string differentType = "1";
		Assert.IsFalse(number.Equals(differentType));
	}

	[TestMethod]
	public void TestToStringWithFormatAndInvariantCulture()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		string result = number.ToString("G", CultureInfo.InvariantCulture);
		Assert.AreEqual("123.45", result);
	}

	[TestMethod]
	public void TestToStringWithFormatAndSpecificCultureThrows()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsExactly<CultureNotFoundException>(() => number.ToString("G", CultureInfo.GetCultureInfo("fr-FR")));
	}

	[TestMethod]
	public void TestToStringWithNullFormatAndInvariantCulture()
	{
		var number = PreciseNumber.CreateFromComponents(3, 12345);
		string result = number.ToString(null, CultureInfo.InvariantCulture);
		Assert.AreEqual("12345000", result);
	}

	[TestMethod]
	public void TestToStringWithNullFormatAndSpecificCultureThrows()
	{
		var number = PreciseNumber.CreateFromComponents(3, 12345);
		Assert.ThrowsExactly<CultureNotFoundException>(() => number.ToString(null, CultureInfo.GetCultureInfo("fr-FR")));
	}

	[TestMethod]
	public void TestToStringWithEmptyFormatAndInvariantCulture()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		string result = number.ToString("", CultureInfo.InvariantCulture);
		Assert.AreEqual("123.45", result);
	}

	[TestMethod]
	public void TestToStringWithEmptyFormatAndSpecificCultureThrows()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsExactly<CultureNotFoundException>(() => number.ToString("", CultureInfo.GetCultureInfo("fr-FR")));
	}

	[TestMethod]
	public void TestTryFormatSufficientBuffer()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("123.45", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatInsufficientBuffer()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Span<char> buffer = stackalloc char[4];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsFalse(result);
		Assert.AreEqual(0, charsWritten);
	}

	[TestMethod]
	public void TestTryFormatEmptyFormat()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Span<char> buffer = stackalloc char[50];
		string format = string.Empty;
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("123.45", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatInvalidFormat()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsException<FormatException>(() => number.TryFormat(stackalloc char[50], out int charsWritten, "e", CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void TestTryFormatNullFormatProvider()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), null);

		Assert.IsTrue(result);
		Assert.AreEqual("123.45", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatSpecificCultureThrows()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsException<CultureNotFoundException>(() => number.TryFormat(stackalloc char[50], out int charsWritten, "G", CultureInfo.GetCultureInfo("fr-FR")));
	}

	[TestMethod]
	public void TestTryFormatZero()
	{
		var number = PreciseNumber.Zero;
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("0", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatOne()
	{
		var number = PreciseNumber.One;
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("1", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatNegativeOne()
	{
		var number = PreciseNumber.NegativeOne;
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("-1", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatInteger()
	{
		var number = 3.ToPreciseNumber();
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("3", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatFloat()
	{
		var number = 3.0.ToPreciseNumber();
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("3", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestAddLargeNumbers()
	{
		var largeNumber1 = PreciseNumber.CreateFromComponents(100, BigInteger.Parse("79228162514264337593543950335"));
		var largeNumber2 = PreciseNumber.CreateFromComponents(100, BigInteger.Parse("79228162514264337593543950335"));
		var result = largeNumber1 + largeNumber2;
		Assert.AreEqual(BigInteger.Parse("15845632502852867518708790067"), result.Significand);
		Assert.AreEqual(101, result.Exponent);
	}

	[TestMethod]
	public void TestSubtractLargeNumbers()
	{
		var largeNumber1 = PreciseNumber.CreateFromComponents(100, BigInteger.Parse("79228162514264337593543950335"));
		var largeNumber2 = PreciseNumber.CreateFromComponents(100, BigInteger.Parse("39228162514264337593543950335"));
		var result = largeNumber1 - largeNumber2;
		Assert.AreEqual(BigInteger.Parse("4"), result.Significand);
		Assert.AreEqual(128, result.Exponent);
	}

	[TestMethod]
	public void TestMultiplyLargeNumbers()
	{
		var largeNumber1 = PreciseNumber.CreateFromComponents(50, BigInteger.Parse("79228162514264337593543950335"));
		var largeNumber2 = PreciseNumber.CreateFromComponents(50, BigInteger.Parse("2"));
		var result = largeNumber1 * largeNumber2;
		Assert.AreEqual(BigInteger.Parse("15845632502852867518708790067"), result.Significand);
		Assert.AreEqual(101, result.Exponent);
	}

	[TestMethod]
	public void TestDivideLargeNumbers()
	{
		var largeNumber1 = PreciseNumber.CreateFromComponents(100, BigInteger.Parse("79228162514264337593543950335"));
		var largeNumber2 = PreciseNumber.CreateFromComponents(1, BigInteger.Parse("2"));
		var result = largeNumber1 / largeNumber2;
		Assert.AreEqual(BigInteger.Parse("396140812571321687967719751675"), result.Significand);
		Assert.AreEqual(98, result.Exponent);
	}

	[TestMethod]
	public void TestAddZero()
	{
		var zero = PreciseNumber.Zero;
		var one = PreciseNumber.One;
		var result = zero + one;
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestSubtractZero()
	{
		var zero = PreciseNumber.Zero;
		var one = PreciseNumber.One;
		var result = one - zero;
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestMultiplyZero()
	{
		var zero = PreciseNumber.Zero;
		var one = PreciseNumber.One;
		var result = one * zero;
		Assert.AreEqual(zero, result);
	}

	[TestMethod]
	public void TestDivideZero()
	{
		var zero = PreciseNumber.Zero;
		Assert.ThrowsException<DivideByZeroException>(() => zero / zero);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointSpecialValues()
	{
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => PreciseNumber.CreateFromFloatingPoint(double.NaN));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => PreciseNumber.CreateFromFloatingPoint(double.PositiveInfinity));
		Assert.ThrowsException<ArgumentOutOfRangeException>(() => PreciseNumber.CreateFromFloatingPoint(double.NegativeInfinity));
	}

	[TestMethod]
	public void TestCreateFromIntegerBoundaryValues()
	{
		var intMax = PreciseNumber.CreateFromInteger(int.MaxValue);
		Assert.AreEqual(BigInteger.Parse(int.MaxValue.ToString()), intMax.Significand);

		var intMin = PreciseNumber.CreateFromInteger(int.MinValue);
		Assert.AreEqual(BigInteger.Parse(int.MinValue.ToString()), intMin.Significand);

		var longMax = PreciseNumber.CreateFromInteger(long.MaxValue);
		Assert.AreEqual(BigInteger.Parse(long.MaxValue.ToString()), longMax.Significand);

		var longMin = PreciseNumber.CreateFromInteger(long.MinValue);
		Assert.AreEqual(BigInteger.Parse(long.MinValue.ToString()), longMin.Significand);
	}

	[TestMethod]
	public void TestNegativeExponentHandling()
	{
		var number = PreciseNumber.CreateFromComponents(-3, 12345);
		Assert.AreEqual(12345, number.Significand);
		Assert.AreEqual(-3, number.Exponent);

		var result = number.Round(2);
		Assert.AreEqual(1235, result.Significand); // After rounding, check if the exponent and significand are adjusted correctly
		Assert.AreEqual(-2, result.Exponent);
	}

	[TestMethod]
	public void TestHandlingTrailingZeros()
	{
		var number = PreciseNumber.CreateFromComponents(2, 123000, true);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(5, number.Exponent); // Ensure trailing zeros are removed and exponent is adjusted correctly

		number = PreciseNumber.CreateFromComponents(-2, 123000, true);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(1, number.Exponent);
	}

	[TestMethod]
	public void TestToStringVariousFormats()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsException<FormatException>(() => number.ToString("E2", CultureInfo.InvariantCulture));
		Assert.ThrowsException<FormatException>(() => number.ToString("F2", CultureInfo.InvariantCulture));
		Assert.ThrowsException<FormatException>(() => number.ToString("N2", CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void TestTryFormatVariousFormats()
	{
		var number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsException<FormatException>(() => number.TryFormat(stackalloc char[50], out int charsWritten, "E2".AsSpan(), CultureInfo.InvariantCulture));
		Assert.ThrowsException<FormatException>(() => number.TryFormat(stackalloc char[50], out int charsWritten, "F2".AsSpan(), CultureInfo.InvariantCulture));
		Assert.ThrowsException<FormatException>(() => number.TryFormat(stackalloc char[50], out int charsWritten, "N2".AsSpan(), CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void ToDouble()
	{
		var preciseNumber = PreciseNumber.CreateFromComponents(3, 12345); // 12345e3
		double result = preciseNumber.To<double>();
		Assert.AreEqual(12345e3, result);
	}

	[TestMethod]
	public void ToFloat()
	{
		var preciseNumber = PreciseNumber.CreateFromComponents(2, 12345); // 12345e2
		float result = preciseNumber.To<float>();
		Assert.AreEqual(12345e2f, result);
	}

	[TestMethod]
	public void ToDecimal()
	{
		var preciseNumber = PreciseNumber.CreateFromComponents(1, 12345); // 12345e1
		decimal result = preciseNumber.To<decimal>();
		Assert.AreEqual(12345e1m, result);
	}

	[TestMethod]
	public void ToInt()
	{
		var preciseNumber = PreciseNumber.CreateFromComponents(0, 12345); // 12345e0
		int result = preciseNumber.To<int>();
		Assert.AreEqual(12345, result);
	}

	[TestMethod]
	public void ToLong()
	{
		var preciseNumber = PreciseNumber.CreateFromComponents(0, 123456789012345); // 123456789012345e0
		long result = preciseNumber.To<long>();
		Assert.AreEqual(123456789012345L, result);
	}

	[TestMethod]
	public void ToBigInteger()
	{
		var preciseNumber = PreciseNumber.CreateFromComponents(5, 12345); // 12345e5
		var result = preciseNumber.To<BigInteger>();
		Assert.AreEqual(BigInteger.Parse("1234500000"), result);
	}

	[TestMethod]
	public void ToOverflow()
	{
		var preciseNumber = PreciseNumber.CreateFromComponents(1000, 12345); // This is a very large number
		Assert.ThrowsException<OverflowException>(() => preciseNumber.To<int>()); // This should throw an exception
	}

	[TestMethod]
	public void SquaredShouldReturnCorrectValue()
	{
		// Arrange
		var number = 3.ToPreciseNumber();
		var expected = 9.ToPreciseNumber();

		// Act
		var result = number.Squared();

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void CubedShouldReturnCorrectValue()
	{
		// Arrange
		var number = 3.ToPreciseNumber();
		var expected = 27.ToPreciseNumber();

		// Act
		var result = number.Cubed();

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void PowShouldReturnCorrectValue()
	{
		// Arrange
		var number = 2.ToPreciseNumber();
		var expected = 8.ToPreciseNumber();

		// Act
		var result = number.Pow(3.ToPreciseNumber());

		// Assert
		Assert.AreEqual(expected, result);

		Assert.AreEqual(PreciseNumber.One, PreciseNumber.One.Pow(10.ToPreciseNumber()));
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Zero.Pow(10.ToPreciseNumber()));

		result = number.Pow(2.5.ToPreciseNumber());
		expected = 5.656854249492381.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void PowZeroPowerShouldReturnOne()
	{
		// Arrange
		var number = 5.ToPreciseNumber();
		var expected = PreciseNumber.One;

		// Act
		var result = number.Pow(0.ToPreciseNumber());

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void PowNegativePowerShouldReturnCorrectValue()
	{
		// Arrange
		var number = 2.ToPreciseNumber();
		var expected = 0.125.ToPreciseNumber();

		// Act
		var result = number.Pow(-3.ToPreciseNumber());

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestExpWithZeroPower()
	{
		var result = PreciseNumber.Exp(0.ToPreciseNumber());
		var expected = PreciseNumber.One; // e^0 = 1
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestExpWithPositivePower()
	{
		var result = PreciseNumber.Exp(1.ToPreciseNumber());
		var expected = PreciseNumber.E; // e^1 = e
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestExpWithNegativePower()
	{
		var result = PreciseNumber.Exp(-1.ToPreciseNumber());
		var expected = PreciseNumber.One / PreciseNumber.E; // e^-1 = 1/e
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestExpWithLargePositivePower()
	{
		var result = PreciseNumber.Exp(5.ToPreciseNumber());
		var expected = 148.4131591025766m.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestExpWithLargeNegativePower()
	{
		var result = PreciseNumber.Exp(-5.ToPreciseNumber());
		var expected = 0.006737946999085467m.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestRoundWithSamePrecision()
	{
		var number = 1234.5.ToPreciseNumber();
		var result = number.Round(1);
		Assert.AreEqual(number, result);
	}

	[TestMethod]
	public void TestDivideByZero()
	{
		var number = 1234.5.ToPreciseNumber();
		Assert.ThrowsException<DivideByZeroException>(() => number / PreciseNumber.Zero);
	}

	[TestMethod]
	public void TestModZero()
	{
		var number = 1234.5.ToPreciseNumber();
		Assert.ThrowsException<DivideByZeroException>(() => number % PreciseNumber.Zero);
	}

	[TestMethod]
	public void TestDivideBySelf()
	{
		var number = 1234.5.ToPreciseNumber();
		var result = number / number;
		Assert.AreEqual(PreciseNumber.One, result);
	}

	[TestMethod]
	public void TestEasyMultiplies()
	{
		var two = 2.ToPreciseNumber();
		Assert.AreEqual(two, PreciseNumber.One * two);
		Assert.AreEqual(two, two * PreciseNumber.One);
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Zero * PreciseNumber.One);
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.One * PreciseNumber.Zero);
	}

	[TestMethod]
	public void TestParseWithValidInput()
	{
		var input = "1.23E4".AsSpan();
		var expected = 1.23e4.ToPreciseNumber();
		var result = PreciseNumber.Parse(input, NumberStyles.Any, null);
		Assert.AreEqual(expected, result);

		result = PreciseNumber.Parse(input, null);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestParseWithZero()
	{
		var input = "0".AsSpan();
		var result = PreciseNumber.Parse(input, NumberStyles.Any, null);
		Assert.AreEqual(PreciseNumber.Zero, result);
	}

	[TestMethod]
	public void TestParseWithNegativeInput()
	{
		var input = "-5.67E-2".AsSpan();
		var expected = -5.67e-2.ToPreciseNumber();
		var result = PreciseNumber.Parse(input, NumberStyles.Any, null);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestParseWithInvalidInput()
	{
		Assert.ThrowsException<FormatException>(() => PreciseNumber.Parse("1.2.3E4".AsSpan(), NumberStyles.Any, null));
		Assert.ThrowsException<FormatException>(() => PreciseNumber.Parse("invalid".AsSpan(), NumberStyles.Any, null));
		Assert.ThrowsException<FormatException>(() => PreciseNumber.Parse(string.Empty.AsSpan(), NumberStyles.Any, null));
	}

	[TestMethod]
	public void TestParseStringWithValidInput()
	{
		string input = "1.23E4";
		var expected = 1.23e4.ToPreciseNumber();
		var result = PreciseNumber.Parse(input, NumberStyles.Any, null);
		Assert.AreEqual(expected, result);

		result = PreciseNumber.Parse(input, null);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestParseStringWithNegativeInput()
	{
		string input = "-5.67E-2";
		var expected = -5.67e-2.ToPreciseNumber();
		var result = PreciseNumber.Parse(input, NumberStyles.Any, null);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestParseStringWithInvalidInput()
	{
		string input = "invalid";
		Assert.ThrowsException<FormatException>(() => PreciseNumber.Parse(input, NumberStyles.Any, null));
	}

	[TestMethod]
	public void TestTryParseWithValidInput()
	{
		var input = "1.23E4".AsSpan();
		var expected = 1.23e4.ToPreciseNumber();
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out var result);
		Assert.IsTrue(success);
		Assert.AreEqual(expected, result);

		success = PreciseNumber.TryParse(input, provider: null, out result);
		Assert.IsTrue(success);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestTryParseWithNegativeInput()
	{
		var input = "-5.67E-2".AsSpan();
		var expected = -5.67e-2.ToPreciseNumber();
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out var result);
		Assert.IsTrue(success);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestTryParseWithInvalidInput()
	{
		var input = "invalid".AsSpan();
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out var result);
		Assert.IsFalse(success);
		Assert.AreEqual(default, result);
	}

	[TestMethod]
	public void TestTryParseStringWithValidInput()
	{
		string input = "1.23E4";
		var expected = 1.23e4.ToPreciseNumber();
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out var result);
		Assert.IsTrue(success);
		Assert.AreEqual(expected, result);

		success = PreciseNumber.TryParse(input, null, out result);
		Assert.IsTrue(success);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestTryParseStringWithNegativeInput()
	{
		string input = "-5.67E-2";
		var expected = -5.67e-2.ToPreciseNumber();
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out var result);
		Assert.IsTrue(success);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestTryParseStringWithInvalidInput()
	{
		string input = "invalid";
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out var result);
		Assert.IsFalse(success);
		Assert.AreEqual(default, result);
	}

	[TestMethod]
	public void TestEValue()
	{
		var expectedSignificand = BigInteger.Parse("27182818284590452353602874713526624977572", CultureInfo.InvariantCulture);
		int expectedExponent = -40;
		var eValue = PreciseNumber.E;

		Assert.AreEqual(expectedSignificand, eValue.Significand);
		Assert.AreEqual(expectedExponent, eValue.Exponent);
	}

	[TestMethod]
	public void TestTauValue()
	{
		var expectedSignificand = BigInteger.Parse("6283185307179586476925287", CultureInfo.InvariantCulture);
		int expectedExponent = -24;
		var tauValue = PreciseNumber.Tau;

		Assert.AreEqual(expectedSignificand, tauValue.Significand);
		Assert.AreEqual(expectedExponent, tauValue.Exponent);
	}

	[TestMethod]
	public void TestPiValue()
	{
		var expectedSignificand = BigInteger.Parse("31415926535897932384626433", CultureInfo.InvariantCulture);
		int expectedExponent = -25;
		var piValue = PreciseNumber.Pi;
		Assert.AreEqual(expectedSignificand, piValue.Significand);
		Assert.AreEqual(expectedExponent, piValue.Exponent);
	}

	[TestMethod]
	public void TestNotEqual()
	{
		var number1 = PreciseNumber.CreateFromComponents(0, 12345);
		var number2 = PreciseNumber.CreateFromComponents(0, 678);
		var number3 = PreciseNumber.CreateFromComponents(0, 12345);

		Assert.IsTrue(PreciseNumber.NotEqual(number1, number2));
		Assert.IsFalse(PreciseNumber.NotEqual(number1, number3));
	}
	[TestMethod]
	public void TestCompareToINumberWithNull()
	{
		var one = PreciseNumber.One;
		Assert.AreEqual(1, one.CompareTo<PreciseNumber>(null!));
	}

	[TestMethod]
	public void TestCompareToINumberWithSmallerValue()
	{
		var one = PreciseNumber.One;
		var zero = PreciseNumber.Zero;
		Assert.IsTrue(one.CompareTo<PreciseNumber>(zero) > 0);
	}

	[TestMethod]
	public void TestCompareToINumberWithLargerValue()
	{
		var one = PreciseNumber.One;
		var two = 2.ToPreciseNumber();
		Assert.IsTrue(one.CompareTo<PreciseNumber>(two) < 0);
	}

	[TestMethod]
	public void TestCompareToINumberWithEqualValue()
	{
		var one = PreciseNumber.One;
		var anotherOne = 1.ToPreciseNumber();
		Assert.AreEqual(0, one.CompareTo<PreciseNumber>(anotherOne));
	}

	[TestMethod]
	public void TryCreate_WithIntegerInput_ReturnsTrueAndCreatesPreciseNumber()
	{
		// Arrange  
		int input = 42;

		// Act  
		bool result = PreciseNumberExtensions.TryCreate(input, out var preciseNumber);

		// Assert  
		Assert.IsTrue(result);
		Assert.IsNotNull(preciseNumber);
		Assert.AreEqual(input, preciseNumber.To<int>());
	}

	[TestMethod]
	public void TryCreate_WithFloatingPointInput_ReturnsTrueAndCreatesPreciseNumber()
	{
		// Arrange  
		double input = 42.42;

		// Act  
		bool result = PreciseNumberExtensions.TryCreate(input, out var preciseNumber);

		// Assert  
		Assert.IsTrue(result);
		Assert.IsNotNull(preciseNumber);
		Assert.AreEqual(input, preciseNumber.To<double>());
	}

	[TestMethod]
	public void TryCreate_InputIsPreciseNumber_ReturnsTrue()
	{
		// Arrange
		var input = new PreciseNumber(2, new BigInteger(12345));

		// Act
		bool result = PreciseNumberExtensions.TryCreate(input, out var preciseNumber);

		// Assert
		Assert.IsTrue(result);
		Assert.IsNotNull(preciseNumber);
		Assert.AreEqual(input, preciseNumber);
	}

	[TestMethod]
	public void TryCreate_WithPreciseNumber_ReturnsTrue()
	{
		// Arrange  
		var input = PreciseNumber.One;

		// Act  
		bool result = PreciseNumberExtensions.TryCreate(input, out var preciseNumber);

		// Assert  
		Assert.IsTrue(result);
		Assert.AreEqual(input, preciseNumber);
	}

	[TestMethod]
	public void TryCreate_WithBinaryInteger_ReturnsTrue()
	{
		// Arrange  
		int input = 42;

		// Act  
		bool result = PreciseNumberExtensions.TryCreate(input, out var preciseNumber);

		// Assert  
		Assert.IsTrue(result);
		Assert.IsNotNull(preciseNumber);
		Assert.AreEqual(PreciseNumber.CreateFromInteger(input), preciseNumber);
	}

	[TestMethod]
	public void TryCreate_WithFloatingPoint_ReturnsTrue()
	{
		// Arrange  
		double input = 3.14;

		// Act  
		bool result = PreciseNumberExtensions.TryCreate(input, out var preciseNumber);

		// Assert  
		Assert.IsTrue(result);
		Assert.IsNotNull(preciseNumber);
		Assert.AreEqual(PreciseNumber.CreateFromFloatingPoint(input), preciseNumber);
	}

	[TestMethod]
	public void As_WithSameInputAndOutputType_ReturnsInput()
	{
		// Arrange
		var input = new PreciseNumber(2, new BigInteger(123));

		// Act
		var result = input.As<PreciseNumber>();

		// Assert
		Assert.AreSame(input, result);
	}

	[TestMethod]
	public void As_WithConvertibleInputAndOutputType_ReturnsConvertedInstance()
	{
		// Arrange
		PreciseNumber input = new(2, new BigInteger(123));

		// Act
		var result = input.As<DerivedPreciseNumber>();

		// Assert
		Assert.IsNotNull(result);
		Assert.AreEqual(input.Exponent, result.Exponent);
		Assert.AreEqual(input.Significand, result.Significand);
	}

	public record DerivedPreciseNumber : PreciseNumber
	{
		public DerivedPreciseNumber(PreciseNumber original) : base(original)
		{
		}
	}
}
