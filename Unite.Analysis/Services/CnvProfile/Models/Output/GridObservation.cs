using Unite.Data.Entities.Omics.Analysis.Dna.Cnv.Enums;

namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public record GridObservation
{
    public int SampleId { get; set; }
    public string RegionId { get; set; }
    public CnvType Event { get; set; }


    public GridObservation(int sampleId, string regionId, CnvType cnvType)
    {
        SampleId = sampleId;
        RegionId = regionId;
        Event = cnvType;
    }
}
