using Unite.Data.Entities.Omics.Analysis;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Services;

public class SampleMetadataLoader
{
    public static T[] Load<T>(SamplesContext context) where T : SampleMetadata, new()
    {
        var entries = new List<T>();

        foreach (var sample in context.OmicsSamples)
        {
            var entry = new T() { Id = context.GetSampleKey(sample.Key) };

            MapEntry(sample.Value, entry, context);

            entries.Add(entry);
        }

        return entries.ToArrayOrNull();
    }

    public static T[] Load<T>(SamplesContext context, Func<Sample, T, SamplesContext, bool> mapper) where T : SampleMetadata, new()
    {
        var entries = new List<T>();

        foreach (var sample in context.OmicsSamples)
        {
            var entry = new T() { Id = context.GetSampleKey(sample.Key) };

            var mapped = mapper(sample.Value, entry, context);

            if (mapped)
            {
                MapEntry(sample.Value, entry, context);

                entries.Add(entry);
            }
        }

        return entries.ToArrayOrNull();
    }

    public static SampleMetadata[] Load(SamplesContext context)
    {
        return Load<SampleMetadata>(context);
    }

    public static SampleMetadata[] Load(SamplesContext context, Func<Sample, SampleMetadata, SamplesContext, bool> mapper)
    {
        return Load<SampleMetadata>(context, mapper);
    }


    private static void MapEntry(in Sample sample, in SampleMetadata entry, in SamplesContext context)
    {
        var donor = context.GetSampleDonor(sample.Id);
        if (donor != null)
        {
            var ageGroups = DefineGroups(context.Donors.Values.Select(donor => donor.ClinicalData?.EnrollmentAge), desiredMin: 0);
            var kpsGroups = DefineGroups(context.Donors.Values.Select(donor => donor.ClinicalData?.Kps), desiredMin: 0);

            entry.Donor = new DonorMetadata()
            {
                Id = donor.ReferenceId.ToString(),

                Sex = donor.ClinicalData?.SexId?.ToDefinitionString(),
                Age = ConvertValue(donor.ClinicalData?.EnrollmentAge, ageGroups),
                Diagnosis = donor.ClinicalData?.Diagnosis,
                PrimarySite = donor.ClinicalData?.PrimarySite?.Value,
                Localization = donor.ClinicalData?.Localization?.Value,
                VitalStatus = ConvertValue(donor.ClinicalData?.VitalStatus),
                ProgressionStatus = ConvertValue(donor.ClinicalData?.ProgressionStatus),
                SteroidsReactive = ConvertValue(donor.ClinicalData?.SteroidsReactive),
                Kps = ConvertValue(donor.ClinicalData?.Kps, kpsGroups),
            };
        }

        var image = context.GetSampleImage(sample.Id);
        if (image != null)
        {
            var wholeTumorGroups = DefineGroups(context.Images.Values.Select(image => image.MrImage?.WholeTumor));
            var contrastEnhancingGroups = DefineGroups(context.Images.Values.Select(image => image.MrImage?.ContrastEnhancing));
            var nonContrastEnhancingGroups = DefineGroups(context.Images.Values.Select(image => image.MrImage?.NonContrastEnhancing));

            entry.Image = new ImageMetadata()
            {
                Id = image.ReferenceId,

                Type = image.TypeId.ToDefinitionString()
            };

            if (image.MrImage != null)
            {
                entry.Image.Mr = new MrMetadata()
                {
                    WholeTumor = ConvertValue(image.MrImage.WholeTumor, wholeTumorGroups),
                    ContrastEnhancing = ConvertValue(image.MrImage.ContrastEnhancing, contrastEnhancingGroups),
                    NonContrastEnhancing = ConvertValue(image.MrImage.NonContrastEnhancing, nonContrastEnhancingGroups)
                };
            }
        }

        var specimen = context.GetSampleSpecimen(sample.Id);
        if (specimen != null)
        {
            entry.Specimen = new SpecimenMetadata()
            {
                Id = specimen.ReferenceId,

                Type = specimen.TypeId.ToDefinitionString(),
                Condition = specimen.CategoryId?.ToDefinitionString(),
                TumorType = specimen.TumorTypeId?.ToDefinitionString(),
                TumorGrade = specimen.TumorGrade?.ToString(),
                TumorSuperfamily = specimen.TumorClassification?.Superfamily?.Name,
                TumorFamily = specimen.TumorClassification?.Family?.Name,
                TumorClass = specimen.TumorClassification?.Class?.Name,
                TumorSubclass = specimen.TumorClassification?.Subclass?.Name,
                IdhStatus = ConvertValue(specimen.MolecularData?.IdhStatus, "Mutant", "Wild Type"),
                TertStatus = ConvertValue(specimen.MolecularData?.TertStatus, "Mutant", "Wild Type"),
                MgmtStatus = ConvertValue(specimen.MolecularData?.MgmtStatus, "Methylated", "Unmethylated"),
                GeneKnockouts = string.Join(", ", specimen.MolecularData?.GeneKnockouts ?? [])
            };

            if (specimen.Material != null)
            {
                entry.Specimen.Material = new MaterialMetadata()
                {
                    FixationType = specimen.Material.FixationTypeId?.ToDefinitionString(),
                    Source = specimen.Material.Source?.Value
                };
            }

            if (specimen.Line != null)
            {
                entry.Specimen.Line = new LineMetadata()
                {
                    CellsSpecies = specimen.Line.CellsSpeciesId?.ToDefinitionString(),
                    CellsType = specimen.Line.CellsTypeId?.ToDefinitionString(),
                    CellsCultureType = specimen.Line.CellsCultureTypeId?.ToDefinitionString(),
                };
            }

            if (specimen.Organoid != null)
            {
                var cellsNumberGroups = DefineGroups(context.Specimens.Values.Select(specimen => specimen.Organoid?.ImplantedCellsNumber), desiredMin: 0);

                entry.Specimen.Organoid = new OrganoidMetadata()
                {
                    Medium = specimen.Organoid.Medium,
                    ImplantedCellsNumber = ConvertValue(specimen.Organoid.ImplantedCellsNumber, cellsNumberGroups),
                    Tumorigenicity = ConvertValue(specimen.Organoid.Tumorigenicity)
                };
            }

            if (specimen.Xenograft != null)
            {
                var groupSizeGroups = DefineGroups(context.Specimens.Values.Select(specimen => specimen.Xenograft?.GroupSize), desiredMin: 0);
                var cellsNumberGroups = DefineGroups(context.Specimens.Values.Select(specimen => specimen.Xenograft?.ImplantedCellsNumber), desiredMin: 0);

                entry.Specimen.Xenograft = new XenograftMetadata()
                {
                    MouseStrain = specimen.Xenograft.MouseStrain,
                    GroupSize = ConvertValue(specimen.Xenograft.GroupSize, groupSizeGroups),
                    ImplantType = specimen.Xenograft.ImplantTypeId?.ToDefinitionString(),
                    ImplantLocation = specimen.Xenograft.ImplantLocationId?.ToDefinitionString(),
                    ImplantedCellsNumber = ConvertValue(specimen.Xenograft.ImplantedCellsNumber, cellsNumberGroups),
                    Tumorigenicity = ConvertValue(specimen.Xenograft.Tumorigenicity),
                    TumorGrowthForm = specimen.Xenograft.TumorGrowthFormId?.ToDefinitionString(),
                    SurvivalDays = ConvertValue(specimen.Xenograft.SurvivalDaysFrom, specimen.Xenograft.SurvivalDaysTo)
                };
            }
        }
    }


    private static string ConvertValue(bool? value, string trueValue = "Yes", string falseValue = "No")
    {
        return value switch
        {
            true => trueValue,
            false => falseValue,
            _ => null
        };
    }
    
    private static string ConvertValue(int? from, int? to)
    {
        if (from == null && to == null)
            return null;
        else if (from == null)
            return to.ToString();
        else if (to == null)
            return from.ToString();
        else
            return $"{from}-{to}";
    }

    public static string ConvertValue<T>(int? value, int[] groups)
    {
        if (value <= groups[0])
            return $"<{groups[0]}";
        else if (value <= groups[1])
            return $"{groups[0] + 1}-{groups[1]}";
        else if (value < groups[2])
            return $"{groups[1] + 1}-{groups[2]}";
        else if (value < groups[3])
            return $"{groups[2] + 1}-{groups[3]}";
        else if (value >= groups[3])
            return $">{groups[3] + 1}";
        else
            return null;
    }

    public static string ConvertValue(double? value, int[] groups)
    {
        if (value <= groups[0])
            return $"<{groups[0]}";
        else if (value <= groups[1])
            return $"{groups[0] + 1}-{groups[1]}";
        else if (value < groups[2])
            return $"{groups[1] + 1}-{groups[2]}";
        else if (value < groups[3])
            return $"{groups[2] + 1}-{groups[3]}";
        else if (value >= groups[3])
            return $">{groups[3] + 1}";
        else
            return null;
    }

    private static int[] DefineGroups(IEnumerable<int?> values, int? desiredMin = 0, int? desiredMax = null)
    {
        if (values.IsEmpty())
            return [0];

        var min = desiredMin ?? values.Select(value => value ?? 0).Min();
        var max = desiredMax ?? values.Select(value => value ?? 0).Max();

        var step = (max - min) / 5;

       return [min + step, min + step * 2, min + step * 3, min + step * 4];
    }

    private static int[] DefineGroups(IEnumerable<double?> values, int? desiredMin = null, int? desiredMax = null)
    {
        if (values.IsEmpty())
            return [0];

        var min = desiredMin ?? (int)values.Select(value => value ?? 0).Min();
        var max = desiredMax ?? (int)values.Select(value => value ?? 0).Max();

        var step = (max - min) / 5;

       return [min + step, min + step * 2, min + step * 3, min + step * 4];
    }
}
