using BenchmarkDotNet.Running;

namespace JsonDynamic;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<JsonBenchmark>();
    }
}