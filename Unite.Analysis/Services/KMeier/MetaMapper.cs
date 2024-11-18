using Unite.Analysis.Services.KMeier.Models.Input;
using Unite.Essentials.Tsv;
using Unite.Essentials.Tsv.Converters;

namespace Unite.Analysis.Services.KMeier;

internal static class MetaMapper
{
    private static readonly IConverter<DateOnly?> _dateConverter = new DateConverter();
    private static readonly IConverter<bool?> _boolConverter = new BoolConverter();

    public static ClassMap<Metadata> Map(Metadata[] entries)
    {
       return new ClassMap<Metadata>()
            .Map(entry => entry.DatasetId, "dataset_id")
            .Map(entry => entry.DonorId, "donor_id")
            .Map(entry => entry.EnrolmentDate, "enrolment_date", _dateConverter)
            .Map(entry => entry.Status, "status", _boolConverter)
            .Map(entry => entry.StatusChangeDate, "status_change_date", _dateConverter)
            .Map(entry => entry.StatusChangeDay, "status_change_day");
    }
}

internal class DateConverter : IConverter<DateOnly?>
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

internal class BoolConverter : IConverter<bool?>
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
