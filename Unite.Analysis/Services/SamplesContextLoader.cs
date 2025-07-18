using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;
using Unite.Data.Context;
using Unite.Data.Context.Repositories;
using Unite.Data.Entities.Donors;
using Unite.Data.Entities.Images;
using Unite.Data.Entities.Specimens;
using Unite.Indices.Search.Services;

using DonorIndex = Unite.Indices.Entities.Donors.DonorIndex;
using ImageIndex = Unite.Indices.Entities.Images.ImageIndex;
using SpecimenIndex = Unite.Indices.Entities.Specimens.SpecimenIndex;

namespace Unite.Analysis.Services;

public class SamplesContextLoader
{
    protected readonly DonorsRepository _donorRepository;
    protected readonly ImagesRepository _imageRepository;
    protected readonly SpecimensRepository _specimenRepository;
    protected readonly ISearchService<DonorIndex> _donorsSearchService;
    protected readonly ISearchService<ImageIndex> _imagesSearchService;
    protected readonly ISearchService<SpecimenIndex> _specimensSearchService;
    protected readonly IDbContextFactory<DomainDbContext> _dbContextFactory;


    public SamplesContextLoader(
        ISearchService<DonorIndex> donorsSearchService,
        ISearchService<ImageIndex> imagesSearchService,
        ISearchService<SpecimenIndex> specimensSearchService,
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


    public virtual async Task<SamplesContext> LoadDatasetData(DatasetCriteria model, params Data.Entities.Omics.Analysis.Enums.AnalysisType[] analysisTypes)
    {
        return model.Domain switch
        {
            DatasetDomain.Donors => await LoadDonorsDatasetData(model, analysisTypes),
            DatasetDomain.Mrs => await LoadImagesDatasetData(model, analysisTypes),
            DatasetDomain.Cts => await LoadImagesDatasetData(model, analysisTypes),
            DatasetDomain.Materials => await LoadSpecimensDatasetData(model, analysisTypes),
            DatasetDomain.Lines => await LoadSpecimensDatasetData(model, analysisTypes),
            DatasetDomain.Organoids => await LoadSpecimensDatasetData(model, analysisTypes),
            DatasetDomain.Xenografts => await LoadSpecimensDatasetData(model, analysisTypes),
            _ => throw new NotSupportedException($"Dataset domain '{model.Domain}' is not supported.")
        };
    }

    protected virtual async Task<SamplesContext> LoadDonorsDatasetData(DatasetCriteria model, params Data.Entities.Omics.Analysis.Enums.AnalysisType[] analysisTypes)
    {
        var stats = await _donorsSearchService.Stats(model.Criteria);

        var context = new SamplesContext(1);

        var donorIds = stats.Keys.Cast<int>().ToArray();
        context.Donors = await LoadDonors(donorIds);

        var specimenIds = await _donorRepository.GetRelatedSpecimens(donorIds);
        context.Specimens = await LoadSpecimens(specimenIds);

        var sampleIds = await _specimenRepository.GetRelatedSamples(specimenIds, analysisTypes);
        context.OmicsSamples = await LoadSamples(sampleIds);

        context.OmicsSamples = context.Donors.Values
            .Select(donor => context.OmicsSamples.Values
                .Where(sample => context.GetSampleDonor(sample.Id)?.Id == donor.Id)
                .OrderBy(sample => context.GetSampleRelevance(sample.Id))
                .FirstOrDefault())
            .DistinctBy(sample => sample.Id)
            .ToDictionary(sample => sample.Id);

        return context;
    }

    protected virtual async Task<SamplesContext> LoadImagesDatasetData(DatasetCriteria model, params Data.Entities.Omics.Analysis.Enums.AnalysisType[] analysisTypes)
    {
        var stats = await _imagesSearchService.Stats(model.Criteria);

        var context = new SamplesContext(2);

        var imageIds = stats.Keys.Cast<int>().ToArray();
        context.Images = await LoadImages(imageIds);

        var donorIds = await _imageRepository.GetRelatedDonors(imageIds);
        context.Donors = await LoadDonors(donorIds);

        var specimenIds = await _imageRepository.GetRelatedSpecimens(imageIds);
        context.Specimens = await LoadSpecimens(specimenIds);

        var sampleIds = await _specimenRepository.GetRelatedSamples(specimenIds, analysisTypes);
        context.OmicsSamples = await LoadSamples(sampleIds);

        context.OmicsSamples = context.Images.Values
            .Select(image => context.OmicsSamples.Values
                .Where(sample => context.GetSampleImage(sample.Id)?.Id == image.Id)
                .OrderBy(sample => context.GetSampleRelevance(sample.Id))
                .FirstOrDefault())
            .DistinctBy(sample => sample.Id)
            .ToDictionary(sample => sample.Id);

        if (stats.Keys.Count() != context.OmicsSamples.Count)
            throw new InvalidOperationException("Images should be of unique donors");

        return context;
    }

    protected virtual async Task<SamplesContext> LoadSpecimensDatasetData(DatasetCriteria model, params Data.Entities.Omics.Analysis.Enums.AnalysisType[] analysisTypes)
    {
        var stats = await _specimensSearchService.Stats(model.Criteria);

        var context = new SamplesContext(3);

        var specimenIds = stats.Keys.Cast<int>().ToArray();
        context.Specimens = await LoadSpecimens(specimenIds);

        var donorIds = await _specimenRepository.GetRelatedDonors(specimenIds);
        context.Donors = await LoadDonors(donorIds);

        var sampleIds = await _specimenRepository.GetRelatedSamples(specimenIds, analysisTypes);
        context.OmicsSamples = await LoadSamples(sampleIds);

        context.OmicsSamples = context.Specimens.Values
            .Select(specimen => context.OmicsSamples.Values
                .Where(sample => context.GetSampleSpecimen(sample.Id)?.Id == specimen.Id)
                .OrderBy(sample => context.GetSampleRelevance(sample.Id))
                .FirstOrDefault())
            .DistinctBy(sample => sample.Id)
            .ToDictionary(sample => sample.Id);

        return context;
    }


    protected virtual async Task<Dictionary<int, Donor>> LoadDonors(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var entitiesQuery = dbContext.Set<Donor>().AsNoTracking();

        var includeQuery = Include(entitiesQuery);

        return await includeQuery
            .Where(donor => ids.Contains(donor.Id))
            .ToDictionaryAsync(donor => donor.Id);
    }

    protected virtual async Task<Dictionary<int, Image>> LoadImages(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var entitiesQuery = dbContext.Set<Image>().AsNoTracking();

        var includeQuery = Include(entitiesQuery);

        return await includeQuery
            .Where(image => ids.Contains(image.Id))
            .ToDictionaryAsync(image => image.Id);
    }

    protected virtual async Task<Dictionary<int, Specimen>> LoadSpecimens(int[] ids)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var entitiesQuery = dbContext.Set<Specimen>().AsNoTracking();

        var includeQuery = Include(entitiesQuery);

        return await includeQuery
            .Where(specimen => ids.Contains(specimen.Id))
            .ToDictionaryAsync(specimen => specimen.Id);
    }

    protected virtual async Task<Dictionary<int, Data.Entities.Omics.Analysis.Sample>> LoadSamples(int[] ids)
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

    protected virtual IQueryable<Donor> Include(IQueryable<Donor> query)
    {
        return query;
    }

    protected virtual IQueryable<Image> Include(IQueryable<Image> query)
    {
        return query;
    }

    protected virtual IQueryable<Specimen> Include(IQueryable<Specimen> query)
    {
        return query;
    }
}
