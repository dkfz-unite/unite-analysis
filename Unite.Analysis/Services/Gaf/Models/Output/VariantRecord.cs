using Unite.Essentials.Extensions;

using SM = Unite.Data.Entities.Omics.Analysis.Dna.Sm;
using CNV = Unite.Data.Entities.Omics.Analysis.Dna.Cnv;
using SV = Unite.Data.Entities.Omics.Analysis.Dna.Sv;

namespace Unite.Analysis.Services.Gaf.Models.Output;

public class VariantRecord
{
    public int Id { get; set; }
    public int DonorId { get; set; }
    public int GeneId { get; set; }

    public string Position { get; set; }
    public string Type { get; set; }
    public string Change { get; set; }
    public string Impact { get; set; }
    public string Effect { get; set; }


    public VariantRecord(SM.Variant variant, int donorId, int geneId)
    {
        Id = variant.Id;
        DonorId = donorId;
        GeneId = geneId;

        Position = $"{variant.ChromosomeId.ToDefinitionString()}:{variant.Start}";
        Type = variant.TypeId.ToDefinitionString();
        Change = $"{variant.Ref ?? "-"} > {variant.Alt ?? "-"}";
        Impact = variant.MostAffectedTranscript?.MostSevereEffect?.Impact;
        Effect = variant.MostAffectedTranscript.MostSevereEffect.Type;
    }

    public VariantRecord(CNV.Variant variant, int donorId, int geneId)
    {
        Id = variant.Id;
        DonorId = donorId;
        GeneId = geneId;

        var loh = variant.Loh == true ? " LOH" : "";
        var del = variant.Del == true ? " DEL" : "";
        Position = $"{variant.ChromosomeId.ToDefinitionString()}:{variant.Start}-{variant.End}";
        Type = $"{variant.TypeId.ToDefinitionString()}{variant.Loh.Value}{loh}{del}";
        Change = $"{variant.Tcn ?? variant.TcnMean}";
        Impact = variant.MostAffectedTranscript?.MostSevereEffect?.Impact;
        Effect = variant.MostAffectedTranscript.MostSevereEffect.Type;
    }

    public VariantRecord(SV.Variant variant, int donorId, int geneId)
    {
        Id = variant.Id;
        DonorId = donorId;
        GeneId = geneId;

        Position = variant.TypeId == SV.Enums.SvType.ITX || variant.TypeId == SV.Enums.SvType.CTX
            ? $"{variant.ChromosomeId.ToDefinitionString()}:{variant.End} > {variant.OtherChromosomeId.ToDefinitionString()}:{variant.OtherStart}"
            : $"{variant.ChromosomeId.ToDefinitionString()}:{variant.End} - {variant.OtherChromosomeId.ToDefinitionString()}:{variant.OtherStart}";
        Type = variant.TypeId.ToDefinitionString();
        Change = null;
        Impact = variant.MostAffectedTranscript?.MostSevereEffect?.Impact;
        Effect = variant.MostAffectedTranscript.MostSevereEffect.Type;
    }
}
