using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Models;

namespace Unite.Analysis.Services.CnvProfile;

public class AnalysisService : AnalysisService<Models.Criteria.Analysis>
{
    public AnalysisService(IAnalysisOptions options) : base(options)
    {
    }

    public override Task<AnalysisTaskResult> Prepare(Models.Criteria.Analysis model, params object[] args)
    {
        throw new NotImplementedException();
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