using Unite.Analysis.Models;
using Unite.Analysis.Web.Repositories;
using Unite.Cache.Configuration.Options;

namespace Unite.Analysis.Web.Services;

public class AnalysisRecordService
{
    private readonly AnalysesRepository _analysesRepository;


    public AnalysisRecordService(IMongoOptions options)
    {
        _analysesRepository = new AnalysesRepository(options);
    }


    public async Task<string> Add(GenericAnalysis data)
	{
		return await _analysesRepository.AddAsync(data);
	}

    public async Task Update(string id, string status)
	{
		var entry = _analysesRepository.Find(id).Document;
        entry.Status = status;
        
        await _analysesRepository.UpdateAsync(id, entry);
	}

    public async Task Delete(string id)
	{
		await _analysesRepository.DeleteAsync(id);
	}

	public async Task<IEnumerable<GenericAnalysis>> Get(string userId)
	{
		var analyses = await _analysesRepository.WhereAsync(item => item.Document.UserId == userId);
        return analyses.Select(analysis => analysis.Document with { Id = analysis.Id });
	}
}
