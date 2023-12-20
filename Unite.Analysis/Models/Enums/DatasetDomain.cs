using System.Runtime.Serialization;

namespace Unite.Analysis.Models.Enums;

public enum DatasetDomain
{
    [EnumMember(Value = "Donors")]
    Donors,

    [EnumMember(Value = "Mris")]
    Mris,

    [EnumMember(Value = "Cts")]
    Cts,

    [EnumMember(Value = "Tissues")]
    Tissues,

    [EnumMember(Value = "Cells")]
    Cells,

    [EnumMember(Value = "Organoids")]
    Organoids,

    [EnumMember(Value = "Xenografts")]
    Xenografts,

    [EnumMember(Value = "Genes")]
    Genes,

    [EnumMember(Value = "Ssms")]
    Ssms,

    [EnumMember(Value = "Cnvs")]
    Cnvs,

    [EnumMember(Value = "Svs")]
    Svs
}
