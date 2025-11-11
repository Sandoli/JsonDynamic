using BenchmarkDotNet.Attributes;

namespace JsonDynamic;

[MemoryDiagnoser]
public class JsonBenchmark
{
    private readonly string _largeJson;
    private readonly string _tinyJson;

    public JsonBenchmark()
    {
        _largeJson = File.ReadAllText("large_test.json");
        _tinyJson = File.ReadAllText("tiny_test.json");
    }

    public void DisplayObj(dynamic obj)
    {
        Console.WriteLine(obj.fragment.user.name);          // Alice
        Console.Write(obj.fragment.user.age);           // 30
        Console.Write(obj.fragment.user.skills[1]);     // SQL

        foreach (var skill in obj.fragment.user.skills)
            Console.Write(skill);              // C#, SQL, Docker        
    }
    
    [Benchmark]
    public void ParseLargeNewtonsoft()
    {
        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(_largeJson);
    }
    
    [Benchmark]
    public void ParseLargeSystemTextJson()
    {
        var obj = DynamicJson.Parse(_largeJson);
    }
    
    [Benchmark]
    public void ParseTinyNewtonsoft()
    {
        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(_tinyJson);
    }
    
    [Benchmark]
    public void ParseTinySystemTextJson()
    {
        var obj = DynamicJson.Parse(_tinyJson);
    }
}