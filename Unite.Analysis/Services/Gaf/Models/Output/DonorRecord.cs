using Unite.Data.Entities.Donors;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Services.Gaf.Models.Output;

public class DonorRecord
{
    public string Id { get; }
    public string DisplayId { get; }

    public string Diagnosis { get; }
    public string PrimarySite { get; }
    public string Localization { get; }
    public string Sex { get; }
    public int? Age { get; }
    public bool? VitalStatus { get; }
    public int? VitalStatusChangeDay { get; }
    public bool? ProgressionStatus { get; }
    public int? ProgressionStatusChangeDay { get; }
    public bool? SteroidsReactive { get; }
    public int? Kps { get; }


    public DonorRecord(Donor donor)
    {
        Id = donor.Id.ToString();
        DisplayId = donor.ReferenceId;

        Diagnosis = donor.ClinicalData?.Diagnosis;
        PrimarySite = donor.ClinicalData?.PrimarySite?.Value;
        Localization = donor.ClinicalData?.Localization?.Value;
        Sex = donor.ClinicalData?.SexId?.ToDefinitionString();
        Age = donor.ClinicalData?.EnrollmentAge;
        VitalStatus = donor.ClinicalData?.VitalStatus;
        VitalStatusChangeDay = donor.ClinicalData?.VitalStatusChangeDay;
        ProgressionStatus = donor.ClinicalData?.ProgressionStatus;
        ProgressionStatusChangeDay = donor.ClinicalData?.ProgressionStatusChangeDay;
        SteroidsReactive = donor.ClinicalData?.SteroidsReactive;
        Kps = donor.ClinicalData?.Kps;
    }
}
