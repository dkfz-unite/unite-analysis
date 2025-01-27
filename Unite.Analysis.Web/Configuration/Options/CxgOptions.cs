using Unite.Orchestrator.Configuration.Options;

namespace Unite.Analysis.Web.Configuration.Options;

public class CxgOptions : ICxgViewerOptions
{
    /// <summary>
    /// Image name.
    /// </summary>
    public string Image
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_VIEWER_CXG_IMAGE");

            if (string.IsNullOrWhiteSpace(option))
                throw new ArgumentNullException("'UNITE_VIEWER_CXG_IMAGE' environment variable has to be set");

            return option.Trim();
        }
    }

    /// <summary>
    /// Container name.
    /// </summary>
    public string Name
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_VIEWER_CXG_NAME");

            if (string.IsNullOrWhiteSpace(option))
                throw new ArgumentNullException("'UNITE_VIEWER_CXG_NAME' environment variable has to be set");

            return option.Trim();
        }
    }

    /// <summary>
    /// Container alias.
    /// </summary>
    public string Alias
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_VIEWER_CXG_ALIAS");

            if (string.IsNullOrWhiteSpace(option))
                throw new ArgumentNullException("'UNITE_VIEWER_CXG_ALIAS' environment variable has to be set");

            return option.Trim();
        }
    }

    /// <summary>
    /// Container ports range.
    /// </summary>
    public int[] Ports
    {
        get
        {
            var option = Environment.GetEnvironmentVariable("UNITE_VIEWER_CXG_PORTS");

            if (string.IsNullOrWhiteSpace(option))
                throw new ArgumentNullException("'UNITE_VIEWER_CXG_PORTS' environment variable has to be set");

            var values = option.Split('-');

            var startValid = int.TryParse(values[0], out var start);
            var endValid = int.TryParse(values[1], out var end);
            var valid = startValid && endValid && start <= end;

            if (!valid)
                throw new ArgumentException("'UNITE_VIEWER_CXG_PORTS' environment variable has to be in the format '54300-54399'");

            return [start, end];
        }
    }
}
