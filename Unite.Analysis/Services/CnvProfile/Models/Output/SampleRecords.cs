using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public class SampleRecords
{
    public SampleRecord[] Records { get; }

    public SampleRecords(int recordCount)
    {
        Records = new SampleRecord[recordCount];
    }
}