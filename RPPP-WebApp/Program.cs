using NLog.Web;
using NLog;
using RPPP_WebApp;
using Microsoft.EntityFrameworkCore;
using FluentValidation.AspNetCore;
using RPPP_WebApp.ModelsValidation;
using FluentValidation;

//NOTE: Add dependencies/services in StartupExtensions.cs and keep this file as-is

var builder = WebApplication.CreateBuilder(args);
var logger = LogManager.Setup().GetCurrentClassLogger();

builder.Services.AddDbContext<RPPP_WebApp.Data.RPPP06Context>(
options => options.UseSqlServer(
builder.Configuration.GetConnectionString("RPPP06")));
builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<SuradnikValidator>()
    .AddValidatorsFromAssemblyContaining<PosaoValidator>()
    .AddValidatorsFromAssemblyContaining<ZahtjevValidator>()
    .AddValidatorsFromAssemblyContaining<ZadatakValidator>();



try
{
    logger.Debug("init main");
    builder.Host.UseNLog(new NLogAspNetCoreOptions() { RemoveLoggerFactoryFilter = false });

    var appSection = builder.Configuration.GetSection("AppSettings");
    builder.Services.Configure<AppSettings>(appSection);


    var app = builder.ConfigureServices().ConfigurePipeline();
    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}

public partial class Program { }