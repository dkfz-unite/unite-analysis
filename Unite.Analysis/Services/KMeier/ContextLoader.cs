using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;
using Unite.Data.Context;
using Unite.Data.Context.Extensions.Queryable;
using Unite.Indices.Search.Services;

namespace Unite.Analysis.Services.KMeier;

public class ContextLoader
{
    private readonly ISearchService<Indices.Entities.Donors.DonorIndex> _donorsSearchService;
    private readonly ISearchService<Indices.Entities.Images.ImageIndex> _imagesSearchService;
    private readonly ISearchService<Indices.Entities.Specimens.SpecimenIndex> _specimensSearchService;
    private readonly IDbContextFactory<DomainDbContext> _dbContextFactory;

    
    public ContextLoader(
        ISearchService<Indices.Entities.Donors.DonorIndex> donorsSearchService,
        ISearchService<Indices.Entities.Images.ImageIndex> imagesSearchService,
        ISearchService<Indices.Entities.Specimens.SpecimenIndex> specimensSearchService,
        IDbContextFactory<DomainDbContext> dbContextFactory)
    {
        _donorsSearchService = donorsSearchService;
        _imagesSearchService = imagesSearchService;
        _specimensSearchService = specimensSearchService;
        _dbContextFactory = dbContextFactory;
    }


    public Task<AnalysisContext> LoadDatasetData(DatasetCriteria model)
    {
        return model.Domain switch
        {
            DatasetDomain.Donors => LoadDonorsDatasetData(model),
            DatasetDomain.Mris => LoadImagesDatasetData(model),
            DatasetDomain.Cts => LoadImagesDatasetData(model),
            DatasetDomain.Materials => LoadSpecimensDatasetData(model),
            DatasetDomain.Lines => LoadSpecimensDatasetData(model),
            DatasetDomain.Organoids => LoadSpecimensDatasetData(model),
            DatasetDomain.Xenografts => LoadSpecimensDatasetData(model),
            _ => throw new NotImplementedException($"Domain {model.Domain} is not supported.")
        };
    }


    private async Task<AnalysisContext> LoadDonorsDatasetData(DatasetCriteria model)
    {
        var stats = await _donorsSearchService.Stats(model.Criteria);

        var context = new AnalysisContext(1);

        var donorIds = stats.Keys.Cast<int>().ToArray();
        context.Donors = await LoadDonors(donorIds);
        
        return context;
    }

    private async Task<AnalysisContext> LoadImagesDatasetData(DatasetCriteria model)
    {
        var stats = await _imagesSearchService.Stats(model.Criteria);

        var context = new AnalysisContext(2);

        var imageIds = stats.Keys.Cast<int>().ToArray();
        context.Images = await LoadImages(imageIds);

        var donorIds = context.Images.Values.Select(image => image.DonorId).Distinct().ToArray();
        context.Donors = await LoadDonors(donorIds);

        return context;
    }

    private async Task<AnalysisContext> LoadSpecimensDatasetData(DatasetCriteria model)
    {
        var stats = await _specimensSearchService.Stats(model.Criteria);

        var context = new AnalysisContext(3);

        var specimenIds = stats.Keys.Cast<int>().ToArray();
        context.Specimens = await LoadSpecimens(specimenIds);

        var donorIds = context.Specimens.Values.Select(specimen => specimen.DonorId).Distinct().ToArray();
        context.Donors = await LoadDonors(donorIds);

        return context;
    }


    private async Task<Dictionary<int, Data.Entities.Donors.Donor>> LoadDonors(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<Data.Entities.Donors.Donor>()
            .AsNoTracking()
            .IncludeClinicalData()
            .Where(donor => ids.Contains(donor.Id))
            .ToDictionaryAsync(donor => donor.Id);
    }

    private async Task<Dictionary<int, Data.Entities.Images.Image>> LoadImages(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<Data.Entities.Images.Image>()
            .AsNoTracking()
            .Include(image => image.Donor.ClinicalData)
            .Where(image => ids.Contains(image.Id))
            .ToDictionaryAsync(image => image.Id);
    }

    private async Task<Dictionary<int, Data.Entities.Specimens.Specimen>> LoadSpecimens(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<Data.Entities.Specimens.Specimen>()
            .AsNoTracking()
            .Include(specimen => specimen.Donor.ClinicalData)
            .Where(specimen => ids.Contains(specimen.Id))
            .ToDictionaryAsync(specimen => specimen.Id);
    }
}
