using Avalonia.Controls;
using Avalonia.Input;

namespace ColorPicker.Controls;

public sealed partial class ColorPickerControl : UserControl
{
    public ColorPickerControl()
    {
        InitializeComponent();
    }

    private void HandleTextSelection(object? sender, TappedEventArgs e)
    {
        if (sender is SelectableTextBlock stb)
        {
            stb.SelectAll();
        }
    }
}

