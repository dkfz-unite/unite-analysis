using Microsoft.EntityFrameworkCore;
using Unite.Data.Context;
using Unite.Data.Context.Repositories;
using Unite.Data.Entities.Genome.Analysis;

namespace Unite.Analysis.Extensions;

public static class RepositoryExtensions
{
    public static async Task<int[]> GetRelatedSamples(this SpecimensRepository repository, int[] specimenIds, IDbContextFactory<DomainDbContext> dbContextFactory)
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        return await dbContext.Set<Sample>()
            .AsNoTracking()
            .Where(sample => specimenIds.Contains(sample.SpecimenId))
            .Select(sample => sample.Id)
            .Distinct()
            .ToArrayAsync();
    }
}
