namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public class Sample
{
    public int Id { get; set; }
    public string TumorType { get; set; }
    public int DonorId { get; set; }
}

public class Observation
{
    public int SampleId { get; set; }
    public int ChromosomeArmIndex { get; set; }
    public Event Event { get; set; }
}

public class Model
{
    public ChromosomeArm[] ChromosomeArms { get; set; }
    public Sample[] Samples { get; set; }
    public IList<Observation> Observations { get; set; }
}