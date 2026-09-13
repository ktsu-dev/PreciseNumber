# PreciseNumber Benchmarks

A [BenchmarkDotNet](https://benchmarkdotnet.org) suite covering the operations that dominate
real use of `PreciseNumber`: building values, comparing them, arithmetic, rounding, text
conversion, and conversion to and from the primitive numeric types.

## Running

From the repository root:

```bash
# Pick benchmarks from an interactive list
dotnet run -c Release --project PreciseNumber.Benchmarks

# Run everything (slow: a full run is tens of minutes)
dotnet run -c Release --project PreciseNumber.Benchmarks -- --filter '*'

# Run one class, or one method
dotnet run -c Release --project PreciseNumber.Benchmarks -- --filter '*ArithmeticBenchmarks*'
dotnet run -c Release --project PreciseNumber.Benchmarks -- --filter '*.Multiply'

# Fewer iterations, for a quick read while iterating on a change
dotnet run -c Release --project PreciseNumber.Benchmarks -- --filter '*Comparison*' --job short
```

Release configuration is required — BenchmarkDotNet refuses to measure a debug build.

Results land in `BenchmarkDotNet.Artifacts/results/` as GitHub-flavoured Markdown, CSV, HTML and
JSON. That directory is gitignored; copy a table into a pull request when a change moves the
numbers.

## What is measured

| Class | Covers |
| --- | --- |
| `ConstructionBenchmarks` | The constructor: counting significant digits and stripping trailing zeros |
| `ComparisonBenchmarks` | Equality, ordering, `CompareTo`, `Max`, `GetHashCode` |
| `ArithmeticBenchmarks` | `+`, `-`, `*`, `/`, `%`, negation, squaring |
| `PowBenchmarks` | Raising to an integer power |
| `RoundingBenchmarks` | `Round`, `ReduceSignificance`, `Clamp` |
| `TextBenchmarks` | `ToString`, `TryFormat`, `Parse` |
| `ConversionBenchmarks` | To and from `int`, `long`, `double`, `float`, `decimal` |

Most classes are parameterised by `Digits` — 8, 30 and 200 significant digits. This is the axis
that matters: a `PreciseNumber` holds its digits in a `BigInteger`, so an operation that touches
each digit separately looks fine at 8 digits and falls apart at 200. Reading a table across the
`Digits` column, rather than down a single value of it, is what catches that.

`ComparisonBenchmarks` and `ArithmeticBenchmarks` also separate operands whose exponents are far
apart from operands in the same decade, because aligning two exponents is its own cost, distinct
from the size of the operands.

## Reading the results

Allocation is reported next to time. Both matter here, and they trade against each other: every
operation returns a new instance, so a change that avoids an intermediate value shows up in the
`Allocated` column before it shows up in `Mean`. A comparison that allocates at all is a
regression — none of them should.

`Divide` is the one row whose cost is not driven by the operands alone. It produces a terminating
quotient exactly and a repeating one to a chosen precision, so the `Digits` column moves it twice
over: wider operands are more work to divide, and they also raise the precision the quotient is
taken to. A number that looks expensive at 200 digits is doing proportionally more work, not doing
the same work badly.

Benchmark operands come from a fixed digit pattern rather than a random source, so two runs on
the same machine measure the same work. Numbers are still only comparable within a single run on
a single machine; a cloud CI runner in particular is too noisy to compare against a previous run
there.
