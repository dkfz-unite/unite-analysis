using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.SCell.Models.Enums;

public enum PreprocessingType
{
    [JsonPropertyName("default")]
    [EnumMember(Value = "default")]
    Default = 1,

    [JsonPropertyName("seurat")]
    [EnumMember(Value = "seurat")]
    Seurat = 2,

    [JsonPropertyName("zheng17")]
    [EnumMember(Value = "zheng17")]
    Zheng17 = 3
}
