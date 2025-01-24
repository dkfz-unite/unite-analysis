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

public record TypedAnalysis<T> : Analysis where T : AnalysisData
{
    public T Data { get; set; }
}

public record GenericAnalysis : Analysis
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Data { get; set; }

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
            Data = JsonSerializer.Serialize(analysis.Data, _serializerOptions)
        };
    }
}
