using System.Diagnostics;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Helpers;
using Unite.Analysis.Models;
using Unite.Analysis.Services.CnvProfile.Models.Criteria;
using Unite.Analysis.Services.Gaf.Models.Output;
using Unite.Data.Entities.Omics.Analysis.Enums;

namespace Unite.Analysis.Services.CnvProfile;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    private const string OptionsFileName = "options.json";
    private const string ResultFileName = "result.json";
    private const string ArchiveFileName = "output.zip";
    
    private readonly SamplesContextLoader _contextLoader;
    
    public AnalysisService(IAnalysisOptions options, SamplesContextLoader contextLoader) : base(options)
    {
        _contextLoader = contextLoader;
    }

    public override async Task<AnalysisTaskResult> Prepare(Models.Criteria.Analysis model, params object[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        var directoryPath = GetWorkingDirectoryPath(model.Id);

        var context = await _contextLoader.LoadDatasetData(model.Datasets.SingleOrDefault(), [AnalysisType.WGS, AnalysisType.WES]);

        WriteOptions(model.Options, directoryPath);

        /*var records = _processingService.ProcessData(context, model.Options);

        WriteRecords(records, directoryPath);*/

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public override async Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var stopwatch = Stopwatch.StartNew();
        var directoryPath = GetWorkingDirectoryPath(key);
        
        
        
        stopwatch.Stop();
        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
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
    
    private static void WriteOptions(Options options, string path)
    {
        var json = MemberJsonSerializer.Serialize(options);

        File.WriteAllText(Path.Combine(path, OptionsFileName), json);
    }

    private static void WriteRecords(Records records, string path)
    {
        var json = MemberJsonSerializer.Serialize(records);

        File.WriteAllText(Path.Combine(path, ResultFileName), json);
    }
}