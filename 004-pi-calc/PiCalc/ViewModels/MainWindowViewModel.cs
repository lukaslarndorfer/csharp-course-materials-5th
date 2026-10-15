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
    [ObservableProperty]
    public partial ExperimentRendererViewModel ExperimentRendererViewModel { get; set; }

    private readonly IClock _clock;
    private readonly IRunner _runner;

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
    public partial string ResultValue { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ElapsedTimeText { get; set; } = string.Empty;

    private bool CanStartCalculation() => !IsRunning;
    private bool CanStopCalculation() => IsRunning;

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
        _runner = runner;
        _clock = clock;
        MaxThreads = Environment.ProcessorCount;
        Threads = MinThreads;

        // for dot rendering
        ExperimentRendererViewModel = new ExperimentRendererViewModel();
    }

    private static string Pluralize<T>(string noun, T count) where T : INumber<T> => count == T.One ? noun : $"{noun}s";

    public void Dispose()
    {
        _cts?.Dispose();
    }

    private void OnProgressReported(long processed, long total)
    {
        double progress = (double) processed / total;

        Dispatcher.UIThread.Invoke(() => ProgressValue = progress);
    }

    private void OnResultRendered(double[] x, double[] y, bool[] inside)
    {
        for (var i = 0; i < x.Length; i++)
        {
            ExperimentRendererViewModel.AddExperimentResult(x[i], y[i], inside[i]);
        }

    }

    [RelayCommand(CanExecute = nameof(CanStartCalculation))]
    private async Task StartCalculationAsync()
    {
        ResultValue = string.Empty;
        ProgressValue = 0D;
        ElapsedTimeText = string.Empty;
        ExperimentRendererViewModel.Clear();
        if (_cts is not null)
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }

        _cts = new CancellationTokenSource();
        IsRunning = true;
        StartCalculationCommand.NotifyCanExecuteChanged();
        StopCalculationCommand.NotifyCanExecuteChanged();
        try
        {
            var start = _clock.GetCurrentInstant();
            double result = await Task.Run(async () => await _runner.RunSimulation(Threads,
                                                ExperimentsToRun,
                                                IsVectorized,
                                                IsProgressReportingEnabled
                                                    ? OnProgressReported
                                                    : null,
                                                IsResultRenderingEnabled ? OnResultRendered : null,
                                                _cts.Token));
            if (IsResultRenderingEnabled)
            {
                RenderResultsOnUiThread();
            }

            var end = _clock.GetCurrentInstant();
            var elapsed = end.Minus(start);
            var pattern = DurationPattern.CreateWithInvariantCulture("HH:mm:ss.FFF");
            ElapsedTimeText = pattern.Format(elapsed);
            ResultValue = result.ToString(CultureInfo.InvariantCulture);
        }
        catch (OperationCanceledException)
        {
            // token triggered
        }
        finally
        {
            IsRunning = false;
            StartCalculationCommand.NotifyCanExecuteChanged();
            StopCalculationCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(CanStopCalculation))]
    private void StopCalculation()
    {
        _cts?.Cancel();
    }

    private void RenderResultsOnUiThread() =>
        Dispatcher.UIThread.Invoke(() => ExperimentRendererViewModel.RenderResults());

    partial void OnExperimentsToRunChanged(long value)
    {
        const long minExperiments = 1L;

        if (value < minExperiments)
        {
            ExperimentsToRun = minExperiments;
        }
    }
}

public sealed class DesignMainWindowViewModel() : MainWindowViewModel(null!, null!);
