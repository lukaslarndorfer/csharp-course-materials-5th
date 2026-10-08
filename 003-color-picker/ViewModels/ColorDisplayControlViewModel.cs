using System;
using Avalonia.Media;
using ColorPicker.Models;

namespace ColorPicker.ViewModels;

public class ColorDisplayControlViewModel(ColorData data, bool overlayHex) : ViewModelBase
{
    // TODO

    public Brush Color => throw new NotImplementedException();
    public Brush HexOverlayColor => throw new NotImplementedException();
    public string HexOverlay => throw new NotImplementedException();

    public void Refresh(ColorData? newData = null)
    {
        throw new NotImplementedException();
    }

    private static Color GetContrastingColor(Color background)
    {
        var r = CalcRelativeLuminance(background.R);
        var g = CalcRelativeLuminance(background.G);
        var b = CalcRelativeLuminance(background.B);

        // Rec. 709 luminance
        var luminance = 0.2126D * r + 0.7152D * g + 0.0722D * b;

        return luminance < 0.5D ? Colors.White : Colors.Black;

        static double CalcRelativeLuminance(byte value)
        {
            var luminance = value / 255D;
            var gammaCorrected = luminance <= 0.03928D
                ? luminance / 12.92D
                : Math.Pow((luminance + 0.055D) / 1.055D, 2.4D);

            return gammaCorrected;
        }
    }
}

public sealed class DesignColorDisplayControlViewModel() : ColorDisplayControlViewModel(ColorData.Default, true);
