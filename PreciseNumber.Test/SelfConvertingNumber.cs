// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Test;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

/// <summary>
/// A number that is neither an integer nor a floating-point type, as <c>ktsu.SignificantNumber</c> is,
/// and that converts itself to <see cref="PreciseNumber"/> through <c>TryConvertToChecked</c> when
/// <see cref="CanConvert"/> is set, and to nothing otherwise.
/// </summary>
/// <param name="Value">The value it holds.</param>
/// <param name="CanConvert">Whether it can convert itself to <see cref="PreciseNumber"/>.</param>
internal readonly record struct SelfConvertingNumber(decimal Value, bool CanConvert) : INumber<SelfConvertingNumber>
{
	public static SelfConvertingNumber One => new(1m, true);
	public static int Radix => 10;
	public static SelfConvertingNumber Zero => new(0m, true);
	public static SelfConvertingNumber AdditiveIdentity => Zero;
	public static SelfConvertingNumber MultiplicativeIdentity => One;

	private SelfConvertingNumber With(decimal value) => new(value, CanConvert);

	public static bool TryConvertToChecked<TOther>(SelfConvertingNumber value, [MaybeNullWhen(false)] out TOther result)
		where TOther : INumberBase<TOther>
	{
		if (value.CanConvert && typeof(TOther) == typeof(PreciseNumber))
		{
			result = (TOther)(object)value.Value.ToPreciseNumber();
			return true;
		}

		result = default;
		return false;
	}

	public static bool TryConvertToSaturating<TOther>(SelfConvertingNumber value, [MaybeNullWhen(false)] out TOther result)
		where TOther : INumberBase<TOther> => TryConvertToChecked(value, out result);

	public static bool TryConvertToTruncating<TOther>(SelfConvertingNumber value, [MaybeNullWhen(false)] out TOther result)
		where TOther : INumberBase<TOther> => TryConvertToChecked(value, out result);

	public static bool TryConvertFromChecked<TOther>(TOther value, out SelfConvertingNumber result)
		where TOther : INumberBase<TOther>
	{
		result = default;
		return false;
	}

	public static bool TryConvertFromSaturating<TOther>(TOther value, out SelfConvertingNumber result)
		where TOther : INumberBase<TOther> => TryConvertFromChecked(value, out result);

	public static bool TryConvertFromTruncating<TOther>(TOther value, out SelfConvertingNumber result)
		where TOther : INumberBase<TOther> => TryConvertFromChecked(value, out result);

	public static SelfConvertingNumber Abs(SelfConvertingNumber value) => value.With(Math.Abs(value.Value));
	public static bool IsCanonical(SelfConvertingNumber value) => true;
	public static bool IsComplexNumber(SelfConvertingNumber value) => false;
	public static bool IsEvenInteger(SelfConvertingNumber value) => decimal.IsEvenInteger(value.Value);
	public static bool IsFinite(SelfConvertingNumber value) => true;
	public static bool IsImaginaryNumber(SelfConvertingNumber value) => false;
	public static bool IsInfinity(SelfConvertingNumber value) => false;
	public static bool IsInteger(SelfConvertingNumber value) => decimal.IsInteger(value.Value);
	public static bool IsNaN(SelfConvertingNumber value) => false;
	public static bool IsNegative(SelfConvertingNumber value) => decimal.IsNegative(value.Value);
	public static bool IsNegativeInfinity(SelfConvertingNumber value) => false;
	public static bool IsNormal(SelfConvertingNumber value) => value.Value != 0m;
	public static bool IsOddInteger(SelfConvertingNumber value) => decimal.IsOddInteger(value.Value);
	public static bool IsPositive(SelfConvertingNumber value) => decimal.IsPositive(value.Value);
	public static bool IsPositiveInfinity(SelfConvertingNumber value) => false;
	public static bool IsRealNumber(SelfConvertingNumber value) => true;
	public static bool IsSubnormal(SelfConvertingNumber value) => false;
	public static bool IsZero(SelfConvertingNumber value) => value.Value == 0m;
	public static SelfConvertingNumber MaxMagnitude(SelfConvertingNumber x, SelfConvertingNumber y) => x.With(decimal.MaxMagnitude(x.Value, y.Value));
	public static SelfConvertingNumber MaxMagnitudeNumber(SelfConvertingNumber x, SelfConvertingNumber y) => MaxMagnitude(x, y);
	public static SelfConvertingNumber MinMagnitude(SelfConvertingNumber x, SelfConvertingNumber y) => x.With(decimal.MinMagnitude(x.Value, y.Value));
	public static SelfConvertingNumber MinMagnitudeNumber(SelfConvertingNumber x, SelfConvertingNumber y) => MinMagnitude(x, y);

	public static SelfConvertingNumber Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => new(decimal.Parse(s, style, provider), true);
	public static SelfConvertingNumber Parse(string s, NumberStyles style, IFormatProvider? provider) => new(decimal.Parse(s, style, provider), true);
	public static SelfConvertingNumber Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => new(decimal.Parse(s, provider), true);
	public static SelfConvertingNumber Parse(string s, IFormatProvider? provider) => new(decimal.Parse(s, provider), true);

	public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out SelfConvertingNumber result)
	{
		bool parsed = decimal.TryParse(s, style, provider, out decimal value);
		result = new(value, true);
		return parsed;
	}

	public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out SelfConvertingNumber result) =>
		TryParse(s.AsSpan(), style, provider, out result);

	public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out SelfConvertingNumber result) =>
		TryParse(s, NumberStyles.Number, provider, out result);

	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out SelfConvertingNumber result) =>
		TryParse(s.AsSpan(), NumberStyles.Number, provider, out result);

	public int CompareTo(object? obj) => obj is SelfConvertingNumber other ? CompareTo(other) : 1;
	public int CompareTo(SelfConvertingNumber other) => Value.CompareTo(other.Value);
	public string ToString(string? format, IFormatProvider? formatProvider) => Value.ToString(format, formatProvider);

	public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) =>
		Value.TryFormat(destination, out charsWritten, format, provider);

	public static SelfConvertingNumber operator +(SelfConvertingNumber value) => value;
	public static SelfConvertingNumber operator -(SelfConvertingNumber value) => value.With(-value.Value);
	public static SelfConvertingNumber operator +(SelfConvertingNumber left, SelfConvertingNumber right) => left.With(left.Value + right.Value);
	public static SelfConvertingNumber operator -(SelfConvertingNumber left, SelfConvertingNumber right) => left.With(left.Value - right.Value);
	public static SelfConvertingNumber operator *(SelfConvertingNumber left, SelfConvertingNumber right) => left.With(left.Value * right.Value);
	public static SelfConvertingNumber operator /(SelfConvertingNumber left, SelfConvertingNumber right) => left.With(left.Value / right.Value);
	public static SelfConvertingNumber operator %(SelfConvertingNumber left, SelfConvertingNumber right) => left.With(left.Value % right.Value);
	public static SelfConvertingNumber operator ++(SelfConvertingNumber value) => value.With(value.Value + 1m);
	public static SelfConvertingNumber operator --(SelfConvertingNumber value) => value.With(value.Value - 1m);
	public static bool operator <(SelfConvertingNumber left, SelfConvertingNumber right) => left.Value < right.Value;
	public static bool operator >(SelfConvertingNumber left, SelfConvertingNumber right) => left.Value > right.Value;
	public static bool operator <=(SelfConvertingNumber left, SelfConvertingNumber right) => left.Value <= right.Value;
	public static bool operator >=(SelfConvertingNumber left, SelfConvertingNumber right) => left.Value >= right.Value;
}
