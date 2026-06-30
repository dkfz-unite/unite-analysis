namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public record Observation
{
    public int SampleId { get; set; }
    public string DnaRegionId { get; set; }
    public Event Event { get; set; }
}