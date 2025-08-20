using Unite.Analysis.Models;

namespace Unite.Analysis.Services.Gaf.Models.Criteria;

public record Analysis : AnalysisData
{
     /// <summary>
    /// Analysis options.
    /// </summary>
    public Options Options { get; set; } = new Options();
}
