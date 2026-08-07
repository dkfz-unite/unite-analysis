using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.CnvProfile.Models.Criteria;

public class Options
{
    [JsonPropertyName("event_threshold")]
    public double EventThreshold { get; set; } = 0.8;

    [JsonPropertyName("track_property")]
    public string[] TrackProperty { get; set; } = [];

    [JsonPropertyName("track_property_value")]
    public string[] TrackPropertyValue { get; set; } = [];
}