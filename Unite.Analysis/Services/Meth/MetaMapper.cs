using System.Linq.Expressions;
using Unite.Analysis.Services.Meth.Models.Data;
using Unite.Essentials.Tsv;
using Unite.Essentials.Tsv.Converters;

namespace Unite.Analysis.Services.Meth;

public class MetaMapper
{
    private static readonly NullableValueConverter _nullableValueConverter = new();

    public static ClassMap<Metadata> Map(Metadata[] entries)
    {
        return new ClassMap<Metadata>()
            .Map(entry => entry.SampleId, "sample_id")
            .Map(entry => entry.Conditions, "conditions")
            .Map(entry => entry.Path, "path");
    }
}

public class NullableValueConverter : IConverter<string>
{
    public object Convert(string value, string row)
    {
        throw new NotImplementedException();
    }

    public string Convert(object value, object row)
    {
        var stringValue = value as string;

        return stringValue ?? "Unknown";
    }
}
