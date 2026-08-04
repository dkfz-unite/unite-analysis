namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public record Sample
{
    public int Id { get; set; }
    public string TumorType { get; set; }
    public int DonorId { get; set; }
}