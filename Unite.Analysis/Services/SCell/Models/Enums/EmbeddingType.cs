using System.Runtime.Serialization;

namespace Unite.Analysis.Services.SCell.Models.Enums;

public enum EmbeddingType
{
    [EnumMember(Value = "umap")]
    Umap = 1,

    [EnumMember(Value = "tsne")]
    Tsne = 2
}
