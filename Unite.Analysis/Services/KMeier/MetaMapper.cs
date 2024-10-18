using Unite.Analysis.Services.KMeier.Models;
using Unite.Essentials.Tsv;
using Unite.Essentials.Tsv.Converters;

namespace Unite.Analysis.Services.KMeier;

public static class MetaMapper
{
    private static readonly IConverter<DateOnly?> _dateConverter = new DateConverter();
    private static readonly IConverter<bool?> _boolConverter = new BoolConverter();

    public static ClassMap<Metadata> Map(Metadata[] entries)
    {
        var map = new ClassMap<Metadata>()
            .Map(entry => entry.Id, "sample_id")
            .Map(entry => entry.DiagnisosDate, "diagnosis_date", _dateConverter)
            .Map(entry => entry.VitalStatus, "vital_status", _boolConverter)
            .Map(entry => entry.VitalStatusChangeDate, "vital_status_change_date", _dateConverter)
            .Map(entry => entry.VitalStatusChangeDay, "vital_status_change_day");

        return map;
    }
}

public class DateConverter : IConverter<DateOnly?>
{
    public object Convert(string value, string row)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateOnly.Parse(value);
    }

    public string Convert(object value, object row)
    {
        if (value == null)
            return string.Empty;

        if (value is DateOnly obj)
            return obj.ToString("yyyy-MM-dd");
        
        return string.Empty;
    }
}

public class BoolConverter : IConverter<bool?>
{
    public object Convert(string value, string row)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return bool.Parse(value);
    }

    public string Convert(object value, object row)
    {
        if (value == null)
            return string.Empty;

        if (value is bool obj)
            return obj.ToString().ToLower();

        return string.Empty;
    }
}
