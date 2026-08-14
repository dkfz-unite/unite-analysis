using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Helpers;
using Unite.Analysis.Models;
using Unite.Data.Entities.Omics.Analysis.Enums;

namespace Unite.Analysis.Services.CnvProfile;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    private readonly SamplesContextLoaderFull _contextLoader;
    private readonly ProcessingService _processingService;
    private readonly ILogger _logger;

    public static readonly string ResultFileName = OutputFile("result.json");
    public override string DefaultLoadFileName => ResultFileName;
    
    
    public AnalysisService(
        IAnalysisOptions options,
        SamplesContextLoaderFull contextLoader,
        ProcessingService processingService,
        ILogger<AnalysisService> logger) : base(options)
    {
        _contextLoader = contextLoader;
        _processingService = processingService;
        _logger = logger;
    }

    public override async Task<AnalysisTaskResult> Prepare(Models.Criteria.Analysis model, params object[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        var directoryPath = GetWorkingDirectoryPath(model.Id);
        var optionsPath = Path.Combine(directoryPath, OptionsFileName);
        var resultsPath = Path.Combine(directoryPath, ResultFileName);

        var context = await _contextLoader.LoadDatasetData(model.Datasets.SingleOrDefault(), [AnalysisType.WGS, AnalysisType.WES, AnalysisType.MethArray]);

        var records = await _processingService.ProcessData(context, model.Options);

        MemberJsonSerializer.Serialize(optionsPath, model.Options);
        MemberJsonSerializer.Serialize(resultsPath, records);

        stopwatch.Stop();

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }

    public override async Task<AnalysisTaskResult> Process(string key, params object[] args)
    {
        var stopwatch = Stopwatch.StartNew();

        ArchiveResults(GetWorkingDirectoryPath(key));
        
        stopwatch.Stop();

        await Task.CompletedTask;

        return AnalysisTaskResult.Success(stopwatch.Elapsed.TotalSeconds);
    }
}