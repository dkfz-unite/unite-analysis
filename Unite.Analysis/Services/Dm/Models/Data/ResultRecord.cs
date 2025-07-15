using Unite.Essentials.Tsv.Attributes;

namespace Unite.Analysis.Services.Dm.Models.Data;

public class ResultRecord
{
    [Column("CpgId")]
    public string CpgId { get; set; }

    [Column("logFc")]
    public double Log2FoldChange { get; set; }

    [Column("adj.P.Val")]
    public double PValueAdjusted { get; set; }

    [Column("Regulatory_Feature_Name")]
    public string RegulatoryFeatureName { get; set; }

    [Column("Phantom4_Enhancers")]
    public string Phantom4Enhancers { get; set; }

    [Column("Phantom5_Enhancers")]
    public string Phantom5Enhancers { get; set; }

    [Column("UCSC_RefGene_Name")]
    public string UcscRefGeneName { get; set; }
}
