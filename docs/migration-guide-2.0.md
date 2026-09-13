# Migrating from PreciseNumber 1.x to 2.0

PreciseNumber 2.0 makes `PreciseNumber` a value type and makes generic math conversions work. Arithmetic, parsing, formatting, equality, and hashing produce the same results as before. What changes is how the type behaves around `null`, inheritance, and conversion.

## Quick checklist

1. Remove `null` checks and `null` assignments for `PreciseNumber` values. A `PreciseNumber?` now means `Nullable<PreciseNumber>`.
2. Replace any type that derives from `PreciseNumber` with one that holds a `PreciseNumber`.
3. Replace calls to `As<TOutput>()` and the copy constructor.
4. Check the `TryParse` failure path, which now yields zero instead of `null`.
5. Check calls to `To<T>()` that convert a fractional value to an integer type, which now truncate instead of returning zero.

## Why

A `record` class allocated an object for every result, and every operator produced one. As a `readonly record struct`, the number lives inline in its variable, field, or array element, and the only heap allocation left is the `BigInteger` digit array. A significand that fits in an `int` doesn't need one, so adding, subtracting, multiplying, and comparing small values allocates nothing. Division still allocates when the quotient repeats, because it computes at least 50 digits.

It also lets `PreciseNumber` satisfy `where T : struct, INumber<T>`, which is the constraint generic numeric libraries such as `ktsu.Semantics.Quantities` put on their storage type. Before 2.0, `CreateChecked`, `CreateSaturating`, and `CreateTruncating` threw `NotSupportedException` in both directions, so that code couldn't convert a unit factor or take a square root through `double`.

## 1. PreciseNumber is a value type

`default(PreciseNumber)` is zero. It has the same `Exponent`, `Significand`, `SignificantDigits`, and hash code as `PreciseNumber.Zero`, so an uninitialized field or array element is a valid number.

```csharp
// Was:
PreciseNumber? total = null;
if (total is null) { total = PreciseNumber.Zero; }

// Now:
PreciseNumber total = default; // zero
```

A `PreciseNumber?` still compiles, but it's now a `Nullable<PreciseNumber>`. Code that used `?` to mean "might be null" should either drop the `?` or use `Nullable<PreciseNumber>` deliberately.

## 2. No inheritance

A struct can't be inherited, so these are gone:

| Removed | Replacement |
|---|---|
| Deriving from `PreciseNumber` | Hold a `PreciseNumber` field and convert to it |
| `PreciseNumber(PreciseNumber original)` copy constructor | Assign the value. Copying a struct copies it. |
| `As<TOutput>()` | Construct the target type from the value directly |

These members were `protected internal` for derived types and are now `internal`:

- `PreciseNumber(int exponent, BigInteger significand)` and `PreciseNumber(int exponent, BigInteger significand, bool sanitize)`
- `LowestDecimalDigits`, `LowestSignificantDigits`, and `CountDecimalDigits`
- `MakeCommonized` and `MakeCommonizedWithExponent`
- `AssertExponentsMatch` and `InvariantCulture`

`ktsu.SignificantNumber` derives from `PreciseNumber` 1.x and uses several of them. It keeps working against 1.x until it moves to 2.0, which means holding a `PreciseNumber` instead of deriving from one.

## 3. Signatures that accepted null

| Member | 1.x | 2.0 |
|---|---|---|
| `Equals` | `Equals(PreciseNumber? other)` returned `false` for `null` | `Equals(PreciseNumber other)` |
| `CompareTo` | `CompareTo(PreciseNumber? other)` returned `1` for `null` | `CompareTo(PreciseNumber other)` |
| `CompareTo(object?)` | Threw `NotSupportedException` for `null` | Returns `1` for `null`, and still throws for a non-`PreciseNumber` object |
| `TryParse` (three overloads) | `out PreciseNumber? result`, `null` on failure | `out PreciseNumber result`, zero on failure |

```csharp
// Was:
if (PreciseNumber.TryParse(text, CultureInfo.InvariantCulture, out PreciseNumber? parsed)) { Use(parsed); }

// Now:
if (PreciseNumber.TryParse(text, CultureInfo.InvariantCulture, out PreciseNumber parsed)) { Use(parsed); }
```

## 4. Generic math conversions work

The six `TryConvertFrom*` and `TryConvertTo*` methods are implemented for every built-in numeric type (`sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `Int128`, `UInt128`, `nint`, `nuint`, `char`, `Half`, `float`, `double`, and `decimal`) and for `BigInteger`. They return `false` for any other type instead of throwing.

```csharp
static T ToMeters<T>(T feet) where T : INumber<T> => feet * T.CreateChecked(0.3048);

PreciseNumber meters = ToMeters(10.ToPreciseNumber()); // exactly 3.048
double asDouble = double.CreateChecked(meters);        // 3.048
```

Converting to `PreciseNumber`:

| Source | Checked | Saturating | Truncating |
|---|---|---|---|
| Integers, `BigInteger`, and `decimal` | Exact | Exact | Exact |
| `double`, `float`, and `Half` | Through decimal text, so `0.3048` is exactly 0.3048 | Same | Same |
| NaN | Throws `OverflowException` | Zero | Zero |
| Infinity | Throws `OverflowException` | Throws `OverflowException` | Throws `OverflowException` |

NaN and infinity follow `BigInteger`, the other built-in numeric type with neither NaN nor a largest value. A `double` keeps 16 significant digits and a `float` 8, which is the same rounding `ToPreciseNumber()` has always applied.

Converting from `PreciseNumber`:

| Destination | Checked | Saturating | Truncating |
|---|---|---|---|
| Integer types | Integral part, truncated toward zero. Throws `OverflowException` when out of range. | Clamps to the minimum or maximum | Wraps, keeping the low bits, as `BigInteger` does |
| `BigInteger` | Integral part, truncated toward zero | Same | Same |
| `double`, `float`, and `Half` | Correctly rounded, overflowing to infinity | Same | Same |
| `decimal` | Rounded to the digits `decimal` holds. Throws `OverflowException` when out of range. | Clamps to `decimal.MinValue` or `decimal.MaxValue` | Clamps, as `BigInteger` does |

## 5. To<T>() uses the same conversions

`To<T>()` used to multiply the significand by `Math.Pow(10, exponent)` converted to the target type. For an integer target and a negative exponent, the power of ten converted to zero, so `12.9` became `0`. For `double`, the result could miss the nearest representable value.

It now uses the checked conversion described earlier. `12.9.ToPreciseNumber().To<int>()` is `12`, and `To<double>()` is correctly rounded however many digits the number has. A type that isn't built in still goes through the old calculation.
