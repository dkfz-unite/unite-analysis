namespace Unite.Analysis.Services.Gaf.Models.Output;

public class Records
{
    public GeneRecord[] Genes { get; set; }
    public DonorRecord[] Donors { get; set; }
    public VariantRecord[] Observations { get; set; }
}
