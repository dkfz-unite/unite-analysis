namespace Unite.Analysis.Configuration.Options;

public interface IAnalysisOptions
{
    string DataPath { get; }
    string DataHost { get; }
    string DonSceUrl { get; }
    string MethDmUrl { get; }
    string RnaDeUrl { get; }
    string RnascDcUrl { get; }
}
