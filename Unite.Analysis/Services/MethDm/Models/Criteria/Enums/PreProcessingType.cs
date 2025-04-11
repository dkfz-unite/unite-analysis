using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.MethDm.Models.Criteria.Enums;

public enum PreprocessingType
{
    [JsonPropertyName("preprocessIllumina")]
    [EnumMember(Value = "preprocessIllumina")]
    Illumina = 1,

    [JsonPropertyName("preprocessSWAN")]
    [EnumMember(Value = "preprocessSWAN")]
    SWAN = 2,

    [JsonPropertyName("preprocessQuantile")]
    [EnumMember(Value = "preprocessQuantile")]
    Quantile = 3,

    [JsonPropertyName("preprocessNoob")]
    [EnumMember(Value = "preprocessNoob")]
    Noob = 4,

    [JsonPropertyName("preprocessRaw")]
    [EnumMember(Value = "preprocessRaw")]
    Raw = 5
}
