using Unite.Data.Context.Configuration.Options;

namespace Unite.Analysis.Web.Configuration.Options;

public class SqlOptions : ISqlOptions
{
    public string Host => Environment.GetEnvironmentVariable("UNITE_SQL_HOST");
    public string Port => Environment.GetEnvironmentVariable("UNITE_SQL_PORT");
    public string User => Environment.GetEnvironmentVariable("UNITE_SQL_USER");
    public string Password => Environment.GetEnvironmentVariable("UNITE_SQL_PASSWORD");
}
