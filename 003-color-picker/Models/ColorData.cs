using System;
using Avalonia.Media;

namespace ColorPicker.Models;

public sealed class ColorData
{
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    public byte Alpha { get; set; }

    // TODO
    public Color Color => throw new NotImplementedException();
    public string Hex => throw new NotImplementedException();
    public string Rgba
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public static ColorData Default => throw new NotImplementedException();
    public static ColorData FromColor(Color color) => throw new NotImplementedException();
}
