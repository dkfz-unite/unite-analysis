namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public record GridData
{
    public GridSample[] Samples { get; set; }
    public GridRegion[] Regions { get; set; }
    public GridObservation[] Observations { get; set; }

    public GridData(GridSample[] samples, GridRegion[] regions, GridObservation[] observations)
    {
        Samples = samples;
        Regions = regions;
        Observations = observations;
    }
}
