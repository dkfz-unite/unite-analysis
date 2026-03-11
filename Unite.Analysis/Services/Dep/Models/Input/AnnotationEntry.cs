using Unite.Data.Entities.Omics;
using Unite.Essentials.Tsv.Attributes;

namespace Unite.Analysis.Services.Dep.Models.Input;

public class AnnotationEntry
{
    [Column("id")]
    public string Id { get; set; }

    [Column("accession")]
    public string Accession { get; set; }

    [Column("symbol")]
    public string Symbol { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("source")]
    public string Source { get; set; }


    public AnnotationEntry(Protein protein)
    {
        Id = protein.StableId;
        Accession = protein.AccessionId;
        Symbol = protein.Symbol;
        Description = protein.Description;
        Source = protein.Database;
    }
}
