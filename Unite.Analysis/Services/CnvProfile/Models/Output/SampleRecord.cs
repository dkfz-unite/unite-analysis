using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public class SampleRecord
{
    public int SampleId { get; }
    public Event[] Events { get; }

    public SampleRecord(int sampleId, int chromosomeArmCount)
    {
        SampleId = sampleId;
        Events = new Event[chromosomeArmCount];
    }
}