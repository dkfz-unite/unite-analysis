using Unite.Essentials.Tsv.Attributes;

namespace Unite.Analysis.Services.Pcam.Models.Input;

public class SampleMetadata : Services.SampleMetadata
{
    [Column("path")]
    public string Path { get; set; }
}
