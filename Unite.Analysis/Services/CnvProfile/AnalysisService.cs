using System.Diagnostics;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Models;

namespace Unite.Analysis.Services.CnvProfile;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    public AnalysisService(IAnalysisOptions options) : base(options)
    {
    }

    public override async Task<AnalysisTaskResult> Prepare(Models.Criteria.Analysis model, params object[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        var directoryPath = GetWorkingDirectoryPath(model.Id);

        /*var context = await _contextLoader.LoadDatasetData(model.Datasets.SingleOrDefault(), [AnalysisType.WGS, AnalysisType.WES]);

        WriteOptions(model.Options, directoryPath);

        var records = _processingService.ProcessData(context, model.Options);

        WriteRecords(records, directoryPath);*/

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public override Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        throw new NotImplementedException();
    }

    public override Task<Stream> Load(string key, params object[] args)
    {
        throw new NotImplementedException();
    }

    public override Task<Stream> Download(string key, params object[] args)
    {
        throw new NotImplementedException();
    }

    public override Task Delete(string key, params object[] args)
    {
        throw new NotImplementedException();
    }
}