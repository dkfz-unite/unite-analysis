namespace Unite.Analysis.Services.DonSce.Models.Context;

public class AnalysisContext
{
    public List<DatasetContext> Datasets { get; set; }


    public AnalysisContext()
    {
        Datasets = [];
    }
}
