using System.Text.Json.Serialization;
using Unite.Data.Entities.Omics;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Services.Gaf.Models.Output;

public class GeneRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
    [JsonPropertyName("biotype")]
    public string Biotype { get; set; }
    [JsonPropertyName("chromosome")]
    public string Chromosome { get; set; }
    [JsonPropertyName("strand")]
    public bool? Strand { get; set; }


    public GeneRecord(Gene gene)
    {
        Id = gene.Id.ToString();
        Symbol = gene.Symbol;
        Biotype = gene.Biotype;
        Chromosome = gene.ChromosomeId.ToDefinitionString();
        Strand = gene.Strand;
    }
}
