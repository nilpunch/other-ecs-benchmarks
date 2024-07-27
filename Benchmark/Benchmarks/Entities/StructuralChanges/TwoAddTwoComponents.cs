using System;
using Benchmark._Context;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Benchmarks.Entities.StructuralChanges;

[ArtifactsPath(".benchmark_results/" + nameof(TwoAddTwoComponents<T>))]
[MemoryDiagnoser]
[BenchmarkCategory(Categories.PerInvocationSetup)]
#if CHECK_CACHE_MISSES
[HardwareCounters(BenchmarkDotNet.Diagnosers.HardwareCounter.CacheMisses)]
#endif
public abstract class TwoAddTwoComponents<T> : IBenchmark<T> where T : IBenchmarkContext
{
    [Params(Constants.EntityCount)] public int EntityCount { get; set; }

    public T Context { get; set; }
    private Array _entitySet;

    [IterationSetup]
    public void Setup()
    {
        Context = BenchmarkContext.Create<T>(EntityCount);
        Context?.Setup();
        _entitySet = Context?.PrepareSet(EntityCount);
        Context?.Warmup<Component1, Component2>(0);
        Context?.CreateEntities(_entitySet, 0, default(Component1), default(Component2));
        Context?.Warmup<Component3, Component4>(1);
        Context?.FinishSetup();
    }

    [IterationCleanup]
    public void Cleanup()
    {
        Context?.DeleteEntities(_entitySet);
        Context?.Cleanup();
        Context?.Dispose();
        Context = default;
    }

    [Benchmark]
    public void Run()
    {
        Context?.Lock();
        Context?.AddComponent<Component3, Component4>(_entitySet, 1);
        Context?.Commit();
    }
}