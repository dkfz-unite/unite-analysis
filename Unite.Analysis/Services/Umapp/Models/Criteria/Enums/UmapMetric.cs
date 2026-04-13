using System.Runtime.Serialization;

namespace Unite.Analysis.Services.Umapp.Models.Criteria.Enums;

public enum UmapMetric
{
    [EnumMember(Value = "euclidean")]
    Euclidean,

    [EnumMember(Value = "manhattan")]
    Manhattan
}
