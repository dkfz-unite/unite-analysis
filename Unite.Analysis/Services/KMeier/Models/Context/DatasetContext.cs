namespace Unite.Analysis.Services.KMeier.Models.Context;

public class DatasetContext
{
    public string Name { get; }
    public int[] Keys { get; set; }
    public Data.Entities.Donors.Donor[] Donors { get; set; }


    public DatasetContext(string name)
    {
        Name = name;

        Keys = [];
        Donors = [];
    }
}
