# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ktsu.PreciseNumber is a high-precision numeric type for .NET that provides arbitrary precision arithmetic. It combines the scale benefits of scientific notation with the precision of `BigInteger`, storing values internally as `significand × 10^exponent`.

## Build Commands

```bash
dotnet build                                           # Build the solution
dotnet test                                            # Run all tests
dotnet test --filter "FullyQualifiedName~TestName"     # Run specific test

# Benchmarks (Release only; BenchmarkDotNet refuses to measure a debug build)
dotnet run -c Release --project PreciseNumber.Benchmarks                        # Pick from a list
dotnet run -c Release --project PreciseNumber.Benchmarks -- --filter '*Compar*' # One class
dotnet run -c Release --project PreciseNumber.Benchmarks -- --filter '*' --job short
```

## Architecture

### Core Types

- **PreciseNumber** (`PreciseNumber/PreciseNumber.cs`): The main numeric type, a `readonly partial record struct` implementing `INumber<PreciseNumber>`. Its `default` is `Zero`, which `PreciseNumberValueTypeTests` pins. Stores values using:
  - `Significand`: A `BigInteger` containing all significant digits
  - `Exponent`: An `int` determining the decimal place
  - `SignificantDigits`: Count of significant digits

- **Generic conversions** (`PreciseNumber/PreciseNumber.Conversions.cs`): The `TryConvertFrom*` and `TryConvertTo*` members behind `CreateChecked`, `CreateSaturating`, and `CreateTruncating`, for every built-in numeric type and `BigInteger`. `To<T>()` uses them too

- **PreciseNumberExtensions** (`PreciseNumber/PreciseNumberExtensions.cs`): Extension methods providing `ToPreciseNumber<T>()` for converting any `INumber<T>` to PreciseNumber

### Key Design Patterns

- Factory methods `CreateFromInteger<T>()` and `CreateFromFloatingPoint<T>()` handle type-specific conversion logic
- Addition, subtraction and modulus align exponents before calculating; multiplication and division work on the significands directly
- `Divide` is exact when the quotient terminates, and otherwise rounds to a precision that never falls below the wider operand or `MinimumDivisionPrecision`
- Roots (`PreciseNumber/PreciseNumber.Roots.cs`, satisfying `IRootFunctions<PreciseNumber>`) follow `Divide`'s precision rule and do not route through `double`. Each scales the significand by a power of ten until the degree divides the exponent, then takes an integer Newton root of the significand, so an exact root stops on the exact answer rather than on a tolerance and no seed has to survive a value outside `double`'s range
- Exponentials, logarithms and powers (`PreciseNumber/PreciseNumber.Exponentials.cs`, satisfying `IExponentialFunctions`, `ILogarithmicFunctions` and `IPowerFunctions`) follow the same precision rule and do not route through `double` either. `ln(m · 10^k)` is `ln m + k · ln 10` against the stored `Ln10`, with the mantissa centred on `[1/√10, √10)` and fed to the atanh series; `exp(v)` factors out `10^round(v / ln 10)` as an exponent shift and halves what is left before a Taylor sum. Nothing here is a free-standing decision: `Exp10`/`Log10` must not route through the natural log, because the exponent is the whole answer for a power of ten, and the `…M1`/`…P1` variants must not be computed as `Exp(x) - 1`/`Log(1 + x)`, because that cancels away the precision near zero they exist to keep
- A fractional `Pow` is `exp(y · ln x)`, carried wider by the integer digits of `y · ln x` because `Exp`'s range reduction consumes them. The integer path stays exponentiation by squaring and is exact; tests pin that exactness rather than a tolerance
- The `sanitize` constructor parameter controls whether trailing zeros are removed (default: true)
- Constants (`Zero`, `One`, `Pi`, `E`, `Tau`) are pre-computed static instances
- `ConstantPrecision` is a ceiling, not just a width. `PiTo`/`ETo`/`Ln2To`/`Ln10To` serve at most that many digits and return the capped constant rather than failing, which is deliberate and pinned by `TestConstantAccessorsReturnTheWholeConstantWhenAskedForMore` — so a caller that *needs* what it asked for has to check, because the accessor will not. The circular functions are where this bites: reducing modulo `π/2` spends one digit of π per integer digit of the argument, so only about `ConstantPrecision - integerDigits` are left for the answer. `RequireReducibleArgument` enforces exactly that sum and nothing wider. Do not tighten it to `reductionDigits`: that pads the requirement with `TrigonometricGuardDigits` twice over as margin, margin is allowed to be unavailable, and bounding it would refuse `Sin(1000000, 130)` — a call whose every reported digit is correct. The half-turn family (`SinPi` and the rest) has no such ceiling, because it reduces before it multiplies, and `Log` has none either, because it carries the magnitude in the decimal exponent rather than cancelling it against a constant
- As a value type it can't be null or inherited. Don't add null checks for `PreciseNumber` parameters, and don't reintroduce `protected` members
- Conversions to integer types go through `BigInteger`, so range checks, clamping, and wrapping follow its conventions. Conversions to `double`, `float`, `Half`, and `decimal` render normalized scientific notation (`d.ddd…E±n`) and parse it, because the runtime parsers round correctly, with Clinger's fast path for small values. Keep one digit before the point. The .NET 7 and 8 parsers clamp an exponent above 1000 and still offset it by every digit ahead of the point, so a long significand rendered as an integer parses as zero there. Conversions from `double`, `float`, and `Half` use the shortest text that round-trips (`"R"`). NaN and infinity coming in follow `BigInteger` too

### Test Structure

Tests use MSTest. `PreciseNumber.Test/PreciseNumberTests.cs` covers arithmetic, parsing, and formatting, `PreciseNumberConversionTests.cs` covers generic math conversion in every mode, `PreciseNumberRootTests.cs` pins the roots against published digits and against squaring back, `PreciseNumberExponentialTests.cs` does the same for the exponentials and logarithms and additionally pins the cases a `double` fallback cannot reach — fifty published digits of a fractional power, and `ExpM1`/`LogP1` of `1e-30` not collapsing to zero — and `PreciseNumberValueTypeTests.cs` pins `default` as zero and asserts that small-value addition, subtraction, multiplication, and comparison allocate nothing. The test project targets only .NET 10.0 while the main library multi-targets net7.0, net8.0, net9.0, and net10.0.

### Benchmarks

`PreciseNumber.Benchmarks` is a BenchmarkDotNet suite, one class per area (construction,
comparison, arithmetic, pow, roots, rounding, text, conversion). The library exposes its internals to it
so construction can be measured directly.

Most classes are parameterised by `Digits` (8, 30, 200). That axis is the point: digits live in a
`BigInteger`, so anything that touches them one at a time looks fine at 8 digits and collapses at
200. Read results across the `Digits` column, not down one value of it.

Allocation is reported alongside time and matters just as much. The number is a value type, so the
only allocations are `BigInteger` digit arrays, and avoiding an intermediate shows up in `Allocated`
before it shows up in `Mean`. Comparison, addition, subtraction, and multiplication should allocate
nothing when the operands and every intermediate and final significand fit in an `int`. Exponent
alignment counts, so `1 + 0.0000000001` allocates because it scales 1 by 10^10, and `99999 * 99999`
allocates because its product is 9,999,800,001.

Run the relevant benchmarks before and after any change to the library's internals. See
`PreciseNumber.Benchmarks/README.md` for details.
