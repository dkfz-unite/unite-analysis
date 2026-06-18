using System.Text.Json.Serialization;
using Unite.Analysis.Services.Cedp.Models.Criteria.Enums;

namespace Unite.Analysis.Services.Cedp.Models.Criteria;

public class Options : Dep.Models.Criteria.Options
{
    [JsonPropertyName("feature_type")]
    public FeatureType FeatureType { get; set; } = FeatureType.Gene;

    [JsonPropertyName("feature")]
    public string Feature { get; set; } = null;

    [JsonPropertyName("condition_property")]
    public string ConditionProperty { get; set; } = null;
}
