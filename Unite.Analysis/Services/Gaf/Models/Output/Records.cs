using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.Gaf.Models.Output;

public class Records
{
    [JsonPropertyName("genes")]
    public GeneRecord[] Genes { get; set; }
    [JsonPropertyName("donors")]
    public DonorRecord[] Donors { get; set; }
    [JsonPropertyName("observations")]
    public VariantRecord[] Observations { get; set; }
}
