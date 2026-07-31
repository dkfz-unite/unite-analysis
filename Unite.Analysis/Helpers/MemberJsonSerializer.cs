using System.Text.Json;
using System.Text.Json.Serialization;

namespace Unite.Analysis.Helpers;

public static class MemberJsonSerializer
{
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumMemberConverter() }
    };

    public static string Serialize<T>(T value)
    {
       return JsonSerializer.Serialize(value, Options);
    }

    public static void Serialize<T>(string path, T value)
    {
        var json = Serialize(value);
        
        File.WriteAllText(path, json);
    }
}
