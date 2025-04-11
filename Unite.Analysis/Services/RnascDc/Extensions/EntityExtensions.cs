using Unite.Analysis.Services.RnascDc.Models.Context;

namespace Unite.Analysis.Services.RnascDc.Extensions;

public static class EntityExtensions
{
    public static string GetKey(this Data.Entities.Genome.Analysis.Sample sample, AnalysisContext context)
    {
        if (context.SampleType == 1)
        {
            var donor = sample.GetDonor(context);
            var specimen = sample.GetSpecimen(context);
            return $"{donor.ReferenceId}-{specimen.ReferenceId}";
        }
        else if (context.SampleType == 2)
        {
            var image = sample.GetImage(context);
            var specimen = sample.GetSpecimen(context);
            return $"{image.ReferenceId}-{specimen.ReferenceId}";
        }
        else if (context.SampleType == 3)
        {
            var specimen = sample.GetSpecimen(context);
            return $"{specimen.ReferenceId}";
        }
        else
        {
            throw new InvalidOperationException("Invalid sample type.");
        }
    }

    public static Data.Entities.Donors.Donor GetDonor(this Data.Entities.Genome.Analysis.Sample sample, AnalysisContext context)
    {
        var specimen = context.Specimens[sample.SpecimenId];

        return context.Donors.TryGetValue(specimen.DonorId, out var donor) ? donor : null;  
    }

    public static Data.Entities.Images.Image GetImage(this Data.Entities.Genome.Analysis.Sample sample, AnalysisContext context)
    {
        var specimen = context.Specimens[sample.SpecimenId];

        return context.Images.Values.FirstOrDefault(image => image.DonorId == specimen.DonorId);
    }

    public static Data.Entities.Specimens.Specimen GetSpecimen(this Data.Entities.Genome.Analysis.Sample sample, AnalysisContext context)
    {
        return context.Specimens[sample.SpecimenId];
    }

    public static int GetRelevance(this Data.Entities.Specimens.Specimen specimen)
    {
        return (specimen.TypeId == Data.Entities.Specimens.Enums.SpecimenType.Material && specimen.Material.TumorTypeId == Data.Entities.Specimens.Materials.Enums.TumorType.Primary) ? 1 :
               (specimen.TypeId == Data.Entities.Specimens.Enums.SpecimenType.Material && specimen.Material.TumorTypeId == Data.Entities.Specimens.Materials.Enums.TumorType.Recurrent) ? 2 :
               (specimen.TypeId == Data.Entities.Specimens.Enums.SpecimenType.Material && specimen.Material.TumorTypeId == Data.Entities.Specimens.Materials.Enums.TumorType.Metastasis) ? 3 :
               (specimen.TypeId == Data.Entities.Specimens.Enums.SpecimenType.Material && specimen.Material.TypeId == Data.Entities.Specimens.Materials.Enums.MaterialType.Tumor) ? 4 :
               (specimen.TypeId == Data.Entities.Specimens.Enums.SpecimenType.Material && specimen.Material.TypeId == Data.Entities.Specimens.Materials.Enums.MaterialType.Normal) ? 5 :
               specimen.TypeId == Data.Entities.Specimens.Enums.SpecimenType.Material ? 6 :
               specimen.TypeId == Data.Entities.Specimens.Enums.SpecimenType.Line ? 10 :
               specimen.TypeId == Data.Entities.Specimens.Enums.SpecimenType.Organoid ? 20 :
               specimen.TypeId == Data.Entities.Specimens.Enums.SpecimenType.Xenograft ? 30 : 40;
    }
}
