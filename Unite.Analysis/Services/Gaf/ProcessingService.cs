using Microsoft.EntityFrameworkCore;
using Unite.Analysis.Services.Gaf.Models.Criteria;
using Unite.Analysis.Services.Gaf.Models.Output;
using Unite.Data.Context;
using Unite.Data.Context.Repositories;
using Unite.Data.Entities.Omics.Analysis.Dna;

using SM = Unite.Data.Entities.Omics.Analysis.Dna.Sm;
using CNV = Unite.Data.Entities.Omics.Analysis.Dna.Cnv;
using SV = Unite.Data.Entities.Omics.Analysis.Dna.Sv;
using Unite.Data.Context.Repositories.Extensions.Queryable;

namespace Unite.Analysis.Services.Gaf;

public class ProcessingService
{
    protected readonly SpecimensRepository _specimensRepository;
    protected readonly VariantsRepository _variantsRepository;
    protected readonly IDbContextFactory<DomainDbContext> _dbContextFactory;


    public ProcessingService(
        IDbContextFactory<DomainDbContext> dbContextFactory)
    {
        _specimensRepository = new SpecimensRepository(dbContextFactory);
        _variantsRepository = new VariantsRepository(dbContextFactory);
        _dbContextFactory = dbContextFactory;
    }


    public Records ProcessData(SamplesContext context, Options options)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();

        var impacts = new string[] { Effect.Impacts.High, Effect.Impacts.Moderate, Effect.Impacts.Low };

        var sampleIds = context.OmicsSamples.Keys.ToArray();

        var entries = dbContext.Set<SM.VariantEntry>()
            .AsNoTracking()
            .IncludeAffectedTranscripts()
            .Where(entry => sampleIds.Contains(entry.SampleId))
            .Where(entry => entry.Entity.AffectedTranscripts.Any(transcript => transcript.Distance == null))
            .ToArray();

        var groups = entries
            .Where(entry => impacts.Contains(entry.Entity.MostAffectedTranscript.MostSevereEffect.Impact))
            .GroupBy(entry => entry.Entity.MostAffectedTranscript.Feature.GeneId.Value)
            .Select(geneGroup => new
            {
                Gene = geneGroup.First().Entity.MostAffectedTranscript.Feature.Gene,
                Samples = geneGroup.GroupBy(entry => entry.SampleId).ToArray()
            })
            .OrderByDescending(group => group.Samples.Length)
            .Take(options.Genes)
            .ToArray();

        var genes = new Dictionary<int, GeneRecord>();
        var donors = new Dictionary<int, DonorRecord>();
        var variants = new List<VariantRecord>();

        foreach (var geneGroup in groups)
        {
            var gene = geneGroup.Gene;
            if (!genes.ContainsKey(gene.Id))
                genes.Add(gene.Id, new GeneRecord(gene));

            foreach (var sampleGroup in geneGroup.Samples)
            {
                var donor = context.GetSampleDonor(sampleGroup.Key);
                if (!donors.ContainsKey(donor.Id))
                    donors.Add(donor.Id, new DonorRecord(donor));

                foreach (var entry in sampleGroup)
                {
                    variants.Add(new VariantRecord(entry.Entity, donor.Id, gene.Id));
                }
            }
        }

        return new Records
        {
            Genes = genes.Values.ToArray(),
            Donors = donors.Values.ToArray(),
            Observations = variants.ToArray()
        };
    }
}
