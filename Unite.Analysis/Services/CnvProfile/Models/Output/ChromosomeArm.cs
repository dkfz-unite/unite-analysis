using Unite.Data.Entities.Omics.Enums;

namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public class ChromosomeArm
{
    public Chromosome Chromosome { get; set; }
    public ChromosomeArm Arm { get; set; }
}