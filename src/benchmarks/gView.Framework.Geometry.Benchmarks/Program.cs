using BenchmarkDotNet.Running;

// Classic gView geometry against SpatialNET (ADR 0002, phase 2). Every benchmark runs once with the switch of
// its area off (classic) and once on (SpatialNET), see the UseSpatialNet parameter.
//
//   dotnet run -c Release --project src/benchmarks/gView.Framework.Geometry.Benchmarks -- --filter *
//   dotnet run -c Release --project src/benchmarks/gView.Framework.Geometry.Benchmarks -- --filter *Clip* --job short
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
