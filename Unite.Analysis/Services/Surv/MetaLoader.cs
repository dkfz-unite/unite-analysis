using Unite.Analysis.Services.Surv.Models.Context;
using Unite.Analysis.Services.Surv.Models.Criteria;
using Unite.Analysis.Services.Surv.Models.Input;

namespace Unite.Analysis.Services.Surv;

internal static class MetaLoader
{
    public static Metadata[] Load(AnalysisContext context, Options options)
    {
        var entries = Load(context, options.Progression).Filter().ToArray();

        Validate(entries);

        return entries;
    }


    private static IEnumerable<Metadata> Load(AnalysisContext context, bool progression)
    {
        foreach (var dataset in context.Datasets)
        {
            foreach (var donor in dataset.Donors)
            {
                yield return new Metadata
                {
                    DonorId = donor.ReferenceId,
                    DatasetId = dataset.Name,
                    EnrolmentDate = donor.ClinicalData?.EnrollmentDate,
                    Status = progression ? donor.ClinicalData?.ProgressionStatus : !donor.ClinicalData?.VitalStatus,
                    StatusChangeDate = progression ? donor.ClinicalData?.ProgressionStatusChangeDate : donor.ClinicalData?.VitalStatusChangeDate,
                    StatusChangeDay = progression ? donor.ClinicalData?.ProgressionStatusChangeDay : donor.ClinicalData?.VitalStatusChangeDay
                };
            }
        }
    }

    private static IEnumerable<Metadata> Filter(this IEnumerable<Metadata> entries)
    {
        return entries.Where(entry =>
        {
            var hasStatus = entry.Status != null;
            var hasStatusChangeDate = entry.EnrolmentDate != null && entry.StatusChangeDate != null;
            var hasStatusChangeDay = entry.StatusChangeDay != null;

            return hasStatus && (hasStatusChangeDate || hasStatusChangeDay);
        });
    }

    private static void Validate(this IEnumerable<Metadata> entries)
    {
        if (entries.Count() < 2)
            throw new InvalidOperationException("The dataset requires at least two unique donors with survival estimation.");
    }
}
