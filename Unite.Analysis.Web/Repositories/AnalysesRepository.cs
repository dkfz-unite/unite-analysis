using Unite.Analysis.Models;
using Unite.Cache.Configuration.Options;
using Unite.Cache.Repositories;

namespace Unite.Analysis.Web.Repositories;

public class AnalysesRepository : CacheRepository<GenericAnalysis>
{
    public override string DatabaseName => "user-data";
    public override string CollectionName => "analyses";

    public AnalysesRepository(IMongoOptions options) : base(options)
    {
    }
}