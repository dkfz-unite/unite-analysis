namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public record GridSample
{
    public int Id { get; set; }
    public int DonorId { get; set; }
    public int SpecimenId { get; set; }
    public Dictionary<string, string> Tracks { get; set; }

    public GridSample(int id, int donorId, int specimenId, Dictionary<string, string> tracks)
    {
        Id = id;
        DonorId = donorId;
        SpecimenId = specimenId;
        Tracks = tracks;
    }
}
