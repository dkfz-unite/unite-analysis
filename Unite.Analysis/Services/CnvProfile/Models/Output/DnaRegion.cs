using Unite.Data.Entities.Omics.Enums;

namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public class DnaRegion
{
    public string Id { get; set; }
    public Chromosome Chromosome { get; set; }
    public ChromosomeArm Arm { get; set; }
}