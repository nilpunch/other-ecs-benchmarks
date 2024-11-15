using System;
using Benchmark._Context;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Benchmarks.Entities.CreateEntity;

[ArtifactsPath(".benchmark_results/" + nameof(CreateEntityWith1RandomComponent<T>))]
[MemoryDiagnoser]

#if CHECK_CACHE_MISSES
[HardwareCounters(BenchmarkDotNet.Diagnosers.HardwareCounter.CacheMisses)]
#endif
public abstract class CreateEntityWith1RandomComponent<T> : IBenchmark<T> where T : IBenchmarkContext
{
    [Params(Constants.EntityCount)] public int EntityCount { get; set; }
    [Params(1, 4, 32)] public int ChunkSize { get; set; }
    public T Context { get; set; }
    private Array[] _entitySets;
    

    [GlobalSetup]
    public void GlobalSetup()
    {
        Context = BenchmarkContext.Create<T>(EntityCount);
        Context.Setup();
        var setsCount = EntityCount / ChunkSize + 1;
        _entitySets = new Array[setsCount];
        for (var i = 0; i < setsCount; i++)
            _entitySets[i] = Context.PrepareSet(ChunkSize);
        Context.Warmup<Component1>(0);
        Context.Warmup<Component2>(1);
        Context.Warmup<Component3>(2);
        Context.Warmup<Component4>(3);
        Context.FinishSetup();
    }

    [Benchmark]
    public void Run()
    {
        for (var i = 0; i < _entitySets.Length; i++)
            switch (ArrayExtensions.Rnd.Next() % 4)
            {
                case 0:
                    Context.CreateEntities<Component1>(_entitySets[i], 0);
                    break;
                case 1:
                    Context.CreateEntities<Component2>(_entitySets[i], 1);
                    break;
                case 2:
                    Context.CreateEntities<Component3>(_entitySets[i], 2);
                    break;
                case 3:
                    Context.CreateEntities<Component4>(_entitySets[i], 3);
                    break;
            }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        var setsCount = EntityCount / ChunkSize + 1;

        for (var i = 0; i < setsCount; i++)
            Context.DeleteEntities(_entitySets[i]);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Context.Cleanup();
        Context.Dispose();
        Context = default;
    }
}