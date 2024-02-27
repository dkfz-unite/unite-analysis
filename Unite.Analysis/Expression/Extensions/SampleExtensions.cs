using Microsoft.EntityFrameworkCore;
using Unite.Data.Entities.Genome.Analysis;
using Unite.Data.Entities.Specimens.Enums;
using Unite.Data.Entities.Specimens.Materials.Enums;

namespace Unite.Analysis.Expression.Extensions;

internal static class SampleExtensions
{
    /// <summary>
    /// Choses the most relevant sample from the query.
    /// </summary>
    /// <param name="query">Source query.</param>
    /// <returns>Most appropriate sample or null if not found.</returns>
    public static Task<AnalysedSample> PickOrDefaultAsync(this IQueryable<AnalysedSample> query)
    {
        var ordered = query.OrderBy(sample =>
            (sample.TargetSample.TypeId == SpecimenType.Material && sample.TargetSample.Material.TumorTypeId == TumorType.Primary) ? 1 :
            (sample.TargetSample.TypeId == SpecimenType.Material && sample.TargetSample.Material.TumorTypeId == TumorType.Recurrent) ? 2 :
            (sample.TargetSample.TypeId == SpecimenType.Material && sample.TargetSample.Material.TumorTypeId == TumorType.Metastasis) ? 3 :
            (sample.TargetSample.TypeId == SpecimenType.Material && sample.TargetSample.Material.TypeId == MaterialType.Tumor) ? 4 :
            (sample.TargetSample.TypeId == SpecimenType.Material && sample.TargetSample.Material.TypeId == MaterialType.Normal) ? 5 :
            sample.TargetSample.TypeId == SpecimenType.Material ? 6 :
            sample.TargetSample.TypeId == SpecimenType.Line ? 10 :
            sample.TargetSample.TypeId == SpecimenType.Organoid ? 20 :
            sample.TargetSample.TypeId == SpecimenType.Xenograft ? 30 : 40
        );

        return query.FirstOrDefaultAsync();
    }
}
