using System.Text.Json;

namespace Unite.Analysis.Models;

public abstract record Analysis
{
    public string Id { get; set; }

    public string UserId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Date { get; set; }

    public string Status { get; set; }

    public string Type { get; set; }
}

/// <summary>
/// This is used in contoller.
/// For example DESeq2 model will be [FromBody] TypedAnalysis<...Services.DESeq2.Criteria.Analysis>.
/// </summary>
/// <typeparam name="T">Specific analysis type (KMeier, SCell, DESeq2). All analysis type schould implement AnalysisData class.</typeparam>
// public record TypedAnalysis<T> : Analysis where T : AnalysisData
public record TypedAnalysis<T> : Analysis where T : AnalysisData
{
    // public string Data { get; set; } // Check this to bind UI

    // public string Key { get; set; } // Check this to bind UI


    public DatasetCriteria[] Datasets { get; set; }
}

/// <summary>
/// This goes to Mongo.
/// When we load analyses list to UI, then we load GenericAnalysis, as we don't care which actual type it is, frontend will know how to display it.
/// </summary>
public record GenericAnalysis : Analysis
{
    public string Datasets { get; set; }

    public static GenericAnalysis From<T>(TypedAnalysis<T> analysis) where T : AnalysisData
    {
        return new GenericAnalysis
        {
            Id = analysis.Id,
            UserId = analysis.UserId,
            Name = analysis.Name,
            Description = analysis.Description,
            Date = analysis.Date,
            Status = analysis.Status,
            Type = analysis.Type,
            Datasets = JsonSerializer.Serialize(analysis.Datasets)
        };
    }
}
