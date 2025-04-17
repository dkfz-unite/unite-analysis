using System.Runtime.Serialization;

namespace Unite.Analysis.Services.Scell.Models.Criteria.Enums;

public enum ClusteringType
{
    [EnumMember(Value = "louvain")]
    Louvain = 1,

    [EnumMember(Value = "leiden")]
    Leiden = 2
}
