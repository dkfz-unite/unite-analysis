using Unite.Analysis.Models;

namespace Unite.Analysis.Services.CnvProfile.Models.Criteria;

public record Analysis : AnalysisData
{
    public Options Options { get; set; } = new Options();
}