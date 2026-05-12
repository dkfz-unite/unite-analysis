using Unite.Data.Entities.Omics.Enums;

namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public class ChromosomeArm
{
    public Chromosome Chromosome { get; set; }
    public Unite.Data.Entities.Omics.Enums.ChromosomeArm Arm { get; set; }
}