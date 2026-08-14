using System.Runtime.Serialization;

namespace Unite.Analysis.Services.Dep.Models.Criteria.Enums;

public enum NormalizationMethod
{
    [EnumMember(Value = "median")]
    Median = 0,

    [EnumMember(Value = "quantile")]
    Quantile = 1
}
