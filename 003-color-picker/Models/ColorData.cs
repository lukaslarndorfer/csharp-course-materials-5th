using System;
using Avalonia.Media;

namespace ColorPicker.Models;

public sealed class ColorData
{
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    public byte Alpha { get; set; }

    public Color Color => new(r: Red, g: Green, b: Blue, a: Alpha);

    public string Hex => $"#{Red:X2}{Green:X2}{Blue:X2}"; // X = hex

    // invariant, always use a dot as decimal separator
    public string Rgba => FormattableString.Invariant($"rgba({Red},{Green},{Blue},{Alpha / 255.0:0.##})");

    public static ColorData Default =>
        new()
        {
            Red = 255,
            Green = 255,
            Blue = 255,
            Alpha = 255
        };

    public static ColorData FromColor(Color color) =>
        new()
        {
            Red = color.R,
            Green = color.G,
            Blue = color.B,
            Alpha = color.A,
        };
}
