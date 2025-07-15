using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.Dm.Models.Criteria.Enums;

public enum PreprocessingType
{
    [JsonPropertyName("Illumina")]
    [EnumMember(Value = "Illumina")]
    Illumina = 1,

    [JsonPropertyName("SWAN")]
    [EnumMember(Value = "SWAN")]
    SWAN = 2,

    [JsonPropertyName("Quantile")]
    [EnumMember(Value = "Quantile")]
    Quantile = 3,

    [JsonPropertyName("Noob")]
    [EnumMember(Value = "Noob")]
    Noob = 4,

    [JsonPropertyName("Raw")]
    [EnumMember(Value = "Raw")]
    Raw = 5
}
