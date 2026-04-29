using Unite.Data.Entities.Omics.Analysis.Dna.Cnv;

namespace Unite.Analysis.Services.CnvProfile.Models;

public class AnalysisContext : SamplesContext
{
    public AnalysisContext(SampleType sampleType) : base(sampleType)
    {
    }

    public Dictionary<int, Profile> CnvProfiles { get; set; }
}