using Unite.Cache.Configuration.Options;
using Unite.Analysis.Models;
using Unite.Analysis.Web.Repositories;

namespace Unite.Analysis.Web.Services;

public class AnalysisRecordsService
{
    private readonly AnalysesRepository _analysesRepository;

    public AnalysisRecordsService(IMongoOptions options)
    {
        _analysesRepository = new AnalysesRepository(options);
    }

	public async Task<IEnumerable<GenericAnalysis>> Load(string userId)
	{
		var entries = await _analysesRepository.WhereAsync(item =>item.Document.UserId == userId);

        return entries.Select(entry => entry.Document with { Id = entry.Id });
	}
}
