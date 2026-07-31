using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Unite.Data.Context;
using Unite.Data.Entities.Donors;
using Unite.Data.Entities.Donors.Clinical.Enums;
using Unite.Data.Entities.Images;
using Unite.Data.Entities.Images.Enums;
using Unite.Data.Entities.Specimens;
using Unite.Data.Entities.Specimens.Enums;
using Unite.Data.Entities.Specimens.Lines.Enums;
using Unite.Data.Entities.Specimens.Materials.Enums;
using Unite.Data.Entities.Specimens.Xenografts.Enums;

namespace Unite.Analysis.Models.Metadata;

public class DataMappings
{
    public IEnumerable<IDataMapping> Donor =>
    [
        For<Donor, int>(MappingKeys.Donor.Id, entry => entry.Id),

        // For<Donor, int?>(MappingKeys.Donor.Age, entry => entry.ClinicalData.EnrollmentAge),
        For<Donor, Sex?>(MappingKeys.Donor.Sex, entry => entry.ClinicalData.SexId),
        For<Donor, string>(MappingKeys.Donor.Diagnosis, entry => entry.ClinicalData.Diagnosis),
        For<Donor, string>(MappingKeys.Donor.DiagnosisPrimarySite, entry => entry.ClinicalData.PrimarySite.Value),
        For<Donor, string>(MappingKeys.Donor.DiagnosisLocalization, entry => entry.ClinicalData.Localization.Value),
        For<Donor, bool?>(MappingKeys.Donor.VitalStatus, entry => entry.ClinicalData.VitalStatus),
        For<Donor, bool?>(MappingKeys.Donor.ProgressionStatus, entry => entry.ClinicalData.ProgressionStatus),
        For<Donor, bool?>(MappingKeys.Donor.SteroidsReactive, entry => entry.ClinicalData.SteroidsReactive),
        For<Donor, int?>(MappingKeys.Donor.Kps, entry => entry.ClinicalData.Kps)
    ];

    public IEnumerable<IDataMapping> Image =>
    [
        For<Image, int>(MappingKeys.Image.Id, entry => entry.Id),
        For<Image, ImageType?>(MappingKeys.Image.Type, entry => entry.TypeId),

        // For<Image, double?>(MappingKeys.Image.Mr.WholeTumor, entry => entry.MrImage.WholeTumor),
        // For<Image, double?>(MappingKeys.Image.Mr.ContrastEnhancing, entry => entry.MrImage.ContrastEnhancing),
        // For<Image, double?>(MappingKeys.Image.Mr.NonContrastEnhancing, entry => entry.MrImage.NonContrastEnhancing)
    ];

    public IEnumerable<IDataMapping> Specimen =>
    [
        For<Specimen, int>(MappingKeys.Specimen.Id, entry => entry.Id),
        For<Specimen, SpecimenType?>(MappingKeys.Specimen.Type, entry => entry.TypeId),
        
        For<Specimen, Category?>(MappingKeys.Specimen.Category, entry => entry.CategoryId),
        For<Specimen, TumorType?>(MappingKeys.Specimen.TumorType, entry => entry.TumorTypeId),
        For<Specimen, int?>(MappingKeys.Specimen.TumorGrade, entry => entry.TumorGrade),
        For<Specimen, string>(MappingKeys.Specimen.TumorSuperfamily, entry => entry.TumorClassification.Superfamily.Name),
        For<Specimen, string>(MappingKeys.Specimen.TumorFamily, entry => entry.TumorClassification.Family.Name),
        For<Specimen, string>(MappingKeys.Specimen.TumorClass, entry => entry.TumorClassification.Class.Name),
        For<Specimen, string>(MappingKeys.Specimen.TumorSubclass, entry => entry.TumorClassification.Subclass.Name),
        For<Specimen, bool?>(MappingKeys.Specimen.IdhStatus, entry => entry.MolecularData.IdhStatus),
        For<Specimen, bool?>(MappingKeys.Specimen.TertStatus, entry => entry.MolecularData.TertStatus),
        For<Specimen, bool?>(MappingKeys.Specimen.MgmtStatus, entry => entry.MolecularData.MgmtStatus),

        For<Specimen, FixationType?>(MappingKeys.Specimen.Material.FixationType, entry => entry.Material.FixationTypeId),
        For<Specimen, string>(MappingKeys.Specimen.Material.Source, entry => entry.Material.Source.Value),

        For<Specimen, CellsSpecies?>(MappingKeys.Specimen.Line.CellsSpecies, entry => entry.Line.CellsSpeciesId),
        For<Specimen, CellsType?>(MappingKeys.Specimen.Line.CellsType, entry => entry.Line.CellsTypeId),
        For<Specimen, CellsCultureType?>(MappingKeys.Specimen.Line.CellsCultureType, entry => entry.Line.CellsCultureTypeId),

        For<Specimen, string>(MappingKeys.Specimen.Organoid.Medium, entry => entry.Organoid.Medium),
        // For<Specimen, int?>(MappingKeys.Specimen.Organoid.ImplantedCellsNumber, entry => entry.Organoid.ImplantedCellsNumber),
        For<Specimen, bool?>(MappingKeys.Specimen.Organoid.Tumorigenicity, entry => entry.Organoid.Tumorigenicity),

        For<Specimen, string>(MappingKeys.Specimen.Xenograft.MouseStrain, entry => entry.Xenograft.MouseStrain),
        For<Specimen, int?>(MappingKeys.Specimen.Xenograft.GroupSize, entry => entry.Xenograft.GroupSize),
        For<Specimen, ImplantType?>(MappingKeys.Specimen.Xenograft.ImplantType, entry => entry.Xenograft.ImplantTypeId),
        For<Specimen, ImplantLocation?>(MappingKeys.Specimen.Xenograft.ImplantLocation, entry => entry.Xenograft.ImplantLocationId),
        // For<Specimen, int?>(MappingKeys.Specimen.Xenograft.ImplantedCellsNumber, entry => entry.Xenograft.ImplantedCellsNumber),
        For<Specimen, bool?>(MappingKeys.Specimen.Xenograft.Tumorigenicity, entry => entry.Xenograft.Tumorigenicity),
        For<Specimen, TumorGrowthForm?>(MappingKeys.Specimen.Xenograft.TumorGrowthForm, entry => entry.Xenograft.TumorGrowthFormId),
        // For<Specimen, int?>(MappingKeys.Specimen.Xenograft.SurvivalDays, entry => entry.Xenograft.SurvivalDaysTo)
    ];

    public IEnumerable<IDataMapping> All =>
    [
        ..Donor,
        ..Image,
        ..Specimen
    ];

    private static DataMapping<T, TProp> For<T, TProp>(string key, Expression<Func<T, TProp>> expression) where T : class
    {
        return new DataMapping<T, TProp>(key, expression);
    }
}

public interface IDataMapping
{
    public string Key { get; }
    public string[] Autocomplete(DomainDbContext dbContext, int limit = 50);
}

public class DataMapping<T, TProp> : IDataMapping where T : class
{
    public string Key { get; }
    public Expression<Func<T, TProp>> Property { get; }

    public DataMapping(string key, Expression<Func<T, TProp>> property)
    {
        Key = key;
        Property = property;
    }

    public string[] Autocomplete(DomainDbContext dbContext, int limit = 50)
    {
        var predicate = Expression.Lambda<Func<T, bool>>(Expression.NotEqual(Property.Body, Expression.Constant(null)), Property.Parameters);

        var values = dbContext.Set<T>()
            .AsNoTracking()
            .Where(predicate)
            .GroupBy(Property)
            .OrderByDescending(group => group.Count())
            .Select(group => group.Key)
            .Take(limit)
            .ToArray();

        return values
            .Select(value => value.ToString())
            .ToArray();
    }
}
