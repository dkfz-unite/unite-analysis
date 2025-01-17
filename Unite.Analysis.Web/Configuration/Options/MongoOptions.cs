using Unite.Cache.Configuration.Options;

namespace Unite.Analysis.Web.Configuration.Options;

public class MongoOptions : IMongoOptions
{
    public string Host => Environment.GetEnvironmentVariable("UNITE_MONGO_HOST");
    public string Port => Environment.GetEnvironmentVariable("UNITE_MONGO_PORT");
    public string User => Environment.GetEnvironmentVariable("UNITE_MONGO_USER");
    public string Password => Environment.GetEnvironmentVariable("UNITE_MONGO_PASSWORD");
}