using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Extensions.Hosting;
using WpfEmlViewerExample;
using Serilog;
using WpfEmlViewerExample.Services;

// Create a builder by specifying the application and main window.
var builder = WpfApplication<App, MainWindow>.CreateBuilder(args);

// Configure dependency injection.
builder.Services.AddTransient<MainWindowViewModel>();
builder.Services.AddSingleton<EmlExtractorService>();

// Configure the settings.
// Injecting IOptions<MySettings> from appsetting.json.
builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Host.UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(hostingContext.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Debug()
    .WriteTo.File(
        @"Logs\log.txt",
        rollingInterval: RollingInterval.Day));

var app = builder.Build();
app.Startup += (sender, eventArgs) =>
{

};

await app.RunAsync();