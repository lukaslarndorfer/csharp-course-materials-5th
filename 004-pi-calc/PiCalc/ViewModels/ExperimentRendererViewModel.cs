using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PiCalc.ViewModels;

/// <summary>
///     A view model for rendering experiment results as colored dots on a bitmap
/// </summary>
public partial class ExperimentRendererViewModel : ViewModelBase
{
    private readonly ConcurrentDictionary<(float X, float Y), int> _dotsBuffer = new();

    /// <summary>
    ///     Creates a new instance of the <see cref="ExperimentRendererViewModel" /> class
    /// </summary>
    public ExperimentRendererViewModel()
    {
        RenderDots = [];
    }

    /// <summary>
    ///     A collection of dots to render, each represented by its location and color
    /// </summary>
    [ObservableProperty]
    public partial IReadOnlyCollection<BitmapDot> RenderDots { get; set; }

    /// <summary>
    ///     Clears the collection of dots to render, resetting the view model state
    /// </summary>
    [MemberNotNull(nameof(RenderDots))]
    public void Clear()
    {
        _dotsBuffer.Clear();
        RenderDots = [];
    }

    /// <summary>
    ///     Sets the <see cref="RenderDots" /> property based on a snapshot of the currently collected
    ///     experiment results.
    /// </summary>
    public void RenderResults()
    {
        RenderDots = new List<BitmapDot>(_dotsBuffer
                                             .Select(d => new BitmapDot(d.Key.X, d.Key.Y, d.Value)));
    }

    /// <summary>
    ///     Adds a new experiment result to the collection.
    ///     Values outside the range [0, 1] for x or y are ignored.
    /// </summary>
    /// <param name="x">The X coordinate of a dot in a 0-1 range</param>
    /// <param name="y">The Y coordinate of a dot in a 0-1 range</param>
    /// <param name="isInsideCircle">A flag indicating if the dot is inside a unit circle or not</param>
    public void AddExperimentResult(double x, double y, bool isInsideCircle)
    {
        if (x is < 0D or > 1D || y is < 0D or > 1D)
        {
            return;
        }

        var c = isInsideCircle ? Colors.Red : Colors.Blue;
        var colorInt = (c.A << 24) | (c.R << 16) | (c.G << 8) | c.B;

        // TODO

        var roundedX = (float) Math.Round(x, 3);
        var roundedY = (float) Math.Round(y, 3);
        _dotsBuffer[(roundedX, roundedY)] = colorInt;
    }

    /// <summary>
    ///     Represents a dot on a bitmap, with its coordinates and color.
    ///     The coordinates are in a 0-1 range with up to three decimal places of precision,
    ///     showing the relative position of the dot on the bitmap and not the absolute pixel position.
    /// </summary>
    /// <param name="X">The relative X coordinate</param>
    /// <param name="Y">The relative Y coordinate</param>
    /// <param name="Color">A value encoding the color information for Bgra8888</param>
    public readonly record struct BitmapDot(float X, float Y, int Color);
}

public sealed class DesignExperimentRendererViewModel() : ExperimentRendererViewModel();
