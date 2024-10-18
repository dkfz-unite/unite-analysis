using Unite.Analysis.Models;

namespace Unite.Analysis.Services.SCell.Models;

public class Analysis
{
    /// <summary>
    /// Analysis key. Used to identify the analysis in the queue and UI.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Datasets to analyse.
    /// </summary>
    public DatasetCriteria[] Datasets { get; set; }

    /// <summary>
    /// Analysis options.
    /// </summary>
    public Options Options { get; set; } = new();
}
