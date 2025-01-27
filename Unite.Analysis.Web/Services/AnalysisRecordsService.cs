using Unite.Cache.Configuration.Options;
using Unite.Analysis.Models;
using Unite.Analysis.Web.Repositories;

namespace Unite.Analysis.Web.Services;

public record SearchModel(string UserId);

public class AnalysisRecordsService
{
    private readonly AnalysesRepository _analysesRepository;


    public AnalysisRecordsService(IMongoOptions options)
    {
        _analysesRepository = new AnalysesRepository(options);
    }


	public async Task<GenericAnalysis[]> Load(SearchModel model)
	{
		var entries = await _analysesRepository.WhereAsync(item =>item.Document.UserId == model.UserId);

        return entries.Select(entry => entry.Document with { Id = entry.Id }).ToArray();
	}

    public async Task Delete(SearchModel model)
    {
        var entries = await _analysesRepository.WhereAsync(item => item.Document.UserId == model.UserId);

        foreach (var entry in entries)
        {
            await _analysesRepository.DeleteAsync(entry.Id);
        }
    }
}
