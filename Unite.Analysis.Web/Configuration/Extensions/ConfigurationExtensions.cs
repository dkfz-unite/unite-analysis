using Unite.Analysis.Configuration.Options;
using Unite.Analysis.Expression;
using Unite.Analysis.Web.Configuration.Options;
using Unite.Analysis.Web.Handlers;
using Unite.Analysis.Web.HostedServices;
using Unite.Analysis.Web.Services;
using Unite.Data.Context.Configuration.Extensions;
using Unite.Data.Context.Configuration.Options;
using Unite.Indices.Context.Configuration.Options;
using Unite.Indices.Search.Configuration.Extensions;


namespace Unite.Analysis.Web.Configuration.Extensions;

public static class ConfigurationExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        var sqlOptions = new SqlOptions();

        services.AddOptions();
        services.AddDatabase();
        services.AddDatabaseFactory(sqlOptions);
        services.AddSearchEngine();
        services.AddSearchServices();

        services.AddHostedService<AnalysisPreparingHostedService>();
        services.AddTransient<AnalysisPreparingHandler>();
        services.AddTransient<AnalysisTaskService>();
        services.AddTransient<ExpressionAnalysisService>();

        services.AddHostedService<AnalysisProcessingHostedService>();
        services.AddTransient<AnalysisProcessingHandler>();
    }

    private static void AddOptions(this IServiceCollection services)
    {
        services.AddTransient<IElasticOptions, ElasticOptions>();
        services.AddTransient<ISqlOptions, SqlOptions>();
        services.AddTransient<IAnalysisOptions, AnalysisOptions>(); 
        services.AddTransient<AnalysisOptions>();
    }
}
