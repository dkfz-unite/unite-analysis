using System.Text.Json.Serialization;
using Unite.Analysis.Services.Meth.Models.Criteria.Enums;

namespace Unite.Analysis.Services.Meth.Models.Criteria;

public class Options
{
    /// <summary>
    /// Pre processing type.
    /// </summary>
    [JsonPropertyName("pp")]
    public PreprocessingType PP { get; set; } = PreprocessingType.Illumina;
}
