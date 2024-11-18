namespace Unite.Analysis.Services.KMeier.Models.Input;

public record Metadata
{
    /// <summary>
    /// Dataset identifier.
    /// </summary>
    public string DatasetId { get; set; }
    
    /// <summary>
    /// Donor identifier.
    /// </summary>
    public string DonorId { get; set; }

    /// <summary>
    /// Enrolment date.
    /// </summary>
    public DateOnly? EnrolmentDate { get; set; }

    /// <summary>
    /// Status at last follow-up. True - event (dead, progression). False - no event (alive, no progression).
    /// </summary>
    public bool? Status { get; set; }

    /// <summary>
    /// Date of last follow-up (require 'EnrollmentDate' to be set).
    /// </summary>
    public DateOnly? StatusChangeDate { get; set; }

    /// <summary>
    /// Days from enrollment to last follow-up.
    /// </summary>
    public int? StatusChangeDay { get; set; }
}
