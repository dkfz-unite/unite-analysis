namespace Unite.Analysis.Configuration.Options;

public interface IAnalysisOptions
{
    string DataPath { get; }
    string DataHost { get; }
    string RnaDeUrl { get; }
    string RnascUrl { get; }
}
