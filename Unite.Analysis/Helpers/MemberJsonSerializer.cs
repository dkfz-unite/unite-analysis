using System.Text.Json;
using System.Text.Json.Serialization;

namespace Unite.Analysis.Helpers;

public static class MemberJsonSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumMemberConverter() }
    };

    public static string Serialize<T>(T value)
    {
       return JsonSerializer.Serialize(value, _options);
    }
}
