using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PuppyAdoption.Persistence.Model;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     Base class for all view models in the application.
///     Has to be disposable to clean up the service scope used for dependency injection.
/// </summary>
public abstract class ViewModelBase : ObservableObject, IAsyncDisposable
{
    private IServiceScope? _serviceScope;

    public virtual async ValueTask DisposeAsync()
    {
        if (_serviceScope is IAsyncDisposable serviceScopeAsyncDisposable)
        {
            await serviceScopeAsyncDisposable.DisposeAsync();
        }
        else
        {
            _serviceScope?.Dispose();
        }
    }

    /// <summary>
    ///     Attaches a service scope to the view model.
    ///     Only called during view model creation.
    /// </summary>
    /// <param name="serviceScope">The service scope to use</param>
    internal void AttachScope(IServiceScope serviceScope)
    {
        _serviceScope = serviceScope;
    }
}

/// <summary>
///     Base class for dialog view models
/// </summary>
/// <typeparam name="TResult">The result type of the dialog</typeparam>
public abstract class DialogViewModelBase<TResult> : ViewModelBase
{
    /// <summary>
    ///     Called when the dialog is closed, providing the result of the dialog
    /// </summary>
    public event EventHandler<TResult>? CloseRequested;

    /// <summary>
    ///     Closes the dialog and returns the specified result
    /// </summary>
    /// <param name="result"></param>
    protected void Close(TResult result)
    {
        CloseRequested?.Invoke(this, result);
    }
}

/// <summary>
///     Base class for dialog view models that operate on a puppy
/// </summary>
/// <typeparam name="TResult">The result type of the dialog</typeparam>
public abstract class PuppyOperationDialogViewModel<TResult> : DialogViewModelBase<TResult>
{
    /// <summary>
    ///     ID of the puppy that is being operated on
    /// </summary>
    public int PuppyId { get; set; }

    // The name is needed by all derived classes, but since we cannot set the NotifyCanExecuteChangedFor
    // attribute in a subclass for a property defined in a base class, we need to define it abstract here
    // and then do the (partially redundant) implementations in each derived class.
    /// <summary>
    ///     The name of the puppy that is being operated on
    /// </summary>
    public abstract string PuppyName { get; set; }
}

/// <summary>
///     Base class for view models that operate dependent on the currently selected shelter
/// </summary>
public abstract class ShelterOperationViewModel : ViewModelBase
{
    /// <summary>
    ///     The currently selected shelter
    /// </summary>
    protected Shelter? SelectedShelter { get; private set; }

    /// <summary>
    ///     Initializes the view model with the given shelter
    /// </summary>
    /// <param name="shelter">The shelter to use</param>
    public virtual ValueTask InitAsync(Shelter shelter)
    {
        SelectedShelter = shelter;

        return ValueTask.CompletedTask;
    }
}
