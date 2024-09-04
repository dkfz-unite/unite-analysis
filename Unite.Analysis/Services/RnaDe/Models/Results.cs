namespace Unite.Analysis.Services.RnaDe.Models;

public class Results
{
    private int _geneId;
    private string _geneStableId;
    private string _geneSymbol;
    private double _log2FoldChange;
    private double _pValueAdjusted;

    public int GeneId
    {
        get => _geneId;
        set => _geneId = value;
    }

    public string GeneStableId
    {
        get => _geneStableId?.Trim();
        set => _geneStableId = value;
    }

    public string GeneSymbol
    {
        get => _geneSymbol?.Trim();
        set => _geneSymbol = value;
    }

    public double Log2FoldChange
    {
        get => Math.Round(_log2FoldChange, 3);
        set => _log2FoldChange = value;
    }

    public double PValueAdjusted
    {
        get => Math.Round(_pValueAdjusted, 3);
        set => _pValueAdjusted = value;
    }    
}
