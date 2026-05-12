using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public class SampleRecords
{
    public ChromosomeArm[] ChromosomeArms { get; }
    public SampleRecord[] Records { get; }

    public SampleRecords(int chromosomeArmCount, int recordCount)
    {
        ChromosomeArms = new ChromosomeArm[chromosomeArmCount];
        Records = new SampleRecord[recordCount];
    }
}