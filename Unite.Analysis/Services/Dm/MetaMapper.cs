using Unite.Analysis.Services.Dm.Models.Data;
using Unite.Essentials.Tsv;

namespace Unite.Analysis.Services.Dm;

public class MetaMapper
{
    public static ClassMap<MetadataSample> Map(MetadataSample[] entries)
    {
        return new ClassMap<MetadataSample>()
            .Map(entry => entry.SampleId, "sample_id")
            .Map(entry => entry.Condition, "condition")
            .Map(entry => entry.Path, "path");
    }
}
