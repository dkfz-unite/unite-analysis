using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Unite.Analysis.Models.Enums;

[JsonConverter(typeof(JsonStringEnumMemberConverter))]
public enum DatasetDomain
{
    [EnumMember(Value = "donors")]
    Donors,

    [EnumMember(Value = "mrs")]
    Mrs,

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

    [EnumMember(Value = "sms")]
    Sms,

    [EnumMember(Value = "cnvs")]
    Cnvs,

    [EnumMember(Value = "svs")]
    Svs
}
