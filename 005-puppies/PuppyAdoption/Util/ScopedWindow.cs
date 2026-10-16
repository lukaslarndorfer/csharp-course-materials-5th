using Avalonia.Controls;
using PuppyAdoption.ViewModels;

namespace PuppyAdoption.Util;

/// <summary>
///     A base class for windows that have their own dependency scope.
///     Will automatically dispose of the view model when the window is closed.
/// </summary>
public abstract class ScopedWindow : Window
{
    protected ScopedWindow()
    {
        Closed += async (_, _) =>
        {
            if (DataContext is ViewModelBase viewModel)
            {
                await viewModel.DisposeAsync();
            }
        };
    }
}
