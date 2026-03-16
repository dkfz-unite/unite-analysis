using Unite.Essentials.Tsv.Attributes;

namespace Unite.Analysis.Services.Dep.Models.Input;

public class MetadataEntry
{
    public const string FileName = "metadata.tsv";

    [Column("sample")]
    public int Sample { get; set; }

    [Column("condition")]
    public string Condition { get; set; }

    [Column("batch")]
    public string Batch { get; set; }

    [Column("donor")]
    public string Donor { get; set; }

    [Column("specimen")]
    public string Specimen { get; set; }

    [Column("specimen_type")]
    public string SpecimenType { get; set; }
}
