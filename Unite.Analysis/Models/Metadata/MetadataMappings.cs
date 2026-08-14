using System.Linq.Expressions;
using Unite.Analysis.Services;

namespace Unite.Analysis.Models.Metadata;

public class MetadataMappings<T> where T : SampleMetadata
{
    public Mapping<T, int> SampleKey => new(MappingKeys.Sample.Key, "Key", entry => entry.Key);
    public Mapping<T, string> SampleId => new(MappingKeys.Sample.Id, "ID", entry => entry.Id);
    
    public IEnumerable<Mapping<T, string>> Donor =>
    [
        For(MappingKeys.Donor.Id, "ID", entry => entry.Donor.Id),
        For(MappingKeys.Donor.Age, "Age", entry => entry.Donor.Age),
        For(MappingKeys.Donor.Sex, "Sex", entry => entry.Donor.Sex),
        For(MappingKeys.Donor.Diagnosis, "Diagnosis", entry => entry.Donor.Diagnosis),
        For(MappingKeys.Donor.DiagnosisPrimarySite, "Diagnosis primary site", entry => entry.Donor.PrimarySite),
        For(MappingKeys.Donor.DiagnosisLocalization, "Diagnosis localization", entry => entry.Donor.Localization),
        For(MappingKeys.Donor.VitalStatus, "Vital status", entry => entry.Donor.VitalStatus),
        For(MappingKeys.Donor.ProgressionStatus, "Progression status", entry => entry.Donor.ProgressionStatus),
        For(MappingKeys.Donor.SteroidsReactive, "Steroids reactive", entry => entry.Donor.SteroidsReactive),
        For(MappingKeys.Donor.Kps, "KPS", entry => entry.Donor.Kps)
    ];

    public IEnumerable<Mapping<T, string>> Image =>
    [
        For(MappingKeys.Image.Id, "ID", entry => entry.Image.Id),
        For(MappingKeys.Image.Type, "Type", entry => entry.Image.Type)
    ];

    public IEnumerable<Mapping<T, string>> ImageMr =>
    [
        For(MappingKeys.Image.Mr.WholeTumor, "Whole tumor", entry => entry.Image.Mr.WholeTumor),
        For(MappingKeys.Image.Mr.ContrastEnhancing, "Contrast enhancing", entry => entry.Image.Mr.ContrastEnhancing),
        For(MappingKeys.Image.Mr.NonContrastEnhancing, "Non contrast enhancing", entry => entry.Image.Mr.NonContrastEnhancing)
    ];

    public IEnumerable<Mapping<T, string>> Specimen =>
    [
        For(MappingKeys.Specimen.Id, "ID", entry => entry.Specimen.Id),
        For(MappingKeys.Specimen.Type, "Type", entry => entry.Specimen.Type),
        For(MappingKeys.Specimen.Category, "Category", entry => entry.Specimen.Category),
        For(MappingKeys.Specimen.TumorType, "Tumor type", entry => entry.Specimen.TumorType),
        For(MappingKeys.Specimen.TumorGrade, "Tumor grade", entry => entry.Specimen.TumorGrade),
        For(MappingKeys.Specimen.TumorSuperfamily, "Tumor superfamily", entry => entry.Specimen.TumorSuperfamily),
        For(MappingKeys.Specimen.TumorFamily, "Tumor family", entry => entry.Specimen.TumorFamily),
        For(MappingKeys.Specimen.TumorClass, "Tumor class", entry => entry.Specimen.TumorClass),
        For(MappingKeys.Specimen.TumorSubclass, "Tumor subclass", entry => entry.Specimen.TumorSubclass),
        For(MappingKeys.Specimen.IdhStatus, "IDH status", entry => entry.Specimen.IdhStatus),
        For(MappingKeys.Specimen.TertStatus, "TERT status", entry => entry.Specimen.TertStatus),
        For(MappingKeys.Specimen.MgmtStatus, "MGMT status", entry => entry.Specimen.MgmtStatus)
    ];

    public IEnumerable<Mapping<T, string>> SpecimenMaterial =>
    [
        For(MappingKeys.Specimen.Material.FixationType, "Fixation type", entry => entry.Specimen.Material.FixationType),
        For(MappingKeys.Specimen.Material.Source, "Source", entry => entry.Specimen.Material.Source)
    ];

    public IEnumerable<Mapping<T, string>> SpecimenLine =>
    [
        For(MappingKeys.Specimen.Line.CellsSpecies, "Cells species", entry => entry.Specimen.Line.CellsSpecies),
        For(MappingKeys.Specimen.Line.CellsType, "Cells type", entry => entry.Specimen.Line.CellsType),
        For(MappingKeys.Specimen.Line.CellsCultureType, "Cells culture type", entry => entry.Specimen.Line.CellsCultureType)
    ];

    public IEnumerable<Mapping<T, string>> SpecimenOrganoid =>
    [
        For(MappingKeys.Specimen.Organoid.Medium, "Medium", entry => entry.Specimen.Organoid.Medium),
        For(MappingKeys.Specimen.Organoid.ImplantedCellsNumber, "Implanted cells number", entry => entry.Specimen.Organoid.ImplantedCellsNumber),
        For(MappingKeys.Specimen.Organoid.Tumorigenicity, "Tumorigenicity", entry => entry.Specimen.Organoid.Tumorigenicity)
    ];

    public IEnumerable<Mapping<T, string>> SpecimenXenograft =>
    [
        For(MappingKeys.Specimen.Xenograft.MouseStrain, "Mouse strain", entry => entry.Specimen.Xenograft.MouseStrain),
        For(MappingKeys.Specimen.Xenograft.GroupSize, "Group size", entry => entry.Specimen.Xenograft.GroupSize),
        For(MappingKeys.Specimen.Xenograft.ImplantType, "Implant type", entry => entry.Specimen.Xenograft.ImplantType),
        For(MappingKeys.Specimen.Xenograft.ImplantLocation, "Implant location", entry => entry.Specimen.Xenograft.ImplantLocation),
        For(MappingKeys.Specimen.Xenograft.ImplantedCellsNumber, "Implanted cells number", entry => entry.Specimen.Xenograft.ImplantedCellsNumber),
        For(MappingKeys.Specimen.Xenograft.Tumorigenicity, "Tumorigenicity", entry => entry.Specimen.Xenograft.Tumorigenicity),
        For(MappingKeys.Specimen.Xenograft.TumorGrowthForm, "Tumor growth form", entry => entry.Specimen.Xenograft.TumorGrowthForm),
        For(MappingKeys.Specimen.Xenograft.SurvivalDays, "Survival days", entry => entry.Specimen.Xenograft.SurvivalDays)
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


    public Mapping(string key, string name, Expression<Func<T, TProp>> expression)
    {
        Key = key;
        Name = name;
        Expression = expression;
    }
}
