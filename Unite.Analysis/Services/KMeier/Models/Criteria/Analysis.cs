using Unite.Analysis.Models;

namespace Unite.Analysis.Services.KMeier.Models.Criteria;

public record Analysis: AnalysisData
{
    /// <summary>
    /// Analysis options.
    /// </summary> 
    public Options Options { get; set; }

    public string UserId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Date { get; set; }

    public string Status { get; set; }

    public string Type { get; set; }
}
