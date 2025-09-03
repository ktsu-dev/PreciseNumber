// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.PreciseNumber.Test;

using System.Globalization;
using System.Numerics;

[TestClass]
public class PreciseNumberTests
{
	[TestMethod]
	public void TestZeroCheck()
	{
		PreciseNumber zero = 0.ToPreciseNumber();
		Assert.IsTrue(PreciseNumber.IsZero(zero));
	}

	[TestMethod]
	public void TestOneCheck()
	{
		PreciseNumber one = 1.ToPreciseNumber();
		Assert.AreEqual(PreciseNumber.One, one);
	}

	[TestMethod]
	public void TestNegativeOneCheck()
	{
		PreciseNumber negativeOne = (-1).ToPreciseNumber();
		Assert.AreEqual(PreciseNumber.NegativeOne, negativeOne);
	}

	[TestMethod]
	public void TestAbs()
	{
		PreciseNumber negative = PreciseNumber.CreateFromComponents(2, new BigInteger(-12345));
		PreciseNumber positive = PreciseNumber.Abs(negative);
		Assert.AreEqual(PreciseNumber.CreateFromComponents(2, new BigInteger(12345)), positive);
	}

	[TestMethod]
	public void TestMax()
	{
		PreciseNumber left = PreciseNumber.CreateFromComponents(2, new BigInteger(200));
		PreciseNumber right = PreciseNumber.CreateFromComponents(1, new BigInteger(50));
		PreciseNumber result = PreciseNumber.Max(left, right);
		Assert.AreEqual(left, result);
	}

	[TestMethod]
	public void TestMin()
	{
		PreciseNumber left = PreciseNumber.CreateFromComponents(2, new BigInteger(200));
		PreciseNumber right = PreciseNumber.CreateFromComponents(1, new BigInteger(50));
		PreciseNumber result = PreciseNumber.Min(left, right);
		Assert.AreEqual(right, result);
	}

	[TestMethod]
	public void TestRound()
	{
		PreciseNumber value = 123.456.ToPreciseNumber();
		PreciseNumber result = value.Round(2);
		PreciseNumber expected = 123.46.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestClamp()
	{
		PreciseNumber value = PreciseNumber.CreateFromComponents(0, new BigInteger(5));
		PreciseNumber min = PreciseNumber.CreateFromComponents(0, new BigInteger(3));
		PreciseNumber max = PreciseNumber.CreateFromComponents(0, new BigInteger(7));
		PreciseNumber result = PreciseNumber.Clamp(value, min, max);
		Assert.AreEqual(value, result);
	}

	[TestMethod]
	public void TestClampLower()
	{
		PreciseNumber value = PreciseNumber.CreateFromComponents(0, new BigInteger(2));
		PreciseNumber min = PreciseNumber.CreateFromComponents(0, new BigInteger(3));
		PreciseNumber max = PreciseNumber.CreateFromComponents(0, new BigInteger(7));
		PreciseNumber result = PreciseNumber.Clamp(value, min, max);
		Assert.AreEqual(min, result);
	}

	[TestMethod]
	public void TestClampUpper()
	{
		PreciseNumber value = PreciseNumber.CreateFromComponents(0, new BigInteger(8));
		PreciseNumber min = PreciseNumber.CreateFromComponents(0, new BigInteger(3));
		PreciseNumber max = PreciseNumber.CreateFromComponents(0, new BigInteger(7));
		PreciseNumber result = PreciseNumber.Clamp(value, min, max);
		Assert.AreEqual(max, result);
	}

	[TestMethod]
	public void TestSquared()
	{
		PreciseNumber value = PreciseNumber.CreateFromComponents(0, new BigInteger(3));
		PreciseNumber result = value.Squared();
		PreciseNumber expected = PreciseNumber.CreateFromComponents(0, new BigInteger(9));
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestCubed()
	{
		PreciseNumber value = PreciseNumber.CreateFromComponents(0, new BigInteger(2));
		PreciseNumber result = value.Cubed();
		PreciseNumber expected = PreciseNumber.CreateFromComponents(0, new BigInteger(8));
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestNegate()
	{
		PreciseNumber value = PreciseNumber.CreateFromComponents(0, new BigInteger(10));
		PreciseNumber result = -value;
		PreciseNumber expected = PreciseNumber.CreateFromComponents(0, new BigInteger(-10));
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestSignificantDigits()
	{
		PreciseNumber value = PreciseNumber.CreateFromComponents(0, new BigInteger(12345));
		Assert.AreEqual(5, value.SignificantDigits);
	}

	[TestMethod]
	public void TestCountDecimalDigits()
	{
		PreciseNumber value = PreciseNumber.CreateFromComponents(-3, new BigInteger(123));
		Assert.AreEqual(3, value.CountDecimalDigits());
	}

	[TestMethod]
	public void TestAddWithCommonizedExponent()
	{
		PreciseNumber left = 1000.ToPreciseNumber();
		PreciseNumber right = 5.ToPreciseNumber();
		PreciseNumber result = left + right;
		PreciseNumber expected = 1005.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestSubtractWithCommonizedExponent()
	{
		PreciseNumber left = 1000.ToPreciseNumber();
		PreciseNumber right = 5.ToPreciseNumber();
		PreciseNumber result = left - right;
		PreciseNumber expected = 995.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestMultiplyWithCommonizedExponent()
	{
		PreciseNumber left = 2000.ToPreciseNumber();
		PreciseNumber right = 30.ToPreciseNumber();
		PreciseNumber result = left * right;
		PreciseNumber expected = 60000.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestDivideWithCommonizedExponent()
	{
		PreciseNumber left = 20000.ToPreciseNumber();
		PreciseNumber right = 40.ToPreciseNumber();
		PreciseNumber result = left / right;
		PreciseNumber expected = 500.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestZero()
	{
		PreciseNumber zero = PreciseNumber.Zero;
		Assert.AreEqual(0, zero.Significand);
		Assert.AreEqual(0, zero.Exponent);
	}

	[TestMethod]
	public void TestOne()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.AreEqual(1, one.Significand);
		Assert.AreEqual(0, one.Exponent);
	}

	[TestMethod]
	public void TestNegativeOne()
	{
		PreciseNumber negativeOne = PreciseNumber.NegativeOne;
		Assert.AreEqual(-1, negativeOne.Significand);
		Assert.AreEqual(0, negativeOne.Exponent);
	}

	[TestMethod]
	public void TestAdd()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber result = one + one;
		Assert.AreEqual(2.ToPreciseNumber(), result);
	}

	[TestMethod]
	public void TestSubtract()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber result = one - one;
		Assert.AreEqual(PreciseNumber.Zero, result);
	}

	[TestMethod]
	public void TestMultiply()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber result = one * one;
		Assert.AreEqual(PreciseNumber.One, result);
	}

	[TestMethod]
	public void TestDivide()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber result = one / one;
		Assert.AreEqual(PreciseNumber.One, result);
	}

	[TestMethod]
	public void TestToString()
	{
		PreciseNumber number = 0.0123.ToPreciseNumber();
		string str = number.ToString();
		Assert.AreEqual("0.0123", str);
	}

	[TestMethod]
	public void TestEquals()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsTrue(one.Equals(PreciseNumber.One));
		Assert.IsFalse(one.Equals(PreciseNumber.Zero));
	}

	[TestMethod]
	public void TestCompareTo()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		Assert.IsTrue(one.CompareTo(zero) > 0);
		Assert.IsTrue(zero.CompareTo(one) < 0);
		Assert.AreEqual(0, one.CompareTo(PreciseNumber.One));
		Assert.IsTrue(one.CompareTo(null) > 0);
	}

	// Tests for comparison operators
	[TestMethod]
	public void TestGreaterThan()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		Assert.IsTrue(one > zero);
		Assert.IsFalse(zero > one);
	}

	[TestMethod]
	public void TestGreaterThanOrEqual()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		Assert.IsTrue(one >= zero);
		Assert.IsTrue(one >= PreciseNumber.One);
		Assert.IsFalse(zero >= one);
	}

	[TestMethod]
	public void TestLessThan()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		Assert.IsTrue(zero < one);
		Assert.IsFalse(one < zero);
	}

	[TestMethod]
	public void TestLessThanOrEqual()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		Assert.IsTrue(zero <= one);
		Assert.IsTrue(one <= PreciseNumber.One);
		Assert.IsFalse(one <= zero);
	}

	[TestMethod]
	public void TestEquality()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber anotherOne = 1.ToPreciseNumber();
		PreciseNumber zero = PreciseNumber.Zero;
		Assert.AreEqual(anotherOne, one);
		Assert.AreNotEqual(zero, one);
	}

	[TestMethod]
	public void TestInequality()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber anotherOne = 1.ToPreciseNumber();
		PreciseNumber zero = PreciseNumber.Zero;
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
		PreciseNumber accumulator = 6.ToPreciseNumber();
		for (int i = 0; i < 3; ++i)
		{
			--accumulator;
		}

		Assert.AreEqual(3.ToPreciseNumber(), accumulator);
	}

	[TestMethod]
	public void TestIncrement()
	{
		PreciseNumber accumulator = PreciseNumber.Zero;
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
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber result = +one;
		Assert.AreEqual(PreciseNumber.One, result);
	}

	// Test for static Max method
	[TestMethod]
	public void TestStaticMax()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		PreciseNumber result = PreciseNumber.Max(one, zero);
		Assert.AreEqual(one, result);
	}

	// Test for static Min method
	[TestMethod]
	public void TestStaticMin()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		PreciseNumber result = PreciseNumber.Min(one, zero);
		Assert.AreEqual(zero, result);
	}

	// Test for static Clamp method
	[TestMethod]
	public void TestStaticClamp()
	{
		PreciseNumber value = 5.ToPreciseNumber();
		PreciseNumber min = 3.ToPreciseNumber();
		PreciseNumber max = 7.ToPreciseNumber();

		PreciseNumber result = PreciseNumber.Clamp(value, min, max);
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
		PreciseNumber number = 1.2345.ToPreciseNumber();
		PreciseNumber result = PreciseNumber.Round(number, 2);
		Assert.AreEqual(1.24.ToPreciseNumber(), result);
	}

	[TestMethod]
	public void TestTryConvertFromChecked()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.ThrowsExactly<NotSupportedException>(() => PreciseNumber.TryConvertFromChecked(one, out PreciseNumber? result));
	}

	[TestMethod]
	public void TestTryConvertFromSaturating()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.ThrowsExactly<NotSupportedException>(() => PreciseNumber.TryConvertFromSaturating(one, out PreciseNumber? result));
	}

	[TestMethod]
	public void TestTryConvertFromTruncating()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.ThrowsExactly<NotSupportedException>(() => PreciseNumber.TryConvertFromTruncating(one, out PreciseNumber? result));
	}

	[TestMethod]
	public void TestTryConvertToChecked()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.ThrowsExactly<NotSupportedException>(() => PreciseNumber.TryConvertToChecked(one, out PreciseNumber result));
	}

	[TestMethod]
	public void TestTryConvertToSaturating()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.ThrowsExactly<NotSupportedException>(() => PreciseNumber.TryConvertToSaturating(one, out PreciseNumber result));
	}

	[TestMethod]
	public void TestTryConvertToTruncating()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.ThrowsExactly<NotSupportedException>(() => PreciseNumber.TryConvertToTruncating(one, out PreciseNumber result));
	}

	[TestMethod]
	public void TestIsCanonical()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsCanonical(one));
	}

	[TestMethod]
	public void TestIsComplexNumber()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsComplexNumber(one));
	}

	[TestMethod]
	public void TestIsEvenInteger()
	{
		PreciseNumber two = 2.ToPreciseNumber();
		Assert.IsTrue(PreciseNumber.IsEvenInteger(two));

		PreciseNumber one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsEvenInteger(one));
	}

	[TestMethod]
	public void TestIsFinite()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsFinite(one));
	}

	[TestMethod]
	public void TestIsImaginaryNumber()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsImaginaryNumber(one));
	}

	[TestMethod]
	public void TestIsInfinity()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsInfinity(one));
	}

	[TestMethod]
	public void TestIsInteger()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsInteger(one));
	}

	[TestMethod]
	public void TestIsNaN()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsNaN(one));
	}

	[TestMethod]
	public void TestIsNegative()
	{
		PreciseNumber negativeOne = PreciseNumber.NegativeOne;
		Assert.IsTrue(PreciseNumber.IsNegative(negativeOne));

		PreciseNumber one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsNegative(one));
	}

	[TestMethod]
	public void TestIsNegativeInfinity()
	{
		PreciseNumber negativeOne = PreciseNumber.NegativeOne;
		Assert.IsFalse(PreciseNumber.IsNegativeInfinity(negativeOne));
	}

	[TestMethod]
	public void TestIsNormal()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsNormal(one));
	}

	[TestMethod]
	public void TestIsOddInteger()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsOddInteger(one));

		PreciseNumber two = 2.ToPreciseNumber();
		Assert.IsFalse(PreciseNumber.IsOddInteger(two));
	}

	[TestMethod]
	public void TestIsPositive()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsPositive(one));

		PreciseNumber negativeOne = PreciseNumber.NegativeOne;
		Assert.IsFalse(PreciseNumber.IsPositive(negativeOne));
	}

	[TestMethod]
	public void TestIsPositiveInfinity()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsPositiveInfinity(one));
	}

	[TestMethod]
	public void TestIsRealNumber()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsTrue(PreciseNumber.IsRealNumber(one));
	}

	[TestMethod]
	public void TestIsSubnormal()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsSubnormal(one));
	}

	[TestMethod]
	public void TestIsZero()
	{
		PreciseNumber zero = PreciseNumber.Zero;
		Assert.IsTrue(PreciseNumber.IsZero(zero));

		PreciseNumber one = PreciseNumber.One;
		Assert.IsFalse(PreciseNumber.IsZero(one));
	}

	[TestMethod]
	public void TestStaticMaxMagnitude()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber negativeOne = PreciseNumber.NegativeOne;
		PreciseNumber result = PreciseNumber.MaxMagnitude(one, negativeOne);
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestStaticMaxMagnitudeNumber()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber negativeOne = PreciseNumber.NegativeOne;
		PreciseNumber result = PreciseNumber.MaxMagnitudeNumber(one, negativeOne);
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestStaticMinMagnitude()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber negativeOne = PreciseNumber.NegativeOne;
		PreciseNumber result = PreciseNumber.MinMagnitude(one, negativeOne);
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestStaticMinMagnitudeNumber()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber negativeOne = PreciseNumber.NegativeOne;
		PreciseNumber result = PreciseNumber.MinMagnitudeNumber(one, negativeOne);
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestCompareToObject()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		object oneObject = PreciseNumber.One;
		object zeroObject = PreciseNumber.Zero;
		object intObject = 1;
		Assert.AreEqual(0, one.CompareTo(oneObject));
		Assert.IsTrue(one.CompareTo(zeroObject) > 0);
		Assert.IsTrue(zero.CompareTo(oneObject) < 0);
		Assert.ThrowsExactly<NotSupportedException>(() => one.CompareTo(intObject));
	}

	[TestMethod]
	public void TestCompareToPreciseNumber()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		PreciseNumber anotherOne = PreciseNumber.One;

		Assert.IsTrue(one.CompareTo(zero) > 0);
		Assert.IsTrue(zero.CompareTo(one) < 0);
		Assert.AreEqual(0, one.CompareTo(anotherOne));
	}

	[TestMethod]
	public void TestCompareToINumber()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		PreciseNumber anotherOne = PreciseNumber.One;

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
		PreciseNumber number = PreciseNumber.CreateFromComponents(2, 123);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(2, number.Exponent);
		Assert.AreEqual(3, number.SignificantDigits);
	}

	[TestMethod]
	public void TestConstructorNegativeNumber()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(2, -123);
		Assert.AreEqual(-123, number.Significand);
		Assert.AreEqual(2, number.Exponent);
		Assert.AreEqual(3, number.SignificantDigits);
	}

	[TestMethod]
	public void TestConstructorZero()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(2, 0);
		Assert.AreEqual(0, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(0, number.SignificantDigits);
	}

	[TestMethod]
	public void TestConstructorSanitizeTrue()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(2, 12300, true);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(4, number.Exponent);
		Assert.AreEqual(3, number.SignificantDigits);
	}

	[TestMethod]
	public void TestConstructorSanitizeFalse()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(2, 12300, false);
		Assert.AreEqual(12300, number.Significand);
		Assert.AreEqual(2, number.Exponent);
		Assert.AreEqual(5, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointPositiveNumber()
	{
		PreciseNumber number = PreciseNumber.CreateFromFloatingPoint(123000.45);
		Assert.AreEqual(12300045, number.Significand);
		Assert.AreEqual(-2, number.Exponent);
		Assert.AreEqual(8, number.SignificantDigits);

		Half input = (Half)1000.0;
		number = PreciseNumber.CreateFromFloatingPoint(input);
		Assert.AreEqual(1, number.Significand);
		Assert.AreEqual(3, number.Exponent);
		Assert.AreEqual(1, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointNegativeNumber()
	{
		PreciseNumber number = PreciseNumber.CreateFromFloatingPoint(-123000.45);
		Assert.AreEqual(-12300045, number.Significand);
		Assert.AreEqual(-2, number.Exponent);
		Assert.AreEqual(8, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointOne()
	{
		PreciseNumber number = PreciseNumber.CreateFromFloatingPoint(1.0);
		Assert.AreEqual(1, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(1, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointNegativeOne()
	{
		PreciseNumber number = PreciseNumber.CreateFromFloatingPoint(-1.0);
		Assert.AreEqual(-1, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(1, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointZero()
	{
		PreciseNumber number = PreciseNumber.CreateFromFloatingPoint(0000.0);
		Assert.AreEqual(0, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(0, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromIntegerPositiveNumber()
	{
		PreciseNumber number = PreciseNumber.CreateFromInteger(123000);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(3, number.Exponent);
		Assert.AreEqual(3, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromIntegerNegativeNumber()
	{
		PreciseNumber number = PreciseNumber.CreateFromInteger(-123000);
		Assert.AreEqual(-123, number.Significand);
		Assert.AreEqual(3, number.Exponent);
		Assert.AreEqual(3, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromIntegerOne()
	{
		PreciseNumber number = PreciseNumber.CreateFromInteger(1);
		Assert.AreEqual(1, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(1, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromIntegerNegativeOne()
	{
		PreciseNumber number = PreciseNumber.CreateFromInteger(-1);
		Assert.AreEqual(-1, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(1, number.SignificantDigits);
	}

	[TestMethod]
	public void TestCreateFromIntegerZero()
	{
		PreciseNumber number = PreciseNumber.CreateFromInteger(0000);
		Assert.AreEqual(0, number.Significand);
		Assert.AreEqual(0, number.Exponent);
		Assert.AreEqual(0, number.SignificantDigits);
	}

	[TestMethod]
	public void TestMaximumBigInteger()
	{
		BigInteger maxBigInt = BigInteger.Parse("79228162514264337593543950335"); // Decimal.MaxValue
		PreciseNumber number = PreciseNumber.CreateFromComponents(0, maxBigInt);
		Assert.AreEqual(maxBigInt, number.Significand);
	}

	[TestMethod]
	public void TestMinimumBigInteger()
	{
		BigInteger minBigInt = BigInteger.Parse("-79228162514264337593543950335"); // Decimal.MinValue
		PreciseNumber number = PreciseNumber.CreateFromComponents(0, minBigInt);
		Assert.AreEqual(minBigInt, number.Significand);
	}

	[TestMethod]
	public void TestNegativeExponent()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-5, 12345);
		Assert.AreEqual(12345, number.Significand);
		Assert.AreEqual(-5, number.Exponent);
	}

	[TestMethod]
	public void TestTrailingZerosBoundary()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(2, 123000, true);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(5, number.Exponent);
	}

	[TestMethod]
	public void TestToStringWithFormat()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(2, 12345);
		string str = number.ToString("G");
		Assert.AreEqual("1234500", str);
	}

	[TestMethod]
	public void TestToStringWithDifferentCultureThrows()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsExactly<CultureNotFoundException>(() => number.ToString(CultureInfo.GetCultureInfo("fr-FR")));
	}

	[TestMethod]
	public void TestToStringWithInvariantCulture()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		string str = number.ToString(CultureInfo.InvariantCulture);
		Assert.AreEqual("123.45", str);
	}

	[TestMethod]
	public void TestAdditionWithLargeNumbers()
	{
		PreciseNumber largeNum1 = PreciseNumber.CreateFromInteger(BigInteger.Parse("79228162514264337593543950335"));
		PreciseNumber largeNum2 = PreciseNumber.CreateFromInteger(BigInteger.Parse("79228162514264337593543950335"));
		PreciseNumber result = largeNum1 + largeNum2;
		Assert.AreEqual(BigInteger.Parse("15845632502852867518708790067"), result.Significand);
		Assert.AreEqual(1, result.Exponent);
	}

	[TestMethod]
	public void TestSubtractionWithLargeNumbers()
	{
		PreciseNumber largeNum1 = PreciseNumber.CreateFromInteger(BigInteger.Parse("79228162514264337593543950335"));
		PreciseNumber largeNum2 = PreciseNumber.CreateFromInteger(BigInteger.Parse("39228162514264337593543950335"));
		PreciseNumber result = largeNum1 - largeNum2;
		Assert.AreEqual(4, result.Significand);
		Assert.AreEqual(28, result.Exponent);
	}

	[TestMethod]
	public void TestMultiplicationWithSmallNumbers()
	{
		PreciseNumber smallNum1 = PreciseNumber.CreateFromFloatingPoint(0.00001);
		PreciseNumber smallNum2 = PreciseNumber.CreateFromFloatingPoint(0.00002);
		PreciseNumber result = smallNum1 * smallNum2;
		Assert.AreEqual(2, result.Significand);
		Assert.AreEqual(-10, result.Exponent);
	}

	[TestMethod]
	public void TestDivisionWithSmallNumbers()
	{
		PreciseNumber smallNum1 = PreciseNumber.CreateFromFloatingPoint(0.00002);
		PreciseNumber smallNum2 = PreciseNumber.CreateFromFloatingPoint(0.00001);
		PreciseNumber result = smallNum1 / smallNum2;
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
		PreciseNumber additiveIdentity = PreciseNumber.AdditiveIdentity;
		Assert.AreEqual(PreciseNumber.Zero, additiveIdentity);
	}

	[TestMethod]
	public void TestMultiplicativeIdentity()
	{
		PreciseNumber multiplicativeIdentity = PreciseNumber.MultiplicativeIdentity;
		Assert.AreEqual(PreciseNumber.One, multiplicativeIdentity);
	}

	[TestMethod]
	public void TestCreateRepeatingDigits()
	{
		BigInteger result = PreciseNumber.CreateRepeatingDigits(5, 3);
		Assert.AreEqual(new BigInteger(555), result);

		result = PreciseNumber.CreateRepeatingDigits(7, 0);
		Assert.AreEqual(new BigInteger(0), result);
	}

	[TestMethod]
	public void TestHasInfinitePrecision()
	{
		PreciseNumber number = PreciseNumber.One;
		Assert.IsTrue(number.HasInfinitePrecision);

		number = PreciseNumber.CreateFromComponents(0, new BigInteger(2));
		Assert.IsFalse(number.HasInfinitePrecision);
	}

	[TestMethod]
	public void TestLowestDecimalDigits()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(-2, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(-3, 678);
		int result = PreciseNumber.LowestDecimalDigits(number1, number2);
		Assert.AreEqual(2, result);
	}

	[TestMethod]
	public void TestLowestSignificantDigits()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(0, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(0, 678);
		int result = PreciseNumber.LowestSignificantDigits(number1, number2);
		Assert.AreEqual(3, result);
	}

	[TestMethod]
	public void TestReduceSignificance()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(0, 12345);
		PreciseNumber result = number.ReduceSignificance(3);
		Assert.AreEqual(124, result.Significand);
		Assert.AreEqual(2, result.Exponent);
		Assert.AreEqual(number, number.ReduceSignificance(5));
	}

	[TestMethod]
	public void TestMakeCommonizedAndGetExponent()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(1, 123);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(3, 456);
		(PreciseNumber common1, PreciseNumber common2, int result) = PreciseNumber.MakeCommonizedWithExponent(number1, number2);
		Assert.AreEqual(1, result);
		Assert.AreEqual(123, common1.Significand);
		Assert.AreEqual(45600, common2.Significand);
	}

	[TestMethod]
	public void TestAbsStatic()
	{
		PreciseNumber negative = PreciseNumber.NegativeOne;
		PreciseNumber result = PreciseNumber.Abs(negative);
		Assert.AreEqual(PreciseNumber.One, result);
	}

	[TestMethod]
	public void TestOperatorNegate()
	{
		PreciseNumber number = PreciseNumber.One;
		PreciseNumber result = -number;
		Assert.AreEqual(PreciseNumber.NegativeOne, result);
	}

	[TestMethod]
	public void TestOperatorAdd()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(-2, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(-3, 678);
		PreciseNumber result = number1 + number2;
		Assert.AreEqual(124128, result.Significand);
		Assert.AreEqual(-3, result.Exponent);
	}

	[TestMethod]
	public void TestOperatorSubtract()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(-2, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(-3, 678);
		PreciseNumber result = number1 - number2;
		Assert.AreEqual(122772, result.Significand);
		Assert.AreEqual(-3, result.Exponent);
	}

	[TestMethod]
	public void TestOperatorMultiply()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(-2, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(-3, 678);
		PreciseNumber result = number1 * number2;
		Assert.AreEqual(836991, result.Significand);
		Assert.AreEqual(-4, result.Exponent);
	}

	[TestMethod]
	public void TestOperatorDivide()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(-2, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(-3, 678);
		PreciseNumber result = number1 / number2;
		Assert.AreEqual(BigInteger.Parse("18207964601769911504"), result.Significand);
		Assert.AreEqual(-17, result.Exponent);
	}

	[TestMethod]
	public void TestOperatorGreaterThan()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(0, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(0, 678);
		Assert.IsTrue(number1 > number2);
	}

	[TestMethod]
	public void TestOperatorLessThan()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(0, 123);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(0, 678);
		Assert.IsTrue(number1 < number2);
	}

	[TestMethod]
	public void TestOperatorGreaterThanOrEqual()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(0, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(0, 12345);
		Assert.IsTrue(number1 >= number2);
	}

	[TestMethod]
	public void TestOperatorLessThanOrEqual()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(0, 123);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(0, 678);
		Assert.IsTrue(number1 <= number2);
	}

	[TestMethod]
	public void TestOperatorEqual()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(0, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(0, 12345);
		Assert.AreEqual(number2, number1);
	}

	[TestMethod]
	public void TestOperatorNotEqual()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(0, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(0, 678);
		Assert.AreNotEqual(number2, number1);
	}

	[TestMethod]
	public void TestGetHashCode()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(2, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(2, 12345);
		PreciseNumber number3 = PreciseNumber.CreateFromComponents(3, 12345);

		// Test if the same values produce the same hash code
		Assert.AreEqual(number1.GetHashCode(), number2.GetHashCode());

		// Test if different values produce different hash codes
		Assert.AreNotEqual(number1.GetHashCode(), number3.GetHashCode());

		// Additional edge cases
		PreciseNumber zero = PreciseNumber.Zero;
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber negativeOne = PreciseNumber.NegativeOne;

		Assert.AreEqual(zero.GetHashCode(), PreciseNumber.Zero.GetHashCode());
		Assert.AreEqual(one.GetHashCode(), PreciseNumber.One.GetHashCode());
		Assert.AreEqual(negativeOne.GetHashCode(), PreciseNumber.NegativeOne.GetHashCode());
	}

	[TestMethod]
	public void TestEqualsObjectSameInstance()
	{
		PreciseNumber number = PreciseNumber.One;
		Assert.IsTrue(number.Equals((object)number));
	}

	[TestMethod]
	public void TestEqualsObjectEquivalentInstance()
	{
		PreciseNumber number1 = PreciseNumber.One;
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(0, 1);
		Assert.IsTrue(number1.Equals((object)number2));
	}

	[TestMethod]
	public void TestEqualsObjectDifferentInstance()
	{
		PreciseNumber number1 = PreciseNumber.One;
		PreciseNumber number2 = PreciseNumber.Zero;
		Assert.IsFalse(number1.Equals((object)number2));
	}

	[TestMethod]
	public void TestEqualsObjectNull()
	{
		PreciseNumber number = PreciseNumber.One;
		Assert.IsFalse(number.Equals(null));
	}

	[TestMethod]
	public void TestEqualsObjectDifferentType()
	{
		PreciseNumber number = PreciseNumber.One;
		string differentType = "1";
		Assert.IsFalse(number.Equals(differentType));
	}

	[TestMethod]
	public void TestToStringWithFormatAndInvariantCulture()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		string result = number.ToString("G", CultureInfo.InvariantCulture);
		Assert.AreEqual("123.45", result);
	}

	[TestMethod]
	public void TestToStringWithFormatAndSpecificCultureThrows()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsExactly<CultureNotFoundException>(() => number.ToString("G", CultureInfo.GetCultureInfo("fr-FR")));
	}

	[TestMethod]
	public void TestToStringWithNullFormatAndInvariantCulture()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(3, 12345);
		string result = number.ToString(null, CultureInfo.InvariantCulture);
		Assert.AreEqual("12345000", result);
	}

	[TestMethod]
	public void TestToStringWithNullFormatAndSpecificCultureThrows()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(3, 12345);
		Assert.ThrowsExactly<CultureNotFoundException>(() => number.ToString(null, CultureInfo.GetCultureInfo("fr-FR")));
	}

	[TestMethod]
	public void TestToStringWithEmptyFormatAndInvariantCulture()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		string result = number.ToString("", CultureInfo.InvariantCulture);
		Assert.AreEqual("123.45", result);
	}

	[TestMethod]
	public void TestToStringWithEmptyFormatAndSpecificCultureThrows()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsExactly<CultureNotFoundException>(() => number.ToString("", CultureInfo.GetCultureInfo("fr-FR")));
	}

	[TestMethod]
	public void TestTryFormatSufficientBuffer()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("123.45", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatInsufficientBuffer()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Span<char> buffer = stackalloc char[4];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsFalse(result);
		Assert.AreEqual(0, charsWritten);
	}

	[TestMethod]
	public void TestTryFormatEmptyFormat()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Span<char> buffer = stackalloc char[50];
		string format = string.Empty;
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("123.45", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatInvalidFormat()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsExactly<FormatException>(() => number.TryFormat(stackalloc char[50], out int charsWritten, "e", CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void TestTryFormatNullFormatProvider()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), null);

		Assert.IsTrue(result);
		Assert.AreEqual("123.45", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatSpecificCultureThrows()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsExactly<CultureNotFoundException>(() => number.TryFormat(stackalloc char[50], out int charsWritten, "G", CultureInfo.GetCultureInfo("fr-FR")));
	}

	[TestMethod]
	public void TestTryFormatZero()
	{
		PreciseNumber number = PreciseNumber.Zero;
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("0", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatOne()
	{
		PreciseNumber number = PreciseNumber.One;
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("1", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatNegativeOne()
	{
		PreciseNumber number = PreciseNumber.NegativeOne;
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("-1", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatInteger()
	{
		PreciseNumber number = 3.ToPreciseNumber();
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("3", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestTryFormatFloat()
	{
		PreciseNumber number = 3.0.ToPreciseNumber();
		Span<char> buffer = stackalloc char[50];
		string format = "G";
		bool result = number.TryFormat(buffer, out int charsWritten, format.AsSpan(), CultureInfo.InvariantCulture);

		Assert.IsTrue(result);
		Assert.AreEqual("3", buffer[..charsWritten].ToString());
	}

	[TestMethod]
	public void TestAddLargeNumbers()
	{
		PreciseNumber largeNumber1 = PreciseNumber.CreateFromComponents(100, BigInteger.Parse("79228162514264337593543950335"));
		PreciseNumber largeNumber2 = PreciseNumber.CreateFromComponents(100, BigInteger.Parse("79228162514264337593543950335"));
		PreciseNumber result = largeNumber1 + largeNumber2;
		Assert.AreEqual(BigInteger.Parse("15845632502852867518708790067"), result.Significand);
		Assert.AreEqual(101, result.Exponent);
	}

	[TestMethod]
	public void TestSubtractLargeNumbers()
	{
		PreciseNumber largeNumber1 = PreciseNumber.CreateFromComponents(100, BigInteger.Parse("79228162514264337593543950335"));
		PreciseNumber largeNumber2 = PreciseNumber.CreateFromComponents(100, BigInteger.Parse("39228162514264337593543950335"));
		PreciseNumber result = largeNumber1 - largeNumber2;
		Assert.AreEqual(BigInteger.Parse("4"), result.Significand);
		Assert.AreEqual(128, result.Exponent);
	}

	[TestMethod]
	public void TestMultiplyLargeNumbers()
	{
		PreciseNumber largeNumber1 = PreciseNumber.CreateFromComponents(50, BigInteger.Parse("79228162514264337593543950335"));
		PreciseNumber largeNumber2 = PreciseNumber.CreateFromComponents(50, BigInteger.Parse("2"));
		PreciseNumber result = largeNumber1 * largeNumber2;
		Assert.AreEqual(BigInteger.Parse("15845632502852867518708790067"), result.Significand);
		Assert.AreEqual(101, result.Exponent);
	}

	[TestMethod]
	public void TestDivideLargeNumbers()
	{
		PreciseNumber largeNumber1 = PreciseNumber.CreateFromComponents(100, BigInteger.Parse("79228162514264337593543950335"));
		PreciseNumber largeNumber2 = PreciseNumber.CreateFromComponents(1, BigInteger.Parse("2"));
		PreciseNumber result = largeNumber1 / largeNumber2;
		Assert.AreEqual(BigInteger.Parse("396140812571321687967719751675"), result.Significand);
		Assert.AreEqual(98, result.Exponent);
	}

	[TestMethod]
	public void TestAddZero()
	{
		PreciseNumber zero = PreciseNumber.Zero;
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber result = zero + one;
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestSubtractZero()
	{
		PreciseNumber zero = PreciseNumber.Zero;
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber result = one - zero;
		Assert.AreEqual(one, result);
	}

	[TestMethod]
	public void TestMultiplyZero()
	{
		PreciseNumber zero = PreciseNumber.Zero;
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber result = one * zero;
		Assert.AreEqual(zero, result);
	}

	[TestMethod]
	public void TestDivideZero()
	{
		PreciseNumber zero = PreciseNumber.Zero;
		Assert.ThrowsExactly<DivideByZeroException>(() => zero / zero);
	}

	[TestMethod]
	public void TestCreateFromFloatingPointSpecialValues()
	{
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.CreateFromFloatingPoint(double.NaN));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.CreateFromFloatingPoint(double.PositiveInfinity));
		Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => PreciseNumber.CreateFromFloatingPoint(double.NegativeInfinity));
	}

	[TestMethod]
	public void TestCreateFromIntegerBoundaryValues()
	{
		PreciseNumber intMax = PreciseNumber.CreateFromInteger(int.MaxValue);
		Assert.AreEqual(BigInteger.Parse(int.MaxValue.ToString()), intMax.Significand);

		PreciseNumber intMin = PreciseNumber.CreateFromInteger(int.MinValue);
		Assert.AreEqual(BigInteger.Parse(int.MinValue.ToString()), intMin.Significand);

		PreciseNumber longMax = PreciseNumber.CreateFromInteger(long.MaxValue);
		Assert.AreEqual(BigInteger.Parse(long.MaxValue.ToString()), longMax.Significand);

		PreciseNumber longMin = PreciseNumber.CreateFromInteger(long.MinValue);
		Assert.AreEqual(BigInteger.Parse(long.MinValue.ToString()), longMin.Significand);
	}

	[TestMethod]
	public void TestNegativeExponentHandling()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-3, 12345);
		Assert.AreEqual(12345, number.Significand);
		Assert.AreEqual(-3, number.Exponent);

		PreciseNumber result = number.Round(2);
		Assert.AreEqual(1235, result.Significand); // After rounding, check if the exponent and significand are adjusted correctly
		Assert.AreEqual(-2, result.Exponent);
	}

	[TestMethod]
	public void TestHandlingTrailingZeros()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(2, 123000, true);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(5, number.Exponent); // Ensure trailing zeros are removed and exponent is adjusted correctly

		number = PreciseNumber.CreateFromComponents(-2, 123000, true);
		Assert.AreEqual(123, number.Significand);
		Assert.AreEqual(1, number.Exponent);
	}

	[TestMethod]
	public void TestToStringVariousFormats()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsExactly<FormatException>(() => number.ToString("E2", CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<FormatException>(() => number.ToString("F2", CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<FormatException>(() => number.ToString("N2", CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void TestTryFormatVariousFormats()
	{
		PreciseNumber number = PreciseNumber.CreateFromComponents(-2, 12345);
		Assert.ThrowsExactly<FormatException>(() => number.TryFormat(stackalloc char[50], out int charsWritten, "E2".AsSpan(), CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<FormatException>(() => number.TryFormat(stackalloc char[50], out int charsWritten, "F2".AsSpan(), CultureInfo.InvariantCulture));
		Assert.ThrowsExactly<FormatException>(() => number.TryFormat(stackalloc char[50], out int charsWritten, "N2".AsSpan(), CultureInfo.InvariantCulture));
	}

	[TestMethod]
	public void ToDouble()
	{
		PreciseNumber preciseNumber = PreciseNumber.CreateFromComponents(3, 12345); // 12345e3
		double result = preciseNumber.To<double>();
		Assert.AreEqual(12345e3, result);
	}

	[TestMethod]
	public void ToFloat()
	{
		PreciseNumber preciseNumber = PreciseNumber.CreateFromComponents(2, 12345); // 12345e2
		float result = preciseNumber.To<float>();
		Assert.AreEqual(12345e2f, result);
	}

	[TestMethod]
	public void ToDecimal()
	{
		PreciseNumber preciseNumber = PreciseNumber.CreateFromComponents(1, 12345); // 12345e1
		decimal result = preciseNumber.To<decimal>();
		Assert.AreEqual(12345e1m, result);
	}

	[TestMethod]
	public void ToInt()
	{
		PreciseNumber preciseNumber = PreciseNumber.CreateFromComponents(0, 12345); // 12345e0
		int result = preciseNumber.To<int>();
		Assert.AreEqual(12345, result);
	}

	[TestMethod]
	public void ToLong()
	{
		PreciseNumber preciseNumber = PreciseNumber.CreateFromComponents(0, 123456789012345); // 123456789012345e0
		long result = preciseNumber.To<long>();
		Assert.AreEqual(123456789012345L, result);
	}

	[TestMethod]
	public void ToBigInteger()
	{
		PreciseNumber preciseNumber = PreciseNumber.CreateFromComponents(5, 12345); // 12345e5
		BigInteger result = preciseNumber.To<BigInteger>();
		Assert.AreEqual(BigInteger.Parse("1234500000"), result);
	}

	[TestMethod]
	public void ToOverflow()
	{
		PreciseNumber preciseNumber = PreciseNumber.CreateFromComponents(1000, 12345); // This is a very large number
		Assert.ThrowsExactly<OverflowException>(() => preciseNumber.To<int>()); // This should throw an exception
	}

	[TestMethod]
	public void SquaredShouldReturnCorrectValue()
	{
		// Arrange
		PreciseNumber number = 3.ToPreciseNumber();
		PreciseNumber expected = 9.ToPreciseNumber();

		// Act
		PreciseNumber result = number.Squared();

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void CubedShouldReturnCorrectValue()
	{
		// Arrange
		PreciseNumber number = 3.ToPreciseNumber();
		PreciseNumber expected = 27.ToPreciseNumber();

		// Act
		PreciseNumber result = number.Cubed();

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void PowShouldReturnCorrectValue()
	{
		// Arrange
		PreciseNumber number = 2.ToPreciseNumber();
		PreciseNumber expected = 8.ToPreciseNumber();

		// Act
		PreciseNumber result = number.Pow(3.ToPreciseNumber());

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
		PreciseNumber number = 5.ToPreciseNumber();
		PreciseNumber expected = PreciseNumber.One;

		// Act
		PreciseNumber result = number.Pow(0.ToPreciseNumber());

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void PowNegativePowerShouldReturnCorrectValue()
	{
		// Arrange
		PreciseNumber number = 2.ToPreciseNumber();
		PreciseNumber expected = 0.125.ToPreciseNumber();

		// Act
		PreciseNumber result = number.Pow(-3.ToPreciseNumber());

		// Assert
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestExpWithZeroPower()
	{
		PreciseNumber result = PreciseNumber.Exp(0.ToPreciseNumber());
		PreciseNumber expected = PreciseNumber.One; // e^0 = 1
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestExpWithPositivePower()
	{
		PreciseNumber result = PreciseNumber.Exp(1.ToPreciseNumber());
		PreciseNumber expected = PreciseNumber.E; // e^1 = e
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestExpWithNegativePower()
	{
		PreciseNumber result = PreciseNumber.Exp(-1.ToPreciseNumber());
		PreciseNumber expected = PreciseNumber.One / PreciseNumber.E; // e^-1 = 1/e
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestExpWithLargePositivePower()
	{
		PreciseNumber result = PreciseNumber.Exp(5.ToPreciseNumber());
		PreciseNumber expected = 148.4131591025766m.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestExpWithLargeNegativePower()
	{
		PreciseNumber result = PreciseNumber.Exp(-5.ToPreciseNumber());
		PreciseNumber expected = 0.006737946999085467m.ToPreciseNumber();
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestRoundWithSamePrecision()
	{
		PreciseNumber number = 1234.5.ToPreciseNumber();
		PreciseNumber result = number.Round(1);
		Assert.AreEqual(number, result);
	}

	[TestMethod]
	public void TestDivideByZero()
	{
		PreciseNumber number = 1234.5.ToPreciseNumber();
		Assert.ThrowsExactly<DivideByZeroException>(() => number / PreciseNumber.Zero);
	}

	[TestMethod]
	public void TestModZero()
	{
		PreciseNumber number = 1234.5.ToPreciseNumber();
		Assert.ThrowsExactly<DivideByZeroException>(() => number % PreciseNumber.Zero);
	}

	[TestMethod]
	public void TestDivideBySelf()
	{
		PreciseNumber number = 1234.5.ToPreciseNumber();
		PreciseNumber result = number / number;
		Assert.AreEqual(PreciseNumber.One, result);
	}

	[TestMethod]
	public void TestEasyMultiplies()
	{
		PreciseNumber two = 2.ToPreciseNumber();
		Assert.AreEqual(two, PreciseNumber.One * two);
		Assert.AreEqual(two, two * PreciseNumber.One);
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.Zero * PreciseNumber.One);
		Assert.AreEqual(PreciseNumber.Zero, PreciseNumber.One * PreciseNumber.Zero);
	}

	[TestMethod]
	public void TestParseWithValidInput()
	{
		ReadOnlySpan<char> input = "1.23E4".AsSpan();
		PreciseNumber expected = 1.23e4.ToPreciseNumber();
		PreciseNumber result = PreciseNumber.Parse(input, NumberStyles.Any, null);
		Assert.AreEqual(expected, result);

		result = PreciseNumber.Parse(input, null);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestParseWithZero()
	{
		ReadOnlySpan<char> input = "0".AsSpan();
		PreciseNumber result = PreciseNumber.Parse(input, NumberStyles.Any, null);
		Assert.AreEqual(PreciseNumber.Zero, result);
	}

	[TestMethod]
	public void TestParseWithNegativeInput()
	{
		ReadOnlySpan<char> input = "-5.67E-2".AsSpan();
		PreciseNumber expected = -5.67e-2.ToPreciseNumber();
		PreciseNumber result = PreciseNumber.Parse(input, NumberStyles.Any, null);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestParseWithInvalidInput()
	{
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse("1.2.3E4".AsSpan(), NumberStyles.Any, null));
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse("invalid".AsSpan(), NumberStyles.Any, null));
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse(string.Empty.AsSpan(), NumberStyles.Any, null));
	}

	[TestMethod]
	public void TestParseStringWithValidInput()
	{
		string input = "1.23E4";
		PreciseNumber expected = 1.23e4.ToPreciseNumber();
		PreciseNumber result = PreciseNumber.Parse(input, NumberStyles.Any, null);
		Assert.AreEqual(expected, result);

		result = PreciseNumber.Parse(input, null);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestParseStringWithNegativeInput()
	{
		string input = "-5.67E-2";
		PreciseNumber expected = -5.67e-2.ToPreciseNumber();
		PreciseNumber result = PreciseNumber.Parse(input, NumberStyles.Any, null);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestParseStringWithInvalidInput()
	{
		string input = "invalid";
		Assert.ThrowsExactly<FormatException>(() => PreciseNumber.Parse(input, NumberStyles.Any, null));
	}

	[TestMethod]
	public void TestTryParseWithValidInput()
	{
		ReadOnlySpan<char> input = "1.23E4".AsSpan();
		PreciseNumber expected = 1.23e4.ToPreciseNumber();
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out PreciseNumber? result);
		Assert.IsTrue(success);
		Assert.AreEqual(expected, result);

		success = PreciseNumber.TryParse(input, provider: null, out result);
		Assert.IsTrue(success);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestTryParseWithNegativeInput()
	{
		ReadOnlySpan<char> input = "-5.67E-2".AsSpan();
		PreciseNumber expected = -5.67e-2.ToPreciseNumber();
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out PreciseNumber? result);
		Assert.IsTrue(success);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestTryParseWithInvalidInput()
	{
		ReadOnlySpan<char> input = "invalid".AsSpan();
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out PreciseNumber? result);
		Assert.IsFalse(success);
		Assert.AreEqual(default, result);
	}

	[TestMethod]
	public void TestTryParseStringWithValidInput()
	{
		string input = "1.23E4";
		PreciseNumber expected = 1.23e4.ToPreciseNumber();
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out PreciseNumber? result);
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
		PreciseNumber expected = -5.67e-2.ToPreciseNumber();
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out PreciseNumber? result);
		Assert.IsTrue(success);
		Assert.AreEqual(expected, result);
	}

	[TestMethod]
	public void TestTryParseStringWithInvalidInput()
	{
		string input = "invalid";
		bool success = PreciseNumber.TryParse(input, NumberStyles.Any, null, out PreciseNumber? result);
		Assert.IsFalse(success);
		Assert.AreEqual(default, result);
	}

	[TestMethod]
	public void TestEValue()
	{
		BigInteger expectedSignificand = BigInteger.Parse("27182818284590452353602874713526624977572", CultureInfo.InvariantCulture);
		int expectedExponent = -40;
		PreciseNumber eValue = PreciseNumber.E;

		Assert.AreEqual(expectedSignificand, eValue.Significand);
		Assert.AreEqual(expectedExponent, eValue.Exponent);
	}

	[TestMethod]
	public void TestTauValue()
	{
		BigInteger expectedSignificand = BigInteger.Parse("6283185307179586476925287", CultureInfo.InvariantCulture);
		int expectedExponent = -24;
		PreciseNumber tauValue = PreciseNumber.Tau;

		Assert.AreEqual(expectedSignificand, tauValue.Significand);
		Assert.AreEqual(expectedExponent, tauValue.Exponent);
	}

	[TestMethod]
	public void TestPiValue()
	{
		BigInteger expectedSignificand = BigInteger.Parse("31415926535897932384626433", CultureInfo.InvariantCulture);
		int expectedExponent = -25;
		PreciseNumber piValue = PreciseNumber.Pi;
		Assert.AreEqual(expectedSignificand, piValue.Significand);
		Assert.AreEqual(expectedExponent, piValue.Exponent);
	}

	[TestMethod]
	public void TestNotEqual()
	{
		PreciseNumber number1 = PreciseNumber.CreateFromComponents(0, 12345);
		PreciseNumber number2 = PreciseNumber.CreateFromComponents(0, 678);
		PreciseNumber number3 = PreciseNumber.CreateFromComponents(0, 12345);

		Assert.IsTrue(PreciseNumber.NotEqual(number1, number2));
		Assert.IsFalse(PreciseNumber.NotEqual(number1, number3));
	}
	[TestMethod]
	public void TestCompareToINumberWithNull()
	{
		PreciseNumber one = PreciseNumber.One;
		Assert.AreEqual(1, one.CompareTo<PreciseNumber>(null!));
	}

	[TestMethod]
	public void TestCompareToINumberWithSmallerValue()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber zero = PreciseNumber.Zero;
		Assert.IsTrue(one.CompareTo<PreciseNumber>(zero) > 0);
	}

	[TestMethod]
	public void TestCompareToINumberWithLargerValue()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber two = 2.ToPreciseNumber();
		Assert.IsTrue(one.CompareTo<PreciseNumber>(two) < 0);
	}

	[TestMethod]
	public void TestCompareToINumberWithEqualValue()
	{
		PreciseNumber one = PreciseNumber.One;
		PreciseNumber anotherOne = 1.ToPreciseNumber();
		Assert.AreEqual(0, one.CompareTo<PreciseNumber>(anotherOne));
	}

	[TestMethod]
	public void TryCreate_WithIntegerInput_ReturnsTrueAndCreatesPreciseNumber()
	{
		// Arrange  
		int input = 42;

		// Act  
		bool result = PreciseNumberExtensions.TryCreate(input, out PreciseNumber? preciseNumber);

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
		bool result = PreciseNumberExtensions.TryCreate(input, out PreciseNumber? preciseNumber);

		// Assert  
		Assert.IsTrue(result);
		Assert.IsNotNull(preciseNumber);
		Assert.AreEqual(input, preciseNumber.To<double>());
	}

	[TestMethod]
	public void TryCreate_InputIsPreciseNumber_ReturnsTrue()
	{
		// Arrange
		PreciseNumber input = new(2, new BigInteger(12345));

		// Act
		bool result = PreciseNumberExtensions.TryCreate(input, out PreciseNumber? preciseNumber);

		// Assert
		Assert.IsTrue(result);
		Assert.IsNotNull(preciseNumber);
		Assert.AreEqual(input, preciseNumber);
	}

	[TestMethod]
	public void TryCreate_WithPreciseNumber_ReturnsTrue()
	{
		// Arrange  
		PreciseNumber input = PreciseNumber.One;

		// Act  
		bool result = PreciseNumberExtensions.TryCreate(input, out PreciseNumber? preciseNumber);

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
		bool result = PreciseNumberExtensions.TryCreate(input, out PreciseNumber? preciseNumber);

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
		bool result = PreciseNumberExtensions.TryCreate(input, out PreciseNumber? preciseNumber);

		// Assert  
		Assert.IsTrue(result);
		Assert.IsNotNull(preciseNumber);
		Assert.AreEqual(PreciseNumber.CreateFromFloatingPoint(input), preciseNumber);
	}

	[TestMethod]
	public void As_WithSameInputAndOutputType_ReturnsInput()
	{
		// Arrange
		PreciseNumber input = new(2, new BigInteger(123));

		// Act
		PreciseNumber result = input.As<PreciseNumber>();

		// Assert
		Assert.AreSame(input, result);
	}

	[TestMethod]
	public void As_WithConvertibleInputAndOutputType_ReturnsConvertedInstance()
	{
		// Arrange
		PreciseNumber input = new(2, new BigInteger(123));

		// Act
		DerivedPreciseNumber result = input.As<DerivedPreciseNumber>();

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
