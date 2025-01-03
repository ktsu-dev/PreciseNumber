// Ignore Spelling: Commonized

[assembly: CLSCompliant(true)]
[assembly: System.Runtime.InteropServices.ComVisible(false)]
namespace ktsu.PreciseNumber;

using System;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;

public record PreciseNumber : PreciseNumber<PreciseNumber> { }

/// <summary>
/// Represents a precise number.
/// </summary>
[DebuggerDisplay("{Significand}e{Exponent}")]
public record PreciseNumber<TSelf>
	: PreciseNumberBase
	, INumber<TSelf>
	where TSelf : PreciseNumber<TSelf>, new()
{
	/// <summary>
	/// Gets the value -1 for the type.
	/// </summary>
	public static new TSelf NegativeOne { get; } = PreciseNumberBase.NegativeOne.As<TSelf>();

	/// <inheritdoc/>
	public static new TSelf One { get; } = PreciseNumberBase.One.As<TSelf>();

	/// <inheritdoc/>
	public static new TSelf Zero { get; } = PreciseNumberBase.Zero.As<TSelf>();

	/// <summary>
	/// Gets the value of e for the type.
	/// </summary>
	public static new TSelf E { get; } = PreciseNumberBase.E.As<TSelf>();

	/// <summary>
	/// Gets the value of pi for the type.
	/// </summary>
	public static new TSelf Pi { get; } = PreciseNumberBase.Pi.As<TSelf>();

	/// <summary>
	/// Gets the value of tau for the type.
	/// </summary>
	public static new TSelf Tau { get; } = PreciseNumberBase.Tau.As<TSelf>();


	/// <inheritdoc/>
	public static new int Radix => PreciseNumberBase.Radix;

	/// <inheritdoc/>
	public static new TSelf AdditiveIdentity => PreciseNumberBase.AdditiveIdentity.As<TSelf>();

	/// <inheritdoc/>
	public static new TSelf MultiplicativeIdentity => PreciseNumberBase.MultiplicativeIdentity.As<TSelf>();

	public static TSelf Abs(TSelf value) => value.Abs().As<TSelf>();
	public static bool IsCanonical(TSelf value) => value.IsCanonical();
	public static bool IsComplexNumber(TSelf value) => value.IsComplexNumber();
	public static bool IsEvenInteger(TSelf value) => value.IsEvenInteger();
	public static bool IsFinite(TSelf value) => value.IsFinite();
	public static bool IsImaginaryNumber(TSelf value) => value.IsImaginaryNumber();
	public static bool IsInfinity(TSelf value) => value.IsInfinity();
	public static bool IsInteger(TSelf value) => value.IsInteger();
	public static bool IsNaN(TSelf value) => value.IsNaN();
	public static bool IsNegative(TSelf value) => value.IsNegative();
	public static bool IsNegativeInfinity(TSelf value) => value.IsNegativeInfinity();
	public static bool IsNormal(TSelf value) => value.IsNormal();
	public static bool IsOddInteger(TSelf value) => value.IsOddInteger();
	public static bool IsPositive(TSelf value) => value.IsPositive();
	public static bool IsPositiveInfinity(TSelf value) => value.IsPositiveInfinity();
	public static bool IsRealNumber(TSelf value) => value.IsRealNumber();
	public static bool IsSubnormal(TSelf value) => value.IsSubnormal();
	public static bool IsZero(TSelf value) => value.IsZero();
	public static TSelf MaxMagnitude(TSelf x, TSelf y) => x.MaxMagnitude(y).As<TSelf>();
	public static TSelf MaxMagnitudeNumber(TSelf x, TSelf y) => x.MaxMagnitudeNumber(y).As<TSelf>();
	public static TSelf MinMagnitude(TSelf x, TSelf y) => x.MinMagnitude(y).As<TSelf>();
	public static TSelf MinMagnitudeNumber(TSelf x, TSelf y) => x.MinMagnitudeNumber(y).As<TSelf>();

	/// <inheritdoc/>
	public static bool TryConvertFromChecked<TOther>(TOther value, out TSelf result)
		where TOther : INumberBase<TOther>
	{
		bool success = PreciseNumberBase.TryConvertFromChecked(value, out var r);
		result = r.As<TSelf>();
		return success;
	}

	/// <inheritdoc/>
	public static bool TryConvertFromSaturating<TOther>(TOther value, out TSelf result)
		where TOther : INumberBase<TOther>
	{
		bool success = PreciseNumberBase.TryConvertFromSaturating(value, out var r);
		result = r.As<TSelf>();
		return success;
	}

	/// <inheritdoc/>
	public static bool TryConvertFromTruncating<TOther>(TOther value, out TSelf result)
		where TOther : INumberBase<TOther>
	{
		bool success = PreciseNumberBase.TryConvertFromTruncating(value, out var r);
		result = r.As<TSelf>();
		return success;
	}

	/// <inheritdoc/>
	public static bool TryConvertToChecked<TOther>(TSelf value, out TOther result)
		where TOther : INumberBase<TOther> =>
		PreciseNumberBase.TryConvertToChecked(value, out result);

	/// <inheritdoc/>
	public static bool TryConvertToSaturating<TOther>(TSelf value, out TOther result)
		where TOther : INumberBase<TOther> =>
		PreciseNumberBase.TryConvertToSaturating(value, out result);

	/// <inheritdoc/>
	public static bool TryConvertToTruncating<TOther>(TSelf value, out TOther result)
		where TOther : INumberBase<TOther> =>
		PreciseNumberBase.TryConvertToTruncating(value, out result);

	public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out TSelf result)
	{
		bool success = PreciseNumberBase.TryParse(s, style, provider, out var r);
		result = r.As<TSelf>();
		return success;
	}

	public static bool TryParse(string? s, NumberStyles style, IFormatProvider? provider, out TSelf result)
	{
		bool success = PreciseNumberBase.TryParse(s, style, provider, out var r);
		result = r.As<TSelf>();
		return success;
	}

	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out TSelf result)
	{
		bool success = PreciseNumberBase.TryParse(s, provider, out var r);
		result = r.As<TSelf>();
		return success;
	}

	public static bool TryParse(string? s, IFormatProvider? provider, out TSelf result)
	{
		bool success = PreciseNumberBase.TryParse(s, provider, out var r);
		result = r.As<TSelf>();
		return success;
	}

	public int CompareTo(TSelf? other) => Compare(this, other);

	static TSelf INumberBase<TSelf>.Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => Parse(s, style, provider).As<TSelf>();
	static TSelf INumberBase<TSelf>.Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s, style, provider).As<TSelf>();
	static TSelf ISpanParsable<TSelf>.Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s, provider).As<TSelf>();
	static TSelf IParsable<TSelf>.Parse(string s, IFormatProvider? provider) => Parse(s, provider).As<TSelf>();
	static bool IComparisonOperators<TSelf, TSelf, bool>.operator >(TSelf left, TSelf right) => left.GreaterThan(right);
	static bool IComparisonOperators<TSelf, TSelf, bool>.operator >=(TSelf left, TSelf right) => left.GreaterThanOrEqualTo(right);
	static bool IComparisonOperators<TSelf, TSelf, bool>.operator <(TSelf left, TSelf right) => left.LessThan(right);
	static bool IComparisonOperators<TSelf, TSelf, bool>.operator <=(TSelf left, TSelf right) => left.LessThanOrEqualTo(right);
	static TSelf IModulusOperators<TSelf, TSelf, TSelf>.operator %(TSelf left, TSelf right) => left.Modulo(right).As<TSelf>();
	static TSelf IAdditionOperators<TSelf, TSelf, TSelf>.operator +(TSelf left, TSelf right) => left.Add(right).As<TSelf>();
	static TSelf IDecrementOperators<TSelf>.operator --(TSelf value) => value.Decrement().As<TSelf>();
	static TSelf IDivisionOperators<TSelf, TSelf, TSelf>.operator /(TSelf left, TSelf right) => left.Divide(right).As<TSelf>();
	static bool IEqualityOperators<TSelf, TSelf, bool>.operator ==(TSelf? left, TSelf? right) => left?.EqualTo(right) ?? false;
	static bool IEqualityOperators<TSelf, TSelf, bool>.operator !=(TSelf? left, TSelf? right) => left?.NotEqualTo(right) ?? false;
	static TSelf IIncrementOperators<TSelf>.operator ++(TSelf value) => value.Increment().As<TSelf>();
	static TSelf IMultiplyOperators<TSelf, TSelf, TSelf>.operator *(TSelf left, TSelf right) => left.Multiply(right).As<TSelf>();
	static TSelf ISubtractionOperators<TSelf, TSelf, TSelf>.operator -(TSelf left, TSelf right) => left.Subtract(right).As<TSelf>();
	static TSelf IUnaryNegationOperators<TSelf, TSelf>.operator -(TSelf value) => value.Negate().As<TSelf>();
	static TSelf IUnaryPlusOperators<TSelf, TSelf>.operator +(TSelf value) => value.UnaryPlus().As<TSelf>();

	public static bool operator <(PreciseNumber<TSelf> left, PreciseNumber<TSelf> right) => left is null ? right is not null : left.CompareTo(right) < 0;

	public static bool operator <=(PreciseNumber<TSelf> left, PreciseNumber<TSelf> right) => left is null || left.CompareTo(right) <= 0;

	public static bool operator >(PreciseNumber<TSelf> left, PreciseNumber<TSelf> right) => left is not null && left.CompareTo(right) > 0;

	public static bool operator >=(PreciseNumber<TSelf> left, PreciseNumber<TSelf> right) => left is null ? right is null : left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static TSelf operator -(PreciseNumber<TSelf> value) =>
		value.Negate().As<TSelf>();

	/// <inheritdoc/>
	public static TSelf operator -(PreciseNumber<TSelf> left, PreciseNumberBase right) =>
		left.Subtract(right).As<TSelf>();

	/// <inheritdoc/>
	public static TSelf operator *(PreciseNumber<TSelf> left, PreciseNumberBase right) =>
		left.Multiply(right).As<TSelf>();

	/// <inheritdoc/>
	public static TSelf operator /(PreciseNumber<TSelf> left, PreciseNumberBase right) =>
		left.Divide(right).As<TSelf>();

	/// <inheritdoc/>
	public static TSelf operator +(PreciseNumber<TSelf> value) =>
		value.UnaryPlus().As<TSelf>();

	/// <inheritdoc/>
	public static TSelf operator +(PreciseNumber<TSelf> left, PreciseNumberBase right) =>
		left.Add(right).As<TSelf>();

	/// <inheritdoc/>
	public static bool operator >(PreciseNumber<TSelf> left, PreciseNumberBase right) =>
		left.GreaterThan(right);

	/// <inheritdoc/>
	public static bool operator <(PreciseNumber<TSelf> left, PreciseNumberBase right) =>
		left.LessThan(right);

	/// <inheritdoc/>
	public static bool operator >=(PreciseNumber<TSelf> left, PreciseNumberBase right) =>
		left.GreaterThanOrEqualTo(right);

	/// <inheritdoc/>
	public static bool operator <=(PreciseNumber<TSelf> left, PreciseNumberBase right) =>
		left.LessThanOrEqualTo(right);

	/// <inheritdoc/>
	public static TSelf operator %(PreciseNumber<TSelf> left, PreciseNumberBase right) =>
		left.Modulo(right).As<TSelf>();

	/// <inheritdoc/>
	public static TSelf operator --(PreciseNumber<TSelf> value) =>
		value.Decrement().As<TSelf>();

	/// <inheritdoc/>
	public static TSelf operator ++(PreciseNumber<TSelf> value) =>
		value.Increment().As<TSelf>();

	public virtual bool Equals(TSelf? other) => EqualTo(other);
}
