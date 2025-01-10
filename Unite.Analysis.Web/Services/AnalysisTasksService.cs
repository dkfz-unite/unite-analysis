using System.Text.Json;
using Unite.Cache.Configuration.Options;
using Unite.Analysis.Models;
using Docker.DotNet.Models;

namespace Unite.Analysis.Web.Services;

public class AnalysisTasksService
{
    private readonly Repositories.AnalysesRepository _analysesRepository;

    public AnalysisTasksService(IMongoOptions options)
    {
        _analysesRepository = new Repositories.AnalysesRepository(options);
    }

	public async Task<IEnumerable<GenericAnalysis>> Load(string userId)
	{
		var genericAnalyses = await _analysesRepository.WhereAsync(item =>item.Document.UserId == userId);

        return genericAnalyses.Select(item => item.Document with {Id = item.Id});
	}
}