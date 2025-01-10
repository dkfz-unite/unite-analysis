namespace Unite.Analysis.Models;

public abstract record AnalysisData
{
    /// <summary>
    /// Analysis Id. Used to identify the analysis in the queue and UI.
    /// </summary>
    // public string Id { get; set; }
    public string Id { get; set; }


    /// <summary>
    /// Datasets to analyse.
    /// </summary>
    public DatasetCriteria[] Datasets { get; set; }
}
