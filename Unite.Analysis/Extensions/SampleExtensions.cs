using Microsoft.EntityFrameworkCore;
using Unite.Data.Entities.Omics.Analysis;
using Unite.Data.Entities.Specimens.Enums;

namespace Unite.Analysis.Extensions;

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
            (sample.Specimen.TypeId == SpecimenType.Material && sample.Specimen.TumorTypeId == TumorType.Primary) ? 1 :
            (sample.Specimen.TypeId == SpecimenType.Material && sample.Specimen.TumorTypeId == TumorType.Recurrent) ? 2 :
            (sample.Specimen.TypeId == SpecimenType.Material && sample.Specimen.TumorTypeId == TumorType.Metastasis) ? 3 :
            (sample.Specimen.TypeId == SpecimenType.Material && sample.Specimen.ConditionId == Condition.Tumor) ? 4 :
            (sample.Specimen.TypeId == SpecimenType.Material && sample.Specimen.ConditionId == Condition.Normal) ? 5 :
            sample.Specimen.TypeId == SpecimenType.Material ? 6 :
            sample.Specimen.TypeId == SpecimenType.Line ? 10 :
            sample.Specimen.TypeId == SpecimenType.Organoid ? 20 :
            sample.Specimen.TypeId == SpecimenType.Xenograft ? 30 : 40
        );

        return query.FirstOrDefaultAsync();
    }
}
