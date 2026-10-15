using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using PiCalc.ViewModels;

namespace PiCalc.Controls;

public sealed partial class ExperimentRenderer : UserControl, IDisposable
{
    private const int DPI = 96;
    private const int DotSize = 3;
    private const int HalfDotSize = DotSize / 2;

    private readonly PixelSize _bitmapSize;
    private readonly Vector _resolution;

    private WriteableBitmap _bitmap;

    public ExperimentRenderer()
    {
        InitializeComponent();

        Size = (int) Math.Min(Width, Height);
        _bitmapSize = new PixelSize(Size, Size);
        _resolution = new Vector(DPI, DPI);
        Clear();

        DotsImage.Width = Size;
        DotsImage.Height = Size;
    }

    private int Size { get; }

    private ExperimentRendererViewModel ViewModel =>
        DataContext as ExperimentRendererViewModel
        ?? throw new InvalidOperationException("DataContext is not the expected view model type");

    public void Dispose()
    {
        _bitmap.Dispose();
        ViewModel.PropertyChanged -= HandlePropertyChanged;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ViewModel.PropertyChanged += HandlePropertyChanged;
    }

    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ExperimentRendererViewModel.RenderDots))
        {
            return;
        }

        RenderResults(ViewModel.RenderDots);
    }

    [MemberNotNull(nameof(_bitmap))]
    private void Clear()
    {
        ReplaceBitmap(CreateBitmap());
    }

    private void RenderResults(IEnumerable<ExperimentRendererViewModel.BitmapDot> dots)
    {
        var bitmap = CreateBitmap();
        using var frameBuffer = bitmap.Lock();
        var baseAddr = frameBuffer.Address;
        var stride = frameBuffer.RowBytes;

        foreach (var dot in dots)
        {
            var px = (int) (dot.X * Size);
            var py = (int) ((1F - dot.Y) * Size);

            for (var dy = -HalfDotSize; dy <= HalfDotSize; dy++)
            {
                var yy = py + dy;
                if (yy < 0 || yy >= Size)
                {
                    continue;
                }

                for (var dx = -HalfDotSize; dx <= HalfDotSize; dx++)
                {
                    var xx = px + dx;
                    if (xx < 0 || xx >= Size)
                    {
                        continue;
                    }

                    if (dx * dx + dy * dy > HalfDotSize * HalfDotSize)
                    {
                        continue;
                    }

                    var offset = yy * stride + xx * 4;
                    Marshal.WriteInt32(baseAddr, offset, dot.Color);
                }
            }
        }

        ReplaceBitmap(bitmap);
    }

    [MemberNotNull(nameof(_bitmap))]
    private void ReplaceBitmap(WriteableBitmap newBitmap)
    {
        var oldBitmap = _bitmap;
        _bitmap = newBitmap;
        oldBitmap?.Dispose();
        DotsImage.Source = _bitmap;
    }

    private WriteableBitmap CreateBitmap() => new(_bitmapSize, _resolution, PixelFormat.Bgra8888, AlphaFormat.Premul);
}
