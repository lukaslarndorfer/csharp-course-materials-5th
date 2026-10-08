using System;
using ColorPicker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace ColorPicker.ViewModels;

public partial class ColorPickerControlViewModel : ViewModelBase
{
    private readonly ColorData _colorData;

    private readonly ColorDisplayControlViewModel _colorDisplayControlViewModel;

    public string Hex => _colorData.Hex;

    public string Rgba => _colorData.Rgba;

    [ObservableProperty]
    public partial byte Red { get; set; }

    [ObservableProperty]
    public partial byte Green { get; set; }

    [ObservableProperty]
    public partial byte Blue { get; set; }

    [ObservableProperty]
    public partial byte Alpha { get; set; }

    public ColorPickerControlViewModel(ColorData colorData, ColorDisplayControlViewModel colorDisplayControlViewModel)
    {
        _colorData = colorData;
        _colorDisplayControlViewModel = colorDisplayControlViewModel;
        Red = _colorData.Red;
        Green = _colorData.Green;
        Blue = _colorData.Blue;
        Alpha = _colorData.Alpha;
    }

    partial void OnRedChanged(byte value)
    {
        _colorData.Red = value;
        UpdateColorProperties();
    }

    partial void OnGreenChanged(byte value)
    {
        _colorData.Green = value;
        UpdateColorProperties();
    }

    partial void OnBlueChanged(byte value)
    {
        _colorData.Blue = value;
        UpdateColorProperties();
    }

    partial void OnAlphaChanged(byte value)
    {
        _colorData.Alpha = value;
        UpdateColorProperties();
    }

    private void UpdateColorProperties()
    {
        OnPropertyChanged(nameof(Hex));
        OnPropertyChanged(nameof(Rgba));
        _colorDisplayControlViewModel.Refresh();

    }
}

public sealed record ColorDataChanged;

public sealed class DesignColorPickerControlViewModel()
    : ColorPickerControlViewModel(ColorData.Default, new DesignColorDisplayControlViewModel());
