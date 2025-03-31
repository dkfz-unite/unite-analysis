using Unite.Analysis.Models;

namespace Unite.Analysis.Services.Meth.Models.Criteria;

public record Analysis: AnalysisData
{
    /// <summary>
    public Options Options { get; set; }
}
