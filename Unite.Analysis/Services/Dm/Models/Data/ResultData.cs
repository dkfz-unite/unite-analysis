namespace Unite.Analysis.Services.Dm.Models.Data;

public class Resultdata
{
    private int _count;
    private double _log2FoldChange;
    private double _pValueAdjusted;
    private string _cpgId;
    private string _regulatoryFeatureName;
    private string _phantom4_Enhancers;
    private string _phantom5_Enhancers;
    private string _ucscRefGeneName;

    public int Count
    {
        get => _count;
        set => _count = value;
    }

    public double Log2FoldChange
    {
        get => _log2FoldChange;
        set => _log2FoldChange = value;
    }

    public double PValueAdjusted
    {
        get => _pValueAdjusted;
        set => _pValueAdjusted = value;
    }

    public string CpgId
    {
        get => _cpgId;
        set => _cpgId = value;
    }

    public string RegulatoryFeatureName
    {
        get => _regulatoryFeatureName;
        set => _regulatoryFeatureName = value;
    }

    public string Phantom4Enhancers
    {
        get => _phantom4_Enhancers;
        set => _phantom4_Enhancers = value;
    }
    public string Phantom5Enhancers
    {
        get => _phantom5_Enhancers;
        set => _phantom5_Enhancers = value;
    }
    
    public string UcscRefGeneName
    {
        get => _ucscRefGeneName;
        set => _ucscRefGeneName = value;
    }
}