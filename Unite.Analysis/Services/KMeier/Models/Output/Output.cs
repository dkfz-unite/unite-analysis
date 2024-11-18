using System.Text.Json.Serialization;

namespace Unite.Analysis.Services.KMeier.Models.Output;

public class Output
{
    [JsonPropertyName("rank")]
    public Rank Rank { get; set; }

    [JsonPropertyName("curves")]
    public Dictionary<string, Curve> Curves { get; set; }
}

public class Curve
{
    [JsonPropertyName("time")]
    public double[] Time { get; set; }

    [JsonPropertyName("survival_prob")]
    public double[] SurvivalProb { get; set; }

    [JsonPropertyName("conf_int_lower")]
    public double[] ConfIntLower { get; set; }

    [JsonPropertyName("conf_int_upper")]
    public double[] ConfIntUpper { get; set; }
}

public class Rank
{
    [JsonPropertyName("chi2")]
    public double Chi2 { get; set; }

    [JsonPropertyName("p")]
    public double P { get; set; }
}
