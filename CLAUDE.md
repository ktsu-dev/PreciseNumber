# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ktsu.PreciseNumber is a high-precision numeric type for .NET that provides arbitrary precision arithmetic. It combines the scale benefits of scientific notation with the precision of `BigInteger`, storing values internally as `significand × 10^exponent`.

## Build Commands

```bash
dotnet build                                           # Build the solution
dotnet test                                            # Run all tests
dotnet test --filter "FullyQualifiedName~TestName"     # Run specific test
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
- Arithmetic operations use `MakeCommonized()` to align exponents before calculation
- The `sanitize` constructor parameter controls whether trailing zeros are removed (default: true)
- Constants (`Zero`, `One`, `Pi`, `E`, `Tau`) are pre-computed static instances

### Test Structure

Tests use MSTest framework in `PreciseNumber.Test/PreciseNumberTests.cs`. The test project targets only .NET 9.0 while the main library multi-targets net7.0, net8.0, and net9.0.
