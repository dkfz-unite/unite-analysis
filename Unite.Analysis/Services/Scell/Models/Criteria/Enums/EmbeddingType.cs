using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.Scell.Models.Criteria.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum EmbeddingType
{
    [JsonPropertyName("umap")]
    [EnumMember(Value = "umap")]
    Umap = 1,

    [JsonPropertyName("tsne")]
    [EnumMember(Value = "tsne")]
    Tsne = 2
}
