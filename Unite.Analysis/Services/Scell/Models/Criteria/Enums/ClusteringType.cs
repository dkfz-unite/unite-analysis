using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.Scell.Models.Criteria.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum ClusteringType
{
    [JsonPropertyName("louvain")]
    [EnumMember(Value = "louvain")]
    Louvain = 1,

    [JsonPropertyName("leiden")]
    [EnumMember(Value = "leiden")]
    Leiden = 2
}
