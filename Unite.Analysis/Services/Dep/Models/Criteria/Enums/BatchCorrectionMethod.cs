using System.Runtime.Serialization;

namespace Unite.Analysis.Services.Dep.Models.Criteria.Enums;

public enum BatchCorrectionMethod
{
    [EnumMember(Value = "combat")]
    ComBat,

    [EnumMember(Value = "limma")]
    Limma
}
