using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using PiCalc.Core;
using PiCalc.ViewModels;
using PiCalc.Views;

namespace PiCalc;

public sealed class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var collection = new ServiceCollection();
        collection.AddCoreServices();
        var services = collection.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = CreateMainWindowViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();

        return;

        MainWindowViewModel CreateMainWindowViewModel()
        {
            var runner = services.GetRequiredService<IRunner>();
            var clock = services.GetRequiredService<IClock>();

            return new MainWindowViewModel(runner, clock);
        }
    }
}
