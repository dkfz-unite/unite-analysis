using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;
using Unite.Analysis.Services.Surv.Models.Context;
using Unite.Data.Context;
using Unite.Data.Context.Repositories.Extensions.Queryable;
using Unite.Indices.Search.Services;

namespace Unite.Analysis.Services.Surv;

public class ContextLoader
{
    private readonly ISearchService<Indices.Entities.Donors.DonorIndex> _donorsSearchService;
    private readonly IDbContextFactory<DomainDbContext> _dbContextFactory;

    
    public ContextLoader(
        ISearchService<Indices.Entities.Donors.DonorIndex> donorsSearchService,
        IDbContextFactory<DomainDbContext> dbContextFactory)
    {
        _donorsSearchService = donorsSearchService;
        _dbContextFactory = dbContextFactory;
    }


    public async Task<AnalysisContext> Load(DatasetCriteria[] datasets)
    {
        var analysisContext = new AnalysisContext();

        foreach (var dataset in datasets)
        {
            var datasetContext = await LoadDatasetData(dataset);

            analysisContext.Datasets.Add(datasetContext);
        }

        return analysisContext;
    }


    private async Task<DatasetContext> LoadDatasetData(DatasetCriteria model)
    {
        return model.Domain switch
        {
            DatasetDomain.Donors => await LoadDonorsDatasetData(model),
            _ => throw new NotSupportedException($"Domain {model.Domain} is not supported.")
        };
    }

    private async Task<DatasetContext> LoadDonorsDatasetData(DatasetCriteria model)
    {
        var stats = await _donorsSearchService.Stats(model.Criteria);

        var context = new DatasetContext(model.Name);
        context.Keys = stats.Keys.Cast<int>().ToArray();
        context.Donors = await LoadDonors(context.Keys);
        
        return context;
    }

    private async Task<Data.Entities.Donors.Donor[]> LoadDonors(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<Data.Entities.Donors.Donor>()
            .AsNoTracking()
            .IncludeClinicalData()
            .Where(donor => ids.Contains(donor.Id))
            .ToArrayAsync();
    }
}
