using Unite.Orchestrator.Configuration.Options;

namespace Unite.Analysis.Web.Configuration.Options;

public class OrchOptions : IOrchestratorOptions
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

    public int IdleTimeout
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_ANALYSIS_IDLE_TIMEOUT");

            if (string.IsNullOrWhiteSpace(option))
                throw new ArgumentNullException("'UNITE_ANALYSIS_IDLE_TIMEOUT' environment variable has to be set");

            if (!int.TryParse(option, out var value))
                throw new ArgumentException("'UNITE_ANALYSIS_IDLE_TIMEOUT' environment variable has to be an integer");

            return value;
        }
    }
}
