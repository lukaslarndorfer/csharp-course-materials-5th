using System;
using ColorPicker.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace ColorPicker.ViewModels;

public partial class ColorPickerControlViewModel : ViewModelBase
{
    // TODO

    public ColorPickerControlViewModel(ColorData colorData)
    {
        throw new NotImplementedException();
    }

    // TODO
}

public sealed record ColorDataChanged;

public sealed class DesignColorPickerControlViewModel() : ColorPickerControlViewModel(ColorData.Default);
