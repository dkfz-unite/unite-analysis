namespace Unite.Analysis.Services.Surv.Models.Context;

public class AnalysisContext
{
    public List<DatasetContext> Datasets { get; set; }


    public AnalysisContext()
    {
        Datasets = [];
    }
}
