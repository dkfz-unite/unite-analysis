using Unite.Analysis.Services.SCell.Extensions;
using Unite.Analysis.Services.SCell.Models;
using Unite.Analysis.Services.SCell.Models.Context;
using Unite.Analysis.Services.SCell.Models.Data;
using Unite.Essentials.Extensions;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.SCell;

public class MetaLoader
{
    public static async Task PrepareMetadata(AnalysisContext context, string workingDirectoryPath)
    {
        var entries = Load(context);

        var map = MetaMapper.Map(entries);

        var tsv = TsvWriter.Write(entries, map);

        File.WriteAllText(Path.Combine(workingDirectoryPath, "metadata.tsv"), tsv);

        await Task.CompletedTask;
    }


    private static Metadata[] Load(AnalysisContext context)
    {
        var metadataEntries = new List<Metadata>();

        foreach(var sample in context.Samples)
        {
            var metadataEntry = new Metadata() { Id = sample.Value.GetKey(context) };

            var donor = sample.Value.GetDonor(context);
            if (donor != null)
            {
                var ageGroups = DefineGroups(context.Donors.Values.Select(donor => donor.ClinicalData?.Age), desiredMin: 0);
                var kpsGroups = DefineGroups(context.Donors.Values.Select(donor => donor.ClinicalData?.KpsBaseline), desiredMin: 0);

                metadataEntry.Donor = new DonorMetadata()
                {
                    Id = donor.ReferenceId.ToString(),

                    Sex = donor.ClinicalData?.GenderId?.ToDefinitionString(),
                    Age = ConvertValue(donor.ClinicalData?.Age, ageGroups),
                    Diagnosis = donor.ClinicalData?.Diagnosis,
                    PrimarySite = donor.ClinicalData?.PrimarySite?.Value,
                    Localization = donor.ClinicalData?.Localization?.Value,
                    VitalStatus = ConvertValue(donor.ClinicalData?.VitalStatus),
                    ProgressionStatus = ConvertValue(donor.ClinicalData?.ProgressionStatus),
                    SteroidsBaseline = ConvertValue(donor.ClinicalData?.SteroidsBaseline),
                    KpsBaseline = ConvertValue(donor.ClinicalData?.KpsBaseline, kpsGroups),
                };
            }

            var image = sample.Value.GetImage(context);
            if (image != null)
            {
                var wholeTumorGroups = DefineGroups(context.Images.Values.Select(image => image.MriImage?.WholeTumor));
                var contrastEnhancingGroups = DefineGroups(context.Images.Values.Select(image => image.MriImage?.ContrastEnhancing));
                var nonContrastEnhancingGroups = DefineGroups(context.Images.Values.Select(image => image.MriImage?.NonContrastEnhancing));

                metadataEntry.Image = new ImageMetadata()
                {
                    Id = image.ReferenceId,
                    
                    Type = image.TypeId.ToDefinitionString()
                };

                if (image.MriImage != null)
                {
                    metadataEntry.Image.Mri = new MriMetadata()
                    {
                        WholeTumor = ConvertValue(image.MriImage.WholeTumor, wholeTumorGroups),
                        ContrastEnhancing = ConvertValue(image.MriImage.ContrastEnhancing, contrastEnhancingGroups),
                        NonContrastEnhancing = ConvertValue(image.MriImage.NonContrastEnhancing, nonContrastEnhancingGroups),
                    };
                }
            }

            var specimen = sample.Value.GetSpecimen(context);
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
