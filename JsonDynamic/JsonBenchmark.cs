using BenchmarkDotNet.Attributes;

namespace JsonDynamic;

public class JsonBenchmark
{
    private readonly string _json;

    public JsonBenchmark()
    {
        _json = File.ReadAllText("large_test.json");
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
    public void ParseNewtonsoft()
    {
        var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(_json);
        
        //DisplayObj(obj);
    }
    
    [Benchmark]
    public void ParseSystemTextJson()
    {
        var obj = DynamicJson.Parse(_json);
        
        //DisplayObj(obj);
    }
}