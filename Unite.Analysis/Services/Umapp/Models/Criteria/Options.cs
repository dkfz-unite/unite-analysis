using System.Text.Json.Serialization;
using Unite.Analysis.Services.Umapp.Models.Criteria.Enums;

namespace Unite.Analysis.Services.Umapp.Models.Criteria;

public class Options : Dep.Models.Criteria.Options
{
    [JsonPropertyName("class_property")]
    public string ClassProperty { get; set; } = null;

    [JsonPropertyName("feature_selection_method")]
    public FeatureSelectionMethod FeatureSelectionMethod { get; set; } = FeatureSelectionMethod.Variance;

    [JsonPropertyName("feature_selection_n_features")]
    public int FeatureSelectionFeaturesNumber { get; set; } = 1000;

    [JsonPropertyName("umap_n_neighbors")]
    public int UmapNeighborsNumber { get; set; } = 15;

    [JsonPropertyName("umap_metric")]
    public UmapMetric UmapMetric { get; set; } = UmapMetric.Euclidean;

    [JsonPropertyName("umap_random_state")]
    public int? UmapRandomState { get; set; } = null;

    [JsonPropertyName("umap_min_dist")]
    public double UmapMinimalDistance { get; set; } = 0.1;

    [JsonPropertyName("umap_n_principal_components")]
    public int UmapPrincipalComponentsNumber { get; set; } = 50;
}
