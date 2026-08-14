namespace Unite.Analysis.Services.CnvProfile.Models.Output;

public record GridTrack
{
    public int Id { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }

    public GridTrack(int id, string key, string value)
    {
        Id = id;
        Key = key;
        Value = value;
    }
}
