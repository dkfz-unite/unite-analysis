using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Extensions;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;
using Unite.Analysis.Services.Dm.Extensions;
using Unite.Analysis.Services.Dm.Models.Context;
using Unite.Data.Context;
using Unite.Data.Context.Repositories;
using Unite.Data.Context.Repositories.Extensions.Queryable;
using Unite.Indices.Search.Services;

namespace Unite.Analysis.Services.Dm;

public class ContextLoader
{
    private readonly DonorsRepository _donorRepository;
    private readonly ImagesRepository _imageRepository;
    private readonly SpecimensRepository _specimenRepository;
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

        _donorRepository = new DonorsRepository(dbContextFactory);
        _imageRepository = new ImagesRepository(dbContextFactory);
        _specimenRepository = new SpecimensRepository(dbContextFactory);
    }


    public async Task<AnalysisContext> LoadDatasetData(DatasetCriteria model)
    {
        return model.Domain switch
        {
            DatasetDomain.Donors => await LoadDonorsDatasetData(model),
            DatasetDomain.Mrs => await LoadImagesDatasetData(model),
            DatasetDomain.Materials => await LoadSpecimensDatasetData(model),
            _ => throw new NotImplementedException($"Domain {model.Domain} is not supported.")
        };
    }

    private async Task<AnalysisContext> LoadDonorsDatasetData(DatasetCriteria model)
    {
        var stats = await _donorsSearchService.Stats(model.Criteria);

        var context = new AnalysisContext(1);

        var donorIds = stats.Keys.Cast<int>().ToArray();
        context.Donors = await LoadDonors(donorIds);

        var specimenIds = await _donorRepository.GetRelatedSpecimens(context.Donors.Select(donor => donor.Value.Id).ToArray());
        context.Specimens = await LoadSpecimens(specimenIds);
       
        var sampleIds = await _specimenRepository.GetRelatedSamples(context.Specimens.Select(specimen => specimen.Value.Id).ToArray(), _dbContextFactory);
        context.Samples = await LoadSamples(sampleIds);

        context.Samples = context.Donors.Values
            .Select(donor => context.Samples.Values
                .Where(sample => sample.GetDonor(context)?.Id == donor.Id)
                .Where(sample => sample.Analysis.TypeId == Data.Entities.Omics.Analysis.Enums.AnalysisType.MethArray)
                .OrderBy(sample => sample.GetSpecimen(context)?.GetRelevance())
                .FirstOrDefault())
            .DistinctBy(sample => sample.Id)
            .ToDictionary(sample => sample.Id);
        

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

        var specimenIds = await _imageRepository.GetRelatedSpecimens(context.Donors.Select(donor => donor.Value.Id).ToArray());
        context.Specimens = await LoadSpecimens(specimenIds);

        var sampleIds = await _specimenRepository.GetRelatedSamples(context.Specimens.Select(specimen => specimen.Value.Id).ToArray(), _dbContextFactory);
        context.Samples = await LoadSamples(sampleIds);

        context.Samples = context.Images.Values
            .Select(image => context.Samples.Values
                .Where(sample => sample.GetImage(context)?.Id == image.Id)
                .Where(sample => sample.Analysis.TypeId == Data.Entities.Omics.Analysis.Enums.AnalysisType.RNASeqSc)
                .OrderBy(sample => sample.GetSpecimen(context)?.GetRelevance())
                .FirstOrDefault())
            .DistinctBy(sample => sample.Id)
            .ToDictionary(sample => sample.Id);

        if (stats.Keys.Count() != context.Samples.Count)
            throw new InvalidOperationException("Images should be of unique donors");

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

        var sampleIds = await _specimenRepository.GetRelatedSamples(context.Specimens.Select(specimen => specimen.Value.Id).ToArray(), _dbContextFactory);
        context.Samples = await LoadSamples(sampleIds);

        context.Samples = context.Specimens.Values
            .Select(specimen => context.Samples.Values
                .Where(sample => sample.GetSpecimen(context)?.Id == specimen.Id)
                .Where(sample => sample.Analysis.TypeId == Data.Entities.Omics.Analysis.Enums.AnalysisType.MethArray)
                .OrderBy(sample => sample.GetSpecimen(context)?.GetRelevance())
                .FirstOrDefault())
            .DistinctBy(sample => sample.Id)
            .ToDictionary(sample => sample.Id);

        return context;
    }


    private async Task<Dictionary<int, Data.Entities.Donors.Donor>> LoadDonors(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<Data.Entities.Donors.Donor>()
            .AsNoTracking()
            .Where(donor => ids.Contains(donor.Id))
            .ToDictionaryAsync(donor => donor.Id);
    }

    private async Task<Dictionary<int, Data.Entities.Images.Image>> LoadImages(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<Data.Entities.Images.Image>()
            .AsNoTracking()
            .IncludeMrImage()
            .Where(image => ids.Contains(image.Id))
            .ToDictionaryAsync(image => image.Id);
    }

    private async Task<Dictionary<int, Data.Entities.Specimens.Specimen>> LoadSpecimens(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<Data.Entities.Specimens.Specimen>()
            .AsNoTracking()
            .IncludeMaterial()
            .Where(specimen => ids.Contains(specimen.Id))
            .ToDictionaryAsync(specimen => specimen.Id);
    }

    private async Task<Dictionary<int, Data.Entities.Omics.Analysis.Sample>> LoadSamples(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        return await dbContext.Set<Data.Entities.Omics.Analysis.Sample>()
            .AsNoTracking()
            .Include(sample => sample.Specimen)
            .Include(sample => sample.Analysis)
            .Include(sample => sample.Resources)
            .Where(sample => ids.Contains(sample.Id))
            .ToDictionaryAsync(sample => sample.Id);
    }
}
