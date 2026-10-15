using System;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NodaTime;
using NodaTime.Text;
using PiCalc.Core;

namespace PiCalc.ViewModels;

/// <summary>
///     View model for the main window of the PiCalc application
/// </summary>
public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    // TODO
    [ObservableProperty]
    private ExperimentRendererViewModel? _experimentRendererViewModel;

    private IClock _clock;
    private IRunner _runner;

    [ObservableProperty]
    public partial bool IsVectorized { get; set; } = false;

    [ObservableProperty]
    public partial bool IsProgressReportingEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsResultRenderingEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool IsRunning { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ThreadsLabel))]
    public partial int Threads { get; set; }

    [ObservableProperty]
    public partial int MinThreads { get; set; } = 1;

    [ObservableProperty]
    public partial int MaxThreads { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExperimentsLabel))]
    public partial long ExperimentsToRun { get; set; } = 100_000L;
    
    [ObservableProperty]
    public partial double ProgressValue { get; set; }

    [ObservableProperty]
    public partial string ResultText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ElapsedTimeText { get; set; } = string.Empty;

    // triggers cancellation and creates tokens
    private CancellationTokenSource? _cts;

    public string ThreadsLabel => $"Using {Threads} {Pluralize("thread", Threads)}";
    public string ExperimentsLabel => $"Running {ExperimentsToRun} {Pluralize("experiment", ExperimentsToRun)}";



    /// <summary>
    ///     Creates a new instance of the <see cref="MainWindowViewModel" /> class
    /// </summary>
    /// <param name="runner">Experiment runner instance</param>
    /// <param name="clock">Clock instance for timing</param>
    public MainWindowViewModel(IRunner runner, IClock clock)
    {
        // TODO
        _runner = runner;
        _clock = clock;
        MaxThreads = Environment.ProcessorCount;
        Threads = MinThreads;
    }

    // TODO

    private static string Pluralize<T>(string noun, T count) where T : INumber<T> => count == T.One ? noun : $"{noun}s";

    public void Dispose()
    {
        _cts?.Dispose();
    }
}

public sealed class DesignMainWindowViewModel() : MainWindowViewModel(null!, null!);
