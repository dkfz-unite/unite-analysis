using Unite.Analysis.Models.Metadata;
using Unite.Essentials.Tsv;
using Unite.Essentials.Tsv.Converters;

namespace Unite.Analysis.Services;

public class SampleMetadataMapper
{
    private static readonly NullableValueConverter _nullableValueConverter = new();


    public static ClassMap<T> Map<T>(T[] entries, bool mapId = false) where T : SampleMetadata
    {
        var map = new ClassMap<T>();

        MapEntries(map, entries, mapId);

        return map;
    }

    public static ClassMap<SampleMetadata> Map(SampleMetadata[] entries, bool mapId = false)
    {
        var map = new ClassMap<SampleMetadata>();

        MapEntries(map, entries, mapId);

        return map;
    }

    private static void MapEntries<T>(ClassMap<T> map, T[] entries, bool mapId = false)
        where T : SampleMetadata
    {
        // TODO: Improve Essentials.Tsv so that it can take array of mappings to ClassMap<T> constructor.
        // Add option to not map or to not write to tsv columns which have no values or to delete mapping if there are no values for it.
        var mappings = new Mappings<T>();

        if (mapId)
            map.Map(mappings.SampleId.Expression, mappings.SampleId.Key);

        MapProperty(map, entries, mappings.SampleKey);

        if (entries.Any(entry => entry.Donor != null))
            MapProperties(map, entries, mappings.Donor);

        if (entries.Any(entry => entry.Image != null))
        {
            MapProperties(map, entries, mappings.Image);

            if (entries.Any(entry => entry.Image.Mr != null))
                MapProperties(map, entries, mappings.ImageMr);
        }

        if (entries.Any(entry => entry.Specimen != null))
        {
            MapProperties(map, entries, mappings.Specimen);

            if (entries.Any(entry => entry.Specimen.Material != null))
                MapProperties(map, entries, mappings.SpecimenMaterial);

            if (entries.Any(entry => entry.Specimen.Line != null))
                MapProperties(map, entries, mappings.SpecimenLine);

            if (entries.Any(entry => entry.Specimen.Organoid != null))
                MapProperties(map, entries, mappings.SpecimenOrganoid);

            if (entries.Any(entry => entry.Specimen.Xenograft != null))
                MapProperties(map, entries, mappings.SpecimenXenograft);
        }
    }


    private static void MapProperty<T>(ClassMap<T> map, T[] entries, Mapping<T, string> mapping)
        where T : SampleMetadata
    {
        var getter = mapping.Expression.Compile();

        if (entries.Any(entry => getter(entry) != null))
        {
            map.Map(mapping.Expression, mapping.Key, _nullableValueConverter);
        }
    }

    private static void MapProperties<T>(ClassMap<T> map, T[] entries, IEnumerable<Mapping<T, string>> mappings)
        where T : SampleMetadata
    {
        foreach (var mapping in mappings)
        {
            MapProperty(map, entries, mapping);
        }
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
