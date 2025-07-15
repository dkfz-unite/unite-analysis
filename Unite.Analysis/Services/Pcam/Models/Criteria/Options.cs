using System.Text.Json.Serialization;
using Unite.Analysis.Services.Pcam.Models.Criteria.Enums;

namespace Unite.Analysis.Services.Pcam.Models.Criteria;

public class Options
{
    /// <summary>
    /// Pre processing type.
    /// </summary>
    [JsonPropertyName("pp")]
    public PreprocessingType PP { get; set; } = PreprocessingType.Illumina;
}
