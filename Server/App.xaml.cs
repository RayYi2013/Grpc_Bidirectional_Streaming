using Server.Application.Interfaces;
using Server.Infrastructure.GrpcServices;
using Server.Infrastructure.Managers;
using Server.Infrastructure.Services;
using Server.ViewModels;
using Server.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace Server;

/// <summary>
/// Application entry point with dependency injection configuration.
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;

    public App()
    {
        // Build DI container with all services
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register ViewModels
                services.AddTransient<MainViewModel>();

                // Register Application Layer Services (Interfaces)
                services.AddSingleton<IServerHostService, KestrelServerHostService>();
                services.AddSingleton<IBroadcastService, GrpcBroadcastService>();

                // Register Infrastructure Layer Services
                services.AddSingleton<IClientConnectionManager, ClientConnectionManager>();
                services.AddSingleton<BidirectionalChatService>();

                // Register Views
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Create and show main window with DI
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }

        base.OnExit(e);
    }
}
