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

    public string DonSceUrl
    {
        get
        {
            return GetOption("UNITE_ANALYSIS_DON_SCE_HOST");
        }
    }

    public string MethDmUrl
    {
        get
        {
            return GetOption("UNITE_ANALYSIS_METH_DM_HOST");
        }
    }

    public string RnaDeUrl
    {
        get
        {
            return GetOption("UNITE_ANALYSIS_RNA_DE_HOST");
        }
    }

    public string RnascDcUrl
    {
        get
        {
            return GetOption("UNITE_ANALYSIS_RNASC_DC_HOST");
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
