using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Unite.Analysis.Models.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum DatasetDomain
{
    [EnumMember(Value = "donors")]
    Donors,

    [EnumMember(Value = "mris")]
    Mris,

    [EnumMember(Value = "cts")]
    Cts,

    [EnumMember(Value = "materials")]
    Materials,

    [EnumMember(Value = "lines")]
    Lines,

    [EnumMember(Value = "organoids")]
    Organoids,

    [EnumMember(Value = "xenografts")]
    Xenografts,

    [EnumMember(Value = "genes")]
    Genes,

    [EnumMember(Value = "ssms")]
    Ssms,

    [EnumMember(Value = "cnvs")]
    Cnvs,

    [EnumMember(Value = "svs")]
    Svs
}
