using System;
using Benchmark._Context;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Benchmarks.Entities.AddComponent;

[ArtifactsPath(".benchmark_results/" + nameof(Add2RandomComponents<T, TE>))]
[MemoryDiagnoser]
#if CHECK_CACHE_MISSES
[HardwareCounters(BenchmarkDotNet.Diagnosers.HardwareCounter.CacheMisses)]
#endif
public abstract class Add2RandomComponents<T, TE> : IBenchmark<T, TE> where T : IBenchmarkContext<TE>
{
    [Params(Constants.EntityCount)] public int EntityCount { get; set; }
    public T Context { get; set; }
    private TE[] _entitySet;
    private TE[] _tmp;

    [GlobalSetup]
    public void GlobalSetup()
    {
        Context = BenchmarkContext.Create<T>(EntityCount);
        Context.Setup();
        _entitySet =  Context.PrepareSet(EntityCount);
        _tmp = Context.PrepareSet(1);
        Context.Warmup<Component1, Component2>(0);
        Context.Warmup<Component2, Component3>(1);
        Context.Warmup<Component3, Component4>(2);
        Context.Warmup<Component4, Component1>(3);
        Context.FinishSetup();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        Context.CreateEntities(_entitySet);
    }

    [Benchmark]
    public void Run()
    {
        for (var _i = 0; _i < EntityCount; _i++) {
            _tmp[0] = _entitySet[_i];
            switch (ArrayExtensions.Rnd.Next() % 4)
            {
                case 0:
                    Context.AddComponent<Component1, Component2>(_tmp, 0, default(Component1), default(Component2));
                    break;
                case 1:
                    Context.AddComponent<Component2, Component3>(_tmp, 1, default(Component2), default(Component3));
                    break;
                case 2:
                    Context.AddComponent<Component3, Component4>(_tmp, 2, default(Component3), default(Component4));
                    break;
                case 3:
                    Context.AddComponent<Component4, Component1>(_tmp, 3, default(Component4), default(Component1));
                    break;
            }
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        Context.DeleteEntities(_entitySet);
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        Context.Cleanup();
        Context.Dispose();
        Context = default;
    }
}
