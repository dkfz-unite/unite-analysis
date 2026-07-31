using System.Runtime.Serialization;

namespace Unite.Analysis.Services.Dep.Models.Criteria.Enums;

public enum BatchCorrectionMethod
{
    [EnumMember(Value = "combat")]
    ComBat = 0,

    [EnumMember(Value = "limma")]
    Limma = 1
}
