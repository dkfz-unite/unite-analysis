namespace Unite.Analysis.Configuration.Options;

public interface IAnalysisOptions
{
    string DataPath { get; }
    string DataHost { get; }
    string SurvUrl { get; }
    string DmUrl { get; }
    string PcamUrl { get; }
    string DeUrl { get; }
    string ScellUrl { get; }
}
