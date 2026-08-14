namespace Unite.Analysis.Models.Metadata;

public static class MappingKeys
{
    public static class Sample
    {
        public const string Key = "sample_key";
        public const string Id = "sample_id";
    }

    public static class Donor
    {
        public const string Id = "donor_id";
        public const string Age = "donor_age";
        public const string Sex = "donor_sex";
        public const string Diagnosis = "donor_diagnosis";
        public const string DiagnosisPrimarySite = "donor_diagnosis_primary_site";
        public const string DiagnosisLocalization = "donor_diagnosis_localization";
        public const string VitalStatus = "donor_vital_status";
        public const string ProgressionStatus = "donor_progression_status";
        public const string SteroidsReactive = "donor_steroids_reactive";
        public const string Kps = "donor_kps";
    }

    public static class Image
    {
        public const string Id = "image_id";
        public const string Type = "image_type";

        public static class Mr
        {
            public const string WholeTumor = "mr_whole_tumor";
            public const string ContrastEnhancing = "mr_contrast_enhancing";
            public const string NonContrastEnhancing = "mr_non_contrast_enhancing";
        }
    }

    public static class Specimen
    {
        public const string Id = "specimen_id";
        public const string Type = "specimen_type";
        public const string Category = "specimen_category";
        public const string TumorType = "specimen_tumor_type";
        public const string TumorGrade = "specimen_tumor_grade";
        public const string TumorSuperfamily = "specimen_tumor_superfamily";
        public const string TumorFamily = "specimen_tumor_family";
        public const string TumorClass = "specimen_tumor_class";
        public const string TumorSubclass = "specimen_tumor_subclass";
        public const string IdhStatus = "specimen_idh_status";
        public const string TertStatus = "specimen_tert_status";
        public const string MgmtStatus = "specimen_mgmt_status";

        public static class Material
        {
            public const string FixationType = "material_fixation_type";
            public const string Source = "material_source";
        }

        public static class Line
        {
            public const string CellsSpecies = "line_cells_species";
            public const string CellsType = "line_cells_type";
            public const string CellsCultureType = "line_cells_culture_type";
        }

        public static class Organoid
        {
            public const string Medium = "organoid_medium";
            public const string ImplantedCellsNumber = "organoid_implanted_cells_number";
            public const string Tumorigenicity = "organoid_tumorigenicity";
        }

        public static class Xenograft
        {
            public const string MouseStrain = "xenograft_mouse_strain";
            public const string GroupSize = "xenograft_group_size";
            public const string ImplantType = "xenograft_implant_type";
            public const string ImplantLocation = "xenograft_implant_location";
            public const string ImplantedCellsNumber = "xenograft_implanted_cells_number";
            public const string Tumorigenicity = "xenograft_tumorigenicity";
            public const string TumorGrowthForm = "xenograft_tumor_growth_form";
            public const string SurvivalDays = "xenograft_survival_days";
        }
    }
}