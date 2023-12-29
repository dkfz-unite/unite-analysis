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

    public string DESeq2Url
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_ANALYSIS_DESEQ2_HOST");

            if (string.IsNullOrWhiteSpace(option))
                throw new ArgumentNullException("'UNITE_ANALYSIS_DESEQ2_HOST' environment variable has to be set");

            return option.Trim();
        }
    }

    public int? Limit { get; }
}
