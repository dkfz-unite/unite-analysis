using System.Runtime.Serialization;

namespace Unite.Analysis.Services.Dep.Models.Criteria.Enums;

public enum ImputationMethod
{
    [EnumMember(Value = "mindet")]
    MinDet = 0,

    [EnumMember(Value = "minprob")]
    MinProb = 1
}
