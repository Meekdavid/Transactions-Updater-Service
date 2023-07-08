using PTAUpdater;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Serilog.Sinks.SystemConsole.Themes;
using PTAUpdater.Helpers.Services;
using PTAUpdater.Interfaces;
using PTAUpdater.Repositories;
using PTAUpdater.Processes;

IHost host = Host.CreateDefaultBuilder(args).UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;
        services.AddHostedService<Worker>();
        services.AddOptions();
        services.AddTransient<ComplianceCheck>();
        services.AddTransient<ISQLBinary, SQLBinary>();
        services.AddTransient<ITransactionsRetreiver, TransactionsRetreiver>();
        services.AddTransient<ICommunicationHandler, CommunicationHandler>();
        services.AddTransient<IFileHandler, FileHandler>();
        services.AddTransient<IEmailSender, EmailSender>();
        services.AddTransient<Updater>();

    }).UseSerilog((hostingContext, LoggerConfiguration) => {
        LoggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
        .WriteTo.Console(theme: AnsiConsoleTheme.Literate, applyThemeToRedirectedOutput: true);
    })
    .Build();

try
{
    await host.RunAsync();
}
catch (Exception onBuild)
{
    Log.Fatal(onBuild?.StackTrace, $"PTA Transactions Updater failed initiation ... Details:\r\n {onBuild}");
}
finally
{
    Log.CloseAndFlush();
}