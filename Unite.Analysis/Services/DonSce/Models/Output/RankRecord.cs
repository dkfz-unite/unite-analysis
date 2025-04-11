using Unite.Essentials.Tsv.Attributes;

namespace Unite.Analysis.Services.DonSce.Models.Output;

public class RankRecord
{
    private double _chi2;
    private double _p;


    [Column("chi2")]
    public double Chi2 { get => Round(_chi2); set => _chi2 = value; }

    [Column("p")]
    public double P { get => Round(_p); set => _p = value; }


    private static double Round(double value)
    {
        return Math.Round(value, 2);
    }
}
