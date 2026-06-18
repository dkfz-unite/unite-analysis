using System.Runtime.Serialization;

namespace Unite.Analysis.Services.Cedp.Models.Criteria.Enums;

public enum FeatureType
{
    [EnumMember(Value = "gene")]
    Gene,

    [EnumMember(Value = "protein")]
    Protein
}
