using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Unite.Analysis.Web.Configuration.Extensions;

public static class JsonOptionsExtensions
{
    public static void AddJsonOptions(this JsonOptions options)
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumMemberConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }
}
