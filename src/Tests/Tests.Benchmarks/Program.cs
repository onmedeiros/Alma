using BenchmarkDotNet.Running;
using Tests.Benchmarks;

Console.WriteLine("Running benchmarks!");

BenchmarkRunner.Run<FlowBenchmark>();

Console.WriteLine("Benchmark finished!");