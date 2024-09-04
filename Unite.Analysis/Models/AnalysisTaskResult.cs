using Unite.Analysis.Models.Enums;

namespace Unite.Analysis.Models;

public class AnalysisTaskResult
{
    public double? Elapsed { get; set; }
    public AnalysisTaskStatus Status { get; set; }
    public string Message { get; set; }

    public AnalysisTaskResult(double elapsed)
    {
        Elapsed = Math.Round(elapsed, 2);
        Status = AnalysisTaskStatus.Success;
    }

    public AnalysisTaskResult(double? elapsed, AnalysisTaskStatus status)
    {
        Elapsed = elapsed;
        Status = status;
    }

    public static AnalysisTaskResult Success(double? elapsed = null)
    {
        return new(elapsed, AnalysisTaskStatus.Success);
    }

    public static AnalysisTaskResult Rejected(double? elapsed = null, string message = null)
    {
        return new(elapsed, AnalysisTaskStatus.Rejected) { Message = message };
    }

    public static AnalysisTaskResult Failed(double? elapsed = null, string message = null)
    {
        return new (elapsed, AnalysisTaskStatus.Failed) { Message = message };
    }
}
