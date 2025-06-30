using Unite.Analysis.Services.Dm.Models.Data;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Dm;

public class ResultMapper
{
    public static ClassMap<Resultdata> Map(Resultdata[] entries)
    {
        return new ClassMap<Resultdata>()
            .Map(entry => entry.Log2FoldChange, "logFc")
            .Map(entry => entry.PValueAdjusted, "adj.P.Val")
            .Map(entry => entry.Count, "Count")
            .Map(entry => entry.CpgId, "CpgId")
            .Map(entry => entry.RegulatoryFeatureName, "Regulatory_Feature_Name")
            .Map(entry => entry.Phantom4Enhancers, "Phantom4_Enhancers")
            .Map(entry => entry.Phantom5Enhancers, "Phantom5_Enhancers")
            .Map(entry => entry.UcscRefGeneName, "UCSC_RefGene_Name");
    }
}