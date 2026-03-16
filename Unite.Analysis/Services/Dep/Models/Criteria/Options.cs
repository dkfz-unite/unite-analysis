using System.Text.Json.Serialization;
using Unite.Analysis.Services.Dep.Models.Criteria.Enums;

namespace Unite.Analysis.Services.Dep.Models.Criteria;

public class Options
{
    [JsonPropertyName("normalization_log_offset")]
    public double NormalizationLogOffset { get; set; } = 0.1;

    [JsonPropertyName("normalization_method")]
    public NormalizationMethod NormalizationMethod { get; set; } = NormalizationMethod.Median;

    [JsonPropertyName("imputation_method")]
    public ImputationMethod ImputationMethod { get; set; } = ImputationMethod.MinDet;

    [JsonPropertyName("stratify_imputation_by_batch")]
    public bool StratifyImputationByBatch { get; set; } = false;

    [JsonPropertyName("batch_correction_method")]
    public BatchCorrectionMethod? BatchCorrectionMethod { get; set; } = null;

    [JsonPropertyName("min_non_missing_fraction")]
    public double MinNonMissingFraction { get; set; } = 0.7;

    [JsonPropertyName("require_min_fraction_one_class")]
    public bool RequireMinFractionOneClass { get; set; } = true;
}
