namespace Unite.Analysis.Services.KMeier.Models.Context;

public class AnalysisContext
{
    public List<DatasetContext> Datasets { get; set; }


    public AnalysisContext()
    {
        Datasets = [];
    }
}