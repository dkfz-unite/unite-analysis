using System.Text.Json.Serialization;
using static Unite.Data.Entities.Omics.Analysis.Dna.Effect;

namespace Unite.Analysis.Services.Gaf.Models.Criteria;

public class Options
{
    /// <summary>
    /// Number of donors.
    /// </summary>
    [JsonPropertyName("donors")]
    public int Donors { get; set; } = 100;

    /// <summary>
    /// Number of genes.
    /// </summary>
    [JsonPropertyName("genes")]
    public int Genes { get; set; } = 30;

    [JsonPropertyName("sm")]
    public string[] Sm { get; set; } = [Impacts.High, Impacts.Moderate]; 
}
