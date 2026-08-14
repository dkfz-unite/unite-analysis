using System.Runtime.Serialization;

namespace Unite.Analysis.Services.Cedp.Models.Criteria.Enums;

public enum ModelType
{
    [EnumMember(Value = "lm")]
    LeastSquares,

    [EnumMember(Value = "rfit")]
    RankBased
}
