namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public class SampleRecords
{
    public readonly SampleRecord[] Records;

    public SampleRecords(int recordCount)
    {
        Records = new SampleRecord[recordCount];
    }
}