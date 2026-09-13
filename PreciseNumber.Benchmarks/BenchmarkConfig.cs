// Copyright (c) 2023-2026 ktsu-dev contributors

namespace ktsu.PreciseNumber.Benchmarks;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Order;

/// <summary>
/// The configuration every benchmark in this assembly runs under.
/// </summary>
internal static class BenchmarkConfig
{
	/// <summary>
	/// Builds the configuration.
	/// </summary>
	/// <returns>The configuration to run benchmarks with.</returns>
	/// <remarks>
	/// Allocation is reported alongside time because most of the cost in this library came from
	/// allocating intermediate values rather than from the arithmetic itself, and a change that
	/// trades one for the other should be visible in the same table. Results are kept in
	/// declaration order so that a summary reads the way the source does.
	/// </remarks>
	internal static IConfig Create() =>
		ManualConfig.Create(DefaultConfig.Instance)
			.AddDiagnoser(MemoryDiagnoser.Default)
			.AddColumn(RankColumn.Arabic)
			.AddExporter(JsonExporter.Full)
			.WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared));
}
