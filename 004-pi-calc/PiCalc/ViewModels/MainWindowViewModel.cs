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

    /// <summary>
    ///     Creates a new instance of the <see cref="MainWindowViewModel" /> class
    /// </summary>
    /// <param name="runner">Experiment runner instance</param>
    /// <param name="clock">Clock instance for timing</param>
    public MainWindowViewModel(IRunner runner, IClock clock)
    {
        // TODO
    }

    // TODO

    private static string Pluralize<T>(string noun, T count) where T : INumber<T> => count == T.One ? noun : $"{noun}s";
}

public sealed class DesignMainWindowViewModel() : MainWindowViewModel(null!, null!);
