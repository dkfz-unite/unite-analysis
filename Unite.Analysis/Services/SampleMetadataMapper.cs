using System.Linq.Expressions;
using Unite.Essentials.Tsv;
using Unite.Essentials.Tsv.Converters;

namespace Unite.Analysis.Services;

public class SampleMetadataMapper
{
    private static readonly NullableValueConverter _nullableValueConverter = new();


    public static ClassMap<T> Map<T>(T[] entries, bool mapId = false) where T : SampleMetadata
    {
        var map = new ClassMap<T>();

        MapEntries(map, entries, mapId);

        return map;
    }

    public static ClassMap<SampleMetadata> Map(SampleMetadata[] entries, bool mapId = false)
    {
        var map = new ClassMap<SampleMetadata>();

        MapEntries(map, entries, mapId);

        return map;
    }

    private static void MapEntries<T>(ClassMap<T> map, T[] entries, bool mapId = false)
        where T : SampleMetadata
    {
        if (mapId)
            map.Map(entry => entry.Id, "sample_id");

        MapProperty(map, entries, entry => entry.Key, "sample_key");

        if (entries.Any(entry => entry.Donor != null))
        {
            MapProperty(map, entries, entry => entry.Donor.Id, "donor_key");

            MapProperty(map, entries, entry => entry.Donor.Age, "donor_age");
            MapProperty(map, entries, entry => entry.Donor.Sex, "donor_sex");
            MapProperty(map, entries, entry => entry.Donor.Diagnosis, "donor_diagnosis");
            MapProperty(map, entries, entry => entry.Donor.PrimarySite, "donor_diagnosis_primary_site");
            MapProperty(map, entries, entry => entry.Donor.Localization, "donor_diagnosis_localization");
            MapProperty(map, entries, entry => entry.Donor.VitalStatus, "donor_vital_status");
            MapProperty(map, entries, entry => entry.Donor.ProgressionStatus, "donor_progression_status");
            MapProperty(map, entries, entry => entry.Donor.SteroidsReactive, "donor_steroids_reactive");
            MapProperty(map, entries, entry => entry.Donor.Kps, "donor_kps");
        }

        if (entries.Any(entry => entry.Image != null))
        {
            MapProperty(map, entries, entry => entry.Image.Id, "image_key");

            MapProperty(map, entries, entry => entry.Image.Type, "image_type");

            if (entries.Any(entry => entry.Image.Mr != null))
            {
                MapProperty(map, entries, entry => entry.Image.Mr.WholeTumor, "mr_whole_tumor");
                MapProperty(map, entries, entry => entry.Image.Mr.ContrastEnhancing, "mr_contrast_enhancing");
                MapProperty(map, entries, entry => entry.Image.Mr.NonContrastEnhancing, "mr_non_contrast_enhancing");
            }
        }

        if (entries.Any(entry => entry.Specimen != null))
        {
            // TODO: Map tumor classification scores.
            MapProperty(map, entries, entry => entry.Specimen.Id, "specimen_key");

            MapProperty(map, entries, entry => entry.Specimen.Type, "specimen_type");
            MapProperty(map, entries, entry => entry.Specimen.Category, "specimen_condition");
            MapProperty(map, entries, entry => entry.Specimen.TumorType, "specimen_tumor_type");
            MapProperty(map, entries, entry => entry.Specimen.TumorGrade, "specimen_tumor_grade");
            MapProperty(map, entries, entry => entry.Specimen.TumorSuperfamily, "specimen_tumor_superfamily");
            MapProperty(map, entries, entry => entry.Specimen.TumorFamily, "specimen_tumor_family");
            MapProperty(map, entries, entry => entry.Specimen.TumorClass, "specimen_tumor_class");
            MapProperty(map, entries, entry => entry.Specimen.TumorSubclass, "specimen_tumor_subclass");
            MapProperty(map, entries, entry => entry.Specimen.IdhStatus, "specimen_idh_status");
            MapProperty(map, entries, entry => entry.Specimen.TertStatus, "specimen_tert_status");
            MapProperty(map, entries, entry => entry.Specimen.MgmtStatus, "specimen_mgmt_status");

            if (entries.Any(entry => entry.Specimen.Material != null))
            {
                MapProperty(map, entries, entry => entry.Specimen.Material.FixationType, "material_fixation_type");
                MapProperty(map, entries, entry => entry.Specimen.Material.Source, "material_source");
            }

            if (entries.Any(entry => entry.Specimen.Line != null))
            {
                MapProperty(map, entries, entry => entry.Specimen.Line.CellsSpecies, "cell_line_cells_species");
                MapProperty(map, entries, entry => entry.Specimen.Line.CellsType, "cell_line_cells_type");
                MapProperty(map, entries, entry => entry.Specimen.Line.CellsCultureType, "cell_line_cells_culture_type");
            }

            if (entries.Any(entry => entry.Specimen.Organoid != null))
            {
                MapProperty(map, entries, entry => entry.Specimen.Organoid.Medium, "organoid_medium");
                MapProperty(map, entries, entry => entry.Specimen.Organoid.ImplantedCellsNumber, "organoid_implanted_cells_number");
                MapProperty(map, entries, entry => entry.Specimen.Organoid.Tumorigenicity, "organoid_tumorigenicity");
            }

            if (entries.Any(entry => entry.Specimen.Xenograft != null))
            {
                MapProperty(map, entries, entry => entry.Specimen.Xenograft.MouseStrain, "xenograft_mouse_strain");
                MapProperty(map, entries, entry => entry.Specimen.Xenograft.GroupSize, "xenograft_group_size");
                MapProperty(map, entries, entry => entry.Specimen.Xenograft.ImplantType, "xenograft_implant_type");
                MapProperty(map, entries, entry => entry.Specimen.Xenograft.ImplantLocation, "xenograft_implant_location");
                MapProperty(map, entries, entry => entry.Specimen.Xenograft.ImplantedCellsNumber, "xenograft_implanted_cells_number");
                MapProperty(map, entries, entry => entry.Specimen.Xenograft.Tumorigenicity, "xenograft_tumorigenicity");
                MapProperty(map, entries, entry => entry.Specimen.Xenograft.TumorGrowthForm, "xenograft_tumor_growth_form");
                MapProperty(map, entries, entry => entry.Specimen.Xenograft.SurvivalDays, "xenograft_survival_days");
            }
        }
    }


    private static void MapProperty<T>(ClassMap<T> map, T[] entries, Expression<Func<T, string>> property, string header)
        where T : SampleMetadata
    {
        var getter = property.Compile();

        if (entries.Any(entry => getter(entry) != null))
        {
            map.Map(property, header, _nullableValueConverter);
        }
    }
}

public class NullableValueConverter : IConverter<string>
{
    public object Convert(string value, string row)
    {
        throw new NotImplementedException();
    }

    public string Convert(object value, object row)
    {
        var stringValue = value as string;

        return stringValue ?? "Unknown";
    }
}
