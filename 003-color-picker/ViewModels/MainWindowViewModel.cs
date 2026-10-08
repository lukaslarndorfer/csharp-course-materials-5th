using ColorPicker.Models;

namespace ColorPicker.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        var colorData = ColorData.Default;

        PickerViewModel = new ColorPickerControlViewModel(colorData);
        ComplementaryViewModel = new ComplementaryColorsControlViewModel(colorData);
    }

    public ColorPickerControlViewModel PickerViewModel { get; }
    public ComplementaryColorsControlViewModel ComplementaryViewModel { get; }
}
