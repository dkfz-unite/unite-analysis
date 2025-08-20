using System.Text.Json.Serialization;
using Unite.Data.Entities.Donors;
using Unite.Essentials.Extensions;

namespace Unite.Analysis.Services.Gaf.Models.Output;

public class DonorRecord
{
    [JsonPropertyName("id")]
    public string Id { get; }
    [JsonPropertyName("displayId")]
    public string DisplayId { get; }

    [JsonPropertyName("diagnosis")]
    public string Diagnosis { get; }
    [JsonPropertyName("primarySite")]
    public string PrimarySite { get; }
    [JsonPropertyName("localization")]
    public string Localization { get; }
    [JsonPropertyName("sex")]
    public string Sex { get; }
    [JsonPropertyName("age")]
    public int? Age { get; }
    [JsonPropertyName("vitalStatus")]
    public bool? VitalStatus { get; }
    [JsonPropertyName("vitalStatusChangeDay")]
    public int? VitalStatusChangeDay { get; }
    [JsonPropertyName("progressionStatus")]
    public bool? ProgressionStatus { get; }
    [JsonPropertyName("progressionStatusChangeDay")]
    public int? ProgressionStatusChangeDay { get; }
    [JsonPropertyName("steroidsReactive")]
    public bool? SteroidsReactive { get; }
    [JsonPropertyName("kps")]
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
