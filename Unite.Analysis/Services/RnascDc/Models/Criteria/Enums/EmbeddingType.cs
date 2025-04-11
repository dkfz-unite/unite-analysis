using System.Runtime.Serialization;

namespace Unite.Analysis.Services.RnascDc.Models.Criteria.Enums;

public enum EmbeddingType
{
    [EnumMember(Value = "umap")]
    Umap = 1,

    [EnumMember(Value = "tsne")]
    Tsne = 2
}
