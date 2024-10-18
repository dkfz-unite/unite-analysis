namespace Unite.Analysis.Services.KMeier.Models;

public record Metadata
{
    public string Id { get; set; }

    public DateOnly? DiagnisosDate { get; set; }
    public bool? VitalStatus { get; set; }
    public DateOnly? VitalStatusChangeDate { get; set; }
    public int? VitalStatusChangeDay { get; set; }
}
