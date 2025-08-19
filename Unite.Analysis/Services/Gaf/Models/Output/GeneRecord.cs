using Unite.Data.Entities.Omics;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Services.Gaf.Models.Output;

public class GeneRecord
{
    public string Id { get; set; }
    public string Symbol { get; set; }
    public string Biotype { get; set; }
    public string Chromosome { get; set; }
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
