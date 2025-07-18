using Unite.Essentials.Tsv.Attributes;

namespace Unite.Analysis.Services.Dm.Models.Input;

public class MetadataSample
{
    [Column("sample_id")]
    public string SampleId { get; set; }

    [Column("path")]
    public string Path { get; set; }

    [Column("condition")]
    public string Condition { get; set; }
}
