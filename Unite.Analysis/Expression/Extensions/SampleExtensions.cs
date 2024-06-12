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
    public static Task<Sample> PickOrDefaultAsync(this IQueryable<Sample> query)
    {
        var ordered = query.OrderBy(sample =>
            (sample.Specimen.TypeId == SpecimenType.Material && sample.Specimen.Material.TumorTypeId == TumorType.Primary) ? 1 :
            (sample.Specimen.TypeId == SpecimenType.Material && sample.Specimen.Material.TumorTypeId == TumorType.Recurrent) ? 2 :
            (sample.Specimen.TypeId == SpecimenType.Material && sample.Specimen.Material.TumorTypeId == TumorType.Metastasis) ? 3 :
            (sample.Specimen.TypeId == SpecimenType.Material && sample.Specimen.Material.TypeId == MaterialType.Tumor) ? 4 :
            (sample.Specimen.TypeId == SpecimenType.Material && sample.Specimen.Material.TypeId == MaterialType.Normal) ? 5 :
            sample.Specimen.TypeId == SpecimenType.Material ? 6 :
            sample.Specimen.TypeId == SpecimenType.Line ? 10 :
            sample.Specimen.TypeId == SpecimenType.Organoid ? 20 :
            sample.Specimen.TypeId == SpecimenType.Xenograft ? 30 : 40
        );

        return query.FirstOrDefaultAsync();
    }
}
