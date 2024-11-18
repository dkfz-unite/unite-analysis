using Unite.Essentials.Tsv.Attributes;

namespace Unite.Analysis.Services.KMeier.Models.Output;

public class ResultRecord
{
    private double _time;
    private double _survivalProb;
    private double _confIntLower;
    private double _confIntUpper;
    private string _datasetId;


    [Column("time")]
    public double Time { get => Round(_time); set => _time = value; }

    [Column("survival_prob")]
    public double SurvivalProb { get => Round(_survivalProb); set => _survivalProb = value; }

    [Column("conf_int_lower")]
    public double ConfIntLower { get => Round(_confIntLower); set => _confIntLower = value; }

    [Column("conf_int_upper")]
    public double ConfIntUpper { get => Round(_confIntUpper); set => _confIntUpper = value; }

    [Column("dataset_id")]
    public string DatasetId { get => _datasetId?.Trim(); set => _datasetId = value; }


    private static double Round(double value)
    {
        return Math.Round(value, 2);
    }
}
