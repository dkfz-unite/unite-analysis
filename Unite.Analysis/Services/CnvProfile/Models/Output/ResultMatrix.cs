namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public class ResultMatrix
{
    public DnaRegion[] DnaRegions { get; set; }
    public Sample[] Samples { get; set; }
    public IList<Observation> Observations { get; set; }
}