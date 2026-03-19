using System.Runtime.Serialization;

namespace Unite.Analysis.Services.Dep.Models.Criteria.Enums;

public enum NormalizationMethod
{
    [EnumMember(Value = "median")]
    Median,

    [EnumMember(Value = "quantile")]
    Quantile
}
