using Avalonia.Controls;
using PuppyAdoption.ViewModels;

namespace PuppyAdoption.Util;

/// <summary>
///     A base class for user controls that have their own dependency scope.
///     Will automatically dispose of the view model when the control is detached from the visual tree.
/// </summary>
public abstract class ScopedControl : UserControl
{
    protected ScopedControl()
    {
        DetachedFromVisualTree += async (_, _) =>
        {
            if (DataContext is ViewModelBase viewModel)
            {
                await viewModel.DisposeAsync();
            }
        };
    }
}
