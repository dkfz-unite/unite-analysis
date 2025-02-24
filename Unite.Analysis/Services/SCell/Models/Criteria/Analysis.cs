using Unite.Analysis.Models;

namespace Unite.Analysis.Services.SCell.Models.Criteria;

public record Analysis: AnalysisData
{
    /// <summary>
    /// Custom annotations.
    /// </summary>
    public string Annotations { get; set; }
    
    /// <summary>
    /// Analysis options.
    /// </summary>
    public Options Options { get; set; } = new();
}
