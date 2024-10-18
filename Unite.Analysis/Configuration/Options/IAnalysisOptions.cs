namespace Unite.Analysis.Configuration.Options;

public interface IAnalysisOptions
{
    string DataPath { get; }
    string DataHost { get; }
    string DESeq2Url { get; }
    string SCellUrl { get; }
    string KMeierUrl { get; }
}
