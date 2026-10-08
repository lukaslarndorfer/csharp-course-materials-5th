using System;
using Avalonia.Media;
using ColorPicker.Models;
using CommunityToolkit.Mvvm.Messaging;

namespace ColorPicker.ViewModels;

public class ComplementaryColorsControlViewModel : ViewModelBase, IRecipient<ColorDataChanged>
{
    private readonly ColorData _colorData;

    public ColorDisplayControlViewModel Complement { get; }
    public ColorDisplayControlViewModel SplitLeft { get; }
    public ColorDisplayControlViewModel SplitRight { get; }

    public ComplementaryColorsControlViewModel(ColorData colorData)
    {
        _colorData = colorData;
        
        Complement = new ColorDisplayControlViewModel(_colorData, true);
        SplitLeft = new ColorDisplayControlViewModel(_colorData, true);
        SplitRight = new ColorDisplayControlViewModel(_colorData, true);
        UpdateColors();
        
        // whether this view model is active
        IsActive = true;
    }

    private static Color[] GetSplitComplements(Color baseColor)
    {
        const double SplitAngle = 30D;

        var hsv = ColorToHSV(baseColor);

        var hC = (hsv.Hue + 180D) % 360D;
        var hS1 = (hC - SplitAngle + 360D) % 360D;
        var hS2 = (hC + SplitAngle) % 360D;

        var s2 = Math.Min(1, hsv.Saturation * 0.8D);
        var v2 = Math.Min(1, hsv.Value * 0.9D + 0.05D);

        return
        [
            HSVtoColor(new HSV(hC, s2, v2), baseColor.A),
            HSVtoColor(new HSV(hS1, s2, v2), baseColor.A),
            HSVtoColor(new HSV(hS2, s2, v2), baseColor.A)
        ];
    }

    private static HSV ColorToHSV(Color color)
    {
        double r = color.R / 255D, g = color.G / 255D, b = color.B / 255D;
        double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;
        var saturation = max == 0D ? 0D : delta / max;
        double hue;
        if (delta == 0D)
        {
            hue = 0D;
        }
        else
        {
            if (Math.Abs(max - r) < double.Epsilon)
            {
                hue = 60D * ((g - b) / delta % 6D);
            }
            else if (Math.Abs(max - g) < double.Epsilon)
            {
                hue = 60D * ((b - r) / delta + 2D);
            }
            else
            {
                hue = 60D * ((r - g) / delta + 4D);
            }

            if (hue < 0D)
            {
                hue += 360D;
            }
        }

        return new HSV(hue, saturation, max);
    }

    private static Color HSVtoColor(HSV hsv, byte alpha = 255)
    {
        var c = hsv.Value * hsv.Saturation;
        var x = c * (1D - Math.Abs(hsv.Hue / 60D % 2D - 1D));
        var m = hsv.Value - c;
        var (r, g, b) = hsv.Hue switch
                        {
                            < 60D  => (c, x, 0D),
                            < 120D => (x, c, 0D),
                            < 180D => (0D, c, x),
                            < 240D => (0D, x, c),
                            < 300D => (x, 0D, c),
                            _      => (c, 0D, x)
                        };

        return Color.FromArgb(alpha, Map(r), Map(g), Map(b));

        byte Map(double value) => (byte) Math.Round((value + m) * 255D);
    }

    private readonly ref struct HSV(double hue, double saturation, double value)
    {
        public double Hue => hue;
        public double Saturation => saturation;
        public double Value => value;
    }

    public void Receive(ColorDataChanged message)
    {
        UpdateColors();
    }

    private void UpdateColors()
    {
        Color[] splitComplements = GetSplitComplements(_colorData.Color);
        var complementData = ColorData.FromColor(splitComplements[0]);
        var splitLeftData = ColorData.FromColor(splitComplements[1]);
        var splitRightData = ColorData.FromColor(splitComplements[2]);
        
        Complement.Refresh(complementData);
        SplitLeft.Refresh(splitLeftData);
        SplitRight.Refresh(splitRightData);
    }
}

public sealed class DesignComplementaryColorsControlViewModel()
    : ComplementaryColorsControlViewModel(ColorData.Default);
