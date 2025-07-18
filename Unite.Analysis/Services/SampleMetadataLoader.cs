using Unite.Essentials.Extensions;

namespace Unite.Analysis.Services;

public class SampleMetadataLoader
{
    public static SampleMetadata[] Load(SamplesContext context)
    {
        var metadataEntries = new List<SampleMetadata>();

        foreach(var sample in context.OmicsSamples)
        {
            var metadataEntry = new SampleMetadata() { Id = context.GetSampleKey(sample.Key) };

            var donor = context.GetSampleDonor(sample.Key);
            if (donor != null)
            {
                var ageGroups = DefineGroups(context.Donors.Values.Select(donor => donor.ClinicalData?.EnrollmentAge), desiredMin: 0);
                var kpsGroups = DefineGroups(context.Donors.Values.Select(donor => donor.ClinicalData?.Kps), desiredMin: 0);

                metadataEntry.Donor = new DonorMetadata()
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

            var image = context.GetSampleImage(sample.Key);
            if (image != null)
            {
                var wholeTumorGroups = DefineGroups(context.Images.Values.Select(image => image.MrImage?.WholeTumor));
                var contrastEnhancingGroups = DefineGroups(context.Images.Values.Select(image => image.MrImage?.ContrastEnhancing));
                var nonContrastEnhancingGroups = DefineGroups(context.Images.Values.Select(image => image.MrImage?.NonContrastEnhancing));

                metadataEntry.Image = new ImageMetadata()
                {
                    Id = image.ReferenceId,
                    
                    Type = image.TypeId.ToDefinitionString()
                };

                if (image.MrImage != null)
                {
                    metadataEntry.Image.Mr = new MrMetadata()
                    {
                        WholeTumor = ConvertValue(image.MrImage.WholeTumor, wholeTumorGroups),
                        ContrastEnhancing = ConvertValue(image.MrImage.ContrastEnhancing, contrastEnhancingGroups),
                        NonContrastEnhancing = ConvertValue(image.MrImage.NonContrastEnhancing, nonContrastEnhancingGroups),
                    };
                }
            }

            var specimen = context.GetSampleSpecimen(sample.Key);
            if (specimen != null)
            {
                metadataEntry.Specimen = new SpecimenMetadata()
                { 
                    Id = specimen.ReferenceId,

                    Type = specimen.TypeId.ToDefinitionString(),
                    IdhStatus = specimen.MolecularData?.IdhStatusId?.ToDefinitionString(),
                    MgmtStatus = specimen.MolecularData?.MgmtStatusId?.ToDefinitionString()
                };

                if (specimen.Material != null)
                {
                    metadataEntry.Specimen.Material = new MaterialMetadata()
                    {
                        Type = specimen.Material.TypeId?.ToDefinitionString(),
                        TumorType = specimen.Material.TumorTypeId?.ToDefinitionString(),
                        TumorGrade = specimen.Material.TumorGrade?.ToString(),
                        FixationType = specimen.Material.FixationTypeId?.ToDefinitionString(),
                        Source = specimen.Material.Source?.Value
                    };
                }

                if (specimen.Line != null)
                {
                    metadataEntry.Specimen.Line = new LineMetadata()
                    {
                        CellsSpecies = specimen.Line.CellsSpeciesId?.ToDefinitionString(),
                        CellsType = specimen.Line.CellsTypeId?.ToDefinitionString(),
                        CellsCultureType = specimen.Line.CellsCultureTypeId?.ToDefinitionString(),
                    };
                }

                if (specimen.Organoid != null)
                {
                    var cellsNumberGroups = DefineGroups(context.Specimens.Values.Select(specimen => specimen.Organoid?.ImplantedCellsNumber), desiredMin: 0);

                    metadataEntry.Specimen.Organoid = new OrganoidMetadata()
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

                    metadataEntry.Specimen.Xenograft = new XenograftMetadata()
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

            metadataEntries.Add(metadataEntry);
        }

        return metadataEntries.ToArrayOrNull();
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
