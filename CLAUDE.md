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

- **PreciseNumber** (`PreciseNumber/PreciseNumber.cs`): The main numeric type implementing `INumber<PreciseNumber>`. Stores values using:
  - `Significand`: A `BigInteger` containing all significant digits
  - `Exponent`: An `int` determining the decimal place
  - `SignificantDigits`: Count of significant digits

- **PreciseNumberExtensions** (`PreciseNumber/PreciseNumberExtensions.cs`): Extension methods providing `ToPreciseNumber<T>()` for converting any `INumber<T>` to PreciseNumber

### Key Design Patterns

- Factory methods `CreateFromInteger<T>()` and `CreateFromFloatingPoint<T>()` handle type-specific conversion logic
- Addition, subtraction and modulus align exponents before calculating; multiplication and division work on the significands directly
- `Divide` is exact when the quotient terminates, and otherwise rounds to a precision that never falls below the wider operand or `MinimumDivisionPrecision`. `Exp` and non-integer `Pow` still route through `double`
- The `sanitize` constructor parameter controls whether trailing zeros are removed (default: true)
- Constants (`Zero`, `One`, `Pi`, `E`, `Tau`) are pre-computed static instances

### Test Structure

Tests use MSTest framework in `PreciseNumber.Test/PreciseNumberTests.cs`. The test project targets only .NET 10.0 while the main library multi-targets net7.0, net8.0, net9.0, and net10.0.

### Benchmarks

`PreciseNumber.Benchmarks` is a BenchmarkDotNet suite, one class per area (construction,
comparison, arithmetic, pow, rounding, text, conversion). The library exposes its internals to it
so construction can be measured directly.

Most classes are parameterised by `Digits` (8, 30, 200). That axis is the point: digits live in a
`BigInteger`, so anything that touches them one at a time looks fine at 8 digits and collapses at
200. Read results across the `Digits` column, not down one value of it.

Allocation is reported alongside time and matters just as much — every operation returns a new
instance, so avoiding an intermediate shows up in `Allocated` before it shows up in `Mean`.
Comparisons should allocate nothing at all.

Run the relevant benchmarks before and after any change to the library's internals. See
`PreciseNumber.Benchmarks/README.md` for details.
