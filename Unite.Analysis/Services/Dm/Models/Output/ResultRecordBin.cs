using Unite.Essentials.Tsv.Attributes;

namespace Unite.Analysis.Services.Dm.Models.Output;

public class ResultRecordBin : ResultRecord
{
    [Column("Count")]
    public int Count { get; set; }
}
