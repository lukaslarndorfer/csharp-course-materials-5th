using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PuppyAdoption.Util;
using PuppyAdoption.ViewModels;
using PuppyAdoption.Views;

namespace PuppyAdoption;

public sealed class App : Application
{
    private static IServiceProvider? _serviceProvider;

    public App(IServiceProvider services)
    {
        _serviceProvider = services;

        // Register a global exception handler for the UI thread to not crash the whole application
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            ILogger<App> logger = services.GetRequiredService<ILogger<App>>();
            var toastService = services.GetRequiredService<IToastService>();
            logger.LogError(e.Exception, "Unhandled exception in UI thread");

            toastService.ShowError("An unexpected error occurred!");
        };
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = CreateViewModel<MainWindowViewModel>()
            };

            _serviceProvider?.GetRequiredService<IToastService>().SetWindow(desktop.MainWindow);
            _serviceProvider?.GetRequiredService<IDialogService>().SetOwner(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }

    internal static T CreateViewModel<T>() where T : ViewModelBase
    {
        if (_serviceProvider is null)
        {
            throw new InvalidOperationException("Service provider is not initialized");
        }

        var scope = _serviceProvider.CreateScope();
        var viewModel = ActivatorUtilities.CreateInstance<T>(scope.ServiceProvider);
        viewModel.AttachScope(scope);

        return viewModel;
    }
}
