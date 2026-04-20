using System.Linq.Expressions;
using Unite.Analysis.Services;

namespace Unite.Analysis.Models.Metadata;

public class Mappings<T> where T : SampleMetadata
{
    public Mapping<T, int> SampleId => new("sample_id", entry => entry.Id);
    public Mapping<T, string> SampleKey => new("sample_key", entry => entry.Key);

    public IEnumerable<Mapping<T, string>> Donor =>
    [
        For("donor_key", "Key", entry => entry.Donor.Id),
        For("donor_age", "Age", entry => entry.Donor.Age),
        For("donor_sex", "Sex", entry => entry.Donor.Sex),
        For("donor_diagnosis", "Diagnosis", entry => entry.Donor.Diagnosis),
        For("donor_diagnosis_primary_site", "Diagnosis primary site", entry => entry.Donor.PrimarySite),
        For("donor_diagnosis_localization", "Diagnosis localization", entry => entry.Donor.Localization),
        For("donor_vital_status", "Vital status", entry => entry.Donor.VitalStatus),
        For("donor_progression_status", "Progression status", entry => entry.Donor.ProgressionStatus),
        For("donor_steroids_reactive", "Steroids reactive", entry => entry.Donor.SteroidsReactive),
        For("donor_kps", "KPS", entry => entry.Donor.Kps)
    ];

    public IEnumerable<Mapping<T, string>> Image =>
    [
        For("image_key", "Key", entry => entry.Image.Id),
        For("image_type", "Type", entry => entry.Image.Type)
    ];

    public IEnumerable<Mapping<T, string>> ImageMr =>
    [
        For("mr_whole_tumor", "Whole tumor", entry => entry.Image.Mr.WholeTumor),
        For("mr_contrast_enhancing", "Contrast enhancing", entry => entry.Image.Mr.ContrastEnhancing),
        For("mr_non_contrast_enhancing", "Non contrast enhancing", entry => entry.Image.Mr.NonContrastEnhancing)
    ];

    public IEnumerable<Mapping<T, string>> Specimen =>
    [
        For("specimen_key", "Key", entry => entry.Specimen.Id),
        For("specimen_type", "Type", entry => entry.Specimen.Type),
        For("specimen_category", "Category", entry => entry.Specimen.Category),
        For("specimen_tumor_type", "Tumor type", entry => entry.Specimen.TumorType),
        For("specimen_tumor_grade", "Tumor grade", entry => entry.Specimen.TumorGrade),
        For("specimen_tumor_superfamily", "Tumor superfamily", entry => entry.Specimen.TumorSuperfamily),
        For("specimen_tumor_family", "Tumor family", entry => entry.Specimen.TumorFamily),
        For("specimen_tumor_class", "Tumor class", entry => entry.Specimen.TumorClass),
        For("specimen_tumor_subclass", "Tumor subclass", entry => entry.Specimen.TumorSubclass),
        For("specimen_idh_status", "IDH status", entry => entry.Specimen.IdhStatus),
        For("specimen_tert_status", "TERT status", entry => entry.Specimen.TertStatus),
        For("specimen_mgmt_status", "MGMT status", entry => entry.Specimen.MgmtStatus)
    ];

    public IEnumerable<Mapping<T, string>> SpecimenMaterial =>
    [
        For("material_fixation_type", "Fixation type", entry => entry.Specimen.Material.FixationType),
        For("material_source", "Source", entry => entry.Specimen.Material.Source)
    ];

    public IEnumerable<Mapping<T, string>> SpecimenLine =>
    [
        For("line_cells_species", "Cells species", entry => entry.Specimen.Line.CellsSpecies),
        For("line_cells_type", "Cells type", entry => entry.Specimen.Line.CellsType),
        For("line_cells_culture_type", "Cells culture type", entry => entry.Specimen.Line.CellsCultureType)
    ];

    public IEnumerable<Mapping<T, string>> SpecimenOrganoid =>
    [
        For("organoid_medium", "Medium", entry => entry.Specimen.Organoid.Medium),
        For("organoid_implanted_cells_number", "Implanted cells number", entry => entry.Specimen.Organoid.ImplantedCellsNumber),
        For("organoid_tumorigenicity", "Tumorigenicity", entry => entry.Specimen.Organoid.Tumorigenicity)
    ];

    public IEnumerable<Mapping<T, string>> SpecimenXenograft =>
    [
        For("xenograft_mouse_strain", "Mouse strain", entry => entry.Specimen.Xenograft.MouseStrain),
        For("xenograft_group_size", "Group size", entry => entry.Specimen.Xenograft.GroupSize),
        For("xenograft_implant_type", "Implant type", entry => entry.Specimen.Xenograft.ImplantType),
        For("xenograft_implant_location", "Implant location", entry => entry.Specimen.Xenograft.ImplantLocation),
        For("xenograft_implanted_cells_number", "Implanted cells number", entry => entry.Specimen.Xenograft.ImplantedCellsNumber),
        For("xenograft_tumorigenicity", "Tumorigenicity", entry => entry.Specimen.Xenograft.Tumorigenicity),
        For("xenograft_tumor_growth_form", "Tumor growth form", entry => entry.Specimen.Xenograft.TumorGrowthForm),
        For("xenograft_survival_days", "Survival days", entry => entry.Specimen.Xenograft.SurvivalDays)
    ];

    public IEnumerable<Mapping<T, string>> All =>
    [
        ..Donor,
        ..Image,
        ..ImageMr,
        ..Specimen,
        ..SpecimenMaterial,
        ..SpecimenLine,
        ..SpecimenOrganoid,
        ..SpecimenXenograft
    ];

    private static Mapping<T, TProp> For<TProp>(string key, string name, Expression<Func<T, TProp>> expression)
    {
        return new Mapping<T, TProp>(key, name, expression);
    }
}

public class Mapping<T, TProp> where T : class
{
    public string Key { get; private set; }
    public string Name { get; private set; }
    public Expression<Func<T, TProp>> Expression { get; private set; }


    public Mapping(string key, Expression<Func<T, TProp>> expression)
    {
        Key = key;
        Name = key;
        Expression = expression;
    }

    public Mapping(string key, string name, Expression<Func<T, TProp>> expression)
    {
        Key = key;
        Name = name;
        Expression = expression;
    }
}
