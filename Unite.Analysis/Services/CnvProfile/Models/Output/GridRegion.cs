using Unite.Data.Entities.Omics.Enums;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public record GridRegion
{
    public string Id => $"Chr{Chromosome.ToDefinitionString()}{Arm.ToDefinitionString()}";
    public Chromosome Chromosome { get; set; }
    public ChromosomeArm Arm { get; set; }


    public GridRegion(Chromosome chromosome, ChromosomeArm arm)
    {
        Chromosome = chromosome;
        Arm = arm;
    }
}