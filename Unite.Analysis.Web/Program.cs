using Unite.Analysis.Web.Configuration.Extensions;


var builder = WebApplication.CreateBuilder(args);


builder.Logging.ClearProviders();

builder.Logging.AddConsole();


builder.Services.AddServices();

builder.Services.AddControllers(options => options.AddMvcOptions())
                .AddJsonOptions(options => options.AddJsonOptions());

var app = builder.Build();

app.UseRouting();

app.MapControllers();


app.Run();
