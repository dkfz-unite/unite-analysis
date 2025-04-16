namespace Unite.Analysis.Services.Scell.Models.Data;

public class Metadata
{
    public string Id { get; set; }

    public DonorMetadata Donor { get; set; }
    public ImageMetadata Image { get; set; }
    public SpecimenMetadata Specimen { get; set; }
}


public class DonorMetadata
{
    public string Id { get; set; }
    
    public string Sex { get; set; }
    public string Age { get; set; }
    public string Diagnosis { get; set; }
    public string PrimarySite { get; set; }
    public string Localization { get; set; }
    public string VitalStatus { get; set; }
    public string ProgressionStatus { get; set; }
    public string SteroidsReactive { get; set; }
    public string Kps { get; set; }
}


public class ImageMetadata
{
    public string Id { get; set; }
    
    public string Type { get; set; }

    public MrMetadata Mr { get; set; }
}

public class MrMetadata
{
    public string WholeTumor { get; set; }
    public string ContrastEnhancing { get; set; }
    public string NonContrastEnhancing { get; set; }
}


public class SpecimenMetadata
{
    public string Id { get; set; }

    public string Type { get; set; }
    public string IdhStatus { get; set; }
    public string MgmtStatus { get; set; }

    public MaterialMetadata Material { get; set; }
    public LineMetadata Line { get; set; }
    public OrganoidMetadata Organoid { get; set; }
    public XenograftMetadata Xenograft { get; set; }
}

public class MaterialMetadata
{
    public string Type { get; set; }
    public string TumorType { get; set; }
    public string Source { get; set; }
}

public class LineMetadata
{
    public string CellsSpecies { get; set; }
    public string CellsType { get; set; }
    public string CellsCultureType { get; set; }
}

public class OrganoidMetadata
{
    public string Medium { get; set; }
    public string ImplantedCellsNumber { get; set; }
    public string Tumorigenicity { get; set; }
}

public class XenograftMetadata
{
    public string MouseStrain { get; set; }
    public string GroupSize { get; set; }
    public string ImplantType { get; set; }
    public string ImplantLocation { get; set; }
    public string ImplantedCellsNumber { get; set; }
    public string Tumorigenicity { get; set; }
    public string TumorGrowthForm { get; set; }
    public string SurvivalDays { get; set; }
}
