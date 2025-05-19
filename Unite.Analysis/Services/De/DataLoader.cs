using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Extensions;
using Unite.Analysis.Models;
using Unite.Analysis.Models.Enums;
using Unite.Data.Context;
using Unite.Data.Context.Repositories;
using Unite.Data.Entities.Omics.Analysis;
using Unite.Data.Entities.Omics.Analysis.Rna;
using Unite.Indices.Search.Services;

using GeneExpressions = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, int>>; // GeneStableId, SampleId, Reads
using SampleExpressions = (int, Unite.Data.Entities.Omics.Analysis.Rna.GeneExpression[]); // SampleId, BulkExpression[]

namespace Unite.Analysis.Services.De;

public class DataLoader
{
    private const byte _threadsCount = 10;

    private readonly DonorsRepository _donorRepository;
    private readonly ImagesRepository _imageRepository;
    private readonly ISearchService<Indices.Entities.Donors.DonorIndex> _donorsSearchService;
    private readonly ISearchService<Indices.Entities.Images.ImageIndex> _imagesSearchService;
    private readonly ISearchService<Indices.Entities.Specimens.SpecimenIndex> _specimensSearchService;
    private readonly IDbContextFactory<DomainDbContext> _dbContextFactory;


    public DataLoader(
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
    }


    public Task<GeneExpressions> LoadDatasetData(DatasetCriteria model)
    {
        return model.Domain switch
        {
            DatasetDomain.Donors => LoadDonorsDatasetData(model),
            DatasetDomain.Mrs => LoadImagesDatasetData(model),
            DatasetDomain.Materials => LoadSpecimensDatasetData(model),
            DatasetDomain.Lines => LoadSpecimensDatasetData(model),
            DatasetDomain.Organoids => LoadSpecimensDatasetData(model),
            DatasetDomain.Xenografts => LoadSpecimensDatasetData(model),
            _ => throw new NotSupportedException()
        };
    }


    private async Task<GeneExpressions> LoadDonorsDatasetData(DatasetCriteria model)
    {
        var stats = await _donorsSearchService.Stats(model.Criteria);

        var ids = stats.Keys.Cast<int>().ToArray();

        var expressions = new GeneExpressions();

        await ids.Batch(_threadsCount, id => 
            LoadDonorExpressions(id)
                .ContinueWith(task => MapSampleExpressions(model.Domain, task.Result, expressions))
        );

        return expressions;
    }

    private async Task<GeneExpressions> LoadImagesDatasetData(DatasetCriteria model)
    {
        var stats = await _imagesSearchService.Stats(model.Criteria);

        var ids = stats.Keys.Cast<int>().ToArray();

        var expressions = new GeneExpressions();

        await ids.Batch(_threadsCount, id => 
            LoadImageExpressions(id)
                .ContinueWith(task => MapSampleExpressions(model.Domain, task.Result, expressions))
        );

        return expressions;
    }

    private async Task<GeneExpressions> LoadSpecimensDatasetData(DatasetCriteria model)
    {
        var stats = await _specimensSearchService.Stats(model.Criteria);

        var ids = stats.Keys.Cast<int>().ToArray();

        var expressions = new GeneExpressions();

        await ids.Batch(_threadsCount, id => 
            LoadSpecimenExpressions(id)
                .ContinueWith(task => MapSampleExpressions(model.Domain, task.Result, expressions))
        );

        return expressions;
    }

    private async Task<SampleExpressions> LoadDonorExpressions(int id)
    {
        var specimenIds = await _donorRepository.GetRelatedSpecimens([id]);

        var expressions = await LoadSampleExpressions(specimenIds);

        return (id, expressions);
    }

    private async Task<SampleExpressions> LoadImageExpressions(int id)
    {
        var specimenIds = await _imageRepository.GetRelatedSpecimens([id]);

        var expressions = await LoadSampleExpressions(specimenIds);

        return (id, expressions);
    }

    private async Task<SampleExpressions> LoadSpecimenExpressions(int id)
    {
        var expressions = await LoadSampleExpressions([id]);

        return (id, expressions);
    }

    private async Task<GeneExpression[]> LoadSampleExpressions(int[] specimenIds)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var sample = await dbContext.Set<Sample>()
            .AsNoTracking()
            .Where(sample => specimenIds.Contains(sample.SpecimenId))
            .Where(sample => sample.GeneExpressions.Any())
            .PickOrDefaultAsync();

        if (sample != null)
        {
            return await dbContext.Set<GeneExpression>()
                .AsNoTracking()
                .Include(expression => expression.Entity)
                .Where(expression => expression.SampleId == sample.Id)
                .OrderBy(expression => expression.Entity.ChromosomeId)
                .ThenBy(expression => expression.Entity.Start)
                .ToArrayAsync();
        }
        else
        {
            return null;
        }
        
    }


    private static void MapSampleExpressions(DatasetDomain domain, SampleExpressions sampleExpressions, GeneExpressions geneExpressions)
    {
        var (id, expressions) = sampleExpressions;

        if (expressions != null)
        {
            foreach (var expression in expressions)
            {
                var geneId = expression.Entity.StableId;

                lock (geneExpressions)
                {
                    if (!geneExpressions.ContainsKey(geneId))
                        geneExpressions.Add(geneId, []);

                    geneExpressions[geneId].Add($"{domain}_{id}", expression.Reads);
                }
            }
        }
    }
}
