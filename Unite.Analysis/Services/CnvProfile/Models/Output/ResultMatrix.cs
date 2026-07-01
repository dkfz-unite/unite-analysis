namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public record ResultMatrix
{
    public DnaRegion[] DnaRegions { get; set; }
    public Sample[] Samples { get; set; }
    public IList<Observation> Observations { get; set; }
}