namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public class SampleRecord
{
    public readonly Event[] Events;

    public SampleRecord(int chromosomeArmCount)
    {
        Events = new Event[chromosomeArmCount];
    }
}