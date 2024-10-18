using Unite.Analysis.Configuration.Options;

namespace Unite.Analysis.Web.Configuration.Options;

public class AnalysisOptions : IAnalysisOptions
{
    public string DataPath
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_ANALYSIS_DATA_PATH");

            if (string.IsNullOrWhiteSpace(option))
                throw new ArgumentNullException("'UNITE_ANALYSIS_DATA_PATH' environment variable has to be set");

            var value = option.Trim();

            if (value.StartsWith("~/"))
                value = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), value[2..]);

            return value;
        }
    }

    public string DataHost
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_ANALYSIS_DATA_HOST");

            return option?.Trim();
        }
    }

    public string DESeq2Url
    {
        get
        {
            return GetOption("UNITE_ANALYSIS_DESEQ2_HOST");
        }
    }

    public string SCellUrl
    {
        get
        {
            return GetOption("UNITE_ANALYSIS_SCELL_HOST");
        }
    }

    public string KMeierUrl
    {
        get
        {
            return GetOption("UNITE_ANALYSIS_KMEIER_HOST");
        }
    }

    public int? Limit { get; }


    private static string GetOption(string name)
    {
        var option = Environment.GetEnvironmentVariable(name);

        if (string.IsNullOrWhiteSpace(option))
            throw new ArgumentNullException($"'{name}' environment variable has to be set");

        return option.Trim();
    }
}
