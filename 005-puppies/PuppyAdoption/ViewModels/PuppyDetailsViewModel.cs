using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Text;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Shared;
using PuppyAdoption.Util;
using PuppyAdoption.Views;
using PuppyAdoption.Views.Dialogs;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="PuppyDetails" /> control
/// </summary>
/// <param name="puppyService">Service for managing puppies</param>
/// <param name="toastService">Toast notification service</param>
/// <param name="dialogService">Dialog management service</param>
/// <param name="options">Application settings</param>
public partial class PuppyDetailsViewModel(
    IPuppyService puppyService,
    IToastService toastService,
    IDialogService dialogService,
    IOptions<Settings> options) : ViewModelBase
{
    private static readonly LocalDatePattern datePattern = LocalDatePattern.CreateWithInvariantCulture("dd.MM.yyyy");
    private Action? _puppyAdoptedHandler;
    private Action? _puppyDeletedHandler;

    /// <summary>
    ///     The maximum size of the image in pixels (both width and height)
    /// </summary>
    public int MaxImageSize { get; } = options.Value.MaxImageSizePixel;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowProgressBar), nameof(ShowDetails))]
    private partial bool Loading { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowDetails))]
    [NotifyCanExecuteChangedFor(nameof(AdoptPuppyCommand), nameof(DeletePuppyCommand))]
    private partial bool DataSet { get; set; }

    /// <summary>
    ///     Flag indicating whether the progress bar should be shown
    /// </summary>
    public bool ShowProgressBar => Loading;

    /// <summary>
    ///     Flag indicating whether the details of the puppy should be shown
    /// </summary>
    public bool ShowDetails => !ShowProgressBar && DataSet;

    [ObservableProperty]
    private partial int Id { get; set; }

    /// <summary>
    ///     The name of the puppy
    /// </summary>
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The breed of the puppy
    /// </summary>
    [ObservableProperty]
    public partial string Breed { get; set; } = string.Empty;

    /// <summary>
    ///     The date of birth of the puppy
    /// </summary>
    [ObservableProperty]
    public partial LocalDate DateOfBirth { get; set; }

    /// <summary>
    ///     The date the puppy arrived at the shelter
    /// </summary>
    [ObservableProperty]
    public partial LocalDate DateArrived { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AdoptionInfo), nameof(CanBeAdopted), nameof(CanBeDeleted))]
    [NotifyCanExecuteChangedFor(nameof(AdoptPuppyCommand), nameof(DeletePuppyCommand))]
    private partial LocalDate? DateAdopted { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AdoptionInfo))]
    private partial string AdoptedBy { get; set; } = string.Empty;

    private bool CanBeAdopted => DataSet && DateAdopted is null;
    private bool CanBeDeleted => DataSet && DateAdopted is not null;

    /// <summary>
    ///     Information about the adoption status of the puppy
    /// </summary>
    public string AdoptionInfo =>
        DateAdopted is null
            ? string.Empty
            : $"Adopted by {AdoptedBy} on {datePattern.Format(DateAdopted.Value)}";

    /// <summary>
    ///     The image of the puppy
    /// </summary>
    [ObservableProperty]
    public partial Bitmap? Image { get; set; }

    /// <summary>
    ///     Resets the view model to its initial state
    /// </summary>
    public void Clear()
    {
        _puppyAdoptedHandler = null;
        _puppyDeletedHandler = null;
        DataSet = false;
        Loading = false;
        Id = 0;
        Name = string.Empty;
        Breed = string.Empty;
        DateOfBirth = default(LocalDate);
        DateArrived = default(LocalDate);
        DateAdopted = null;
        AdoptedBy = string.Empty;
        SetImage(null);
    }

    /// <summary>
    ///     Initializes the view model with the details of a specific puppy
    /// </summary>
    /// <param name="puppyId">The ID of the puppy for which to display details</param>
    /// <param name="puppyAdoptedHandler">A callback for after adoption operations</param>
    /// <param name="puppyDeletedHandler">A callback for after deletion operations</param>
    public async ValueTask InitDetailsAsync(int puppyId, Action puppyAdoptedHandler, Action puppyDeletedHandler)
    {
        // TODO
        await ValueTask.CompletedTask;
        throw new NotImplementedException();
    }

    private void SetImage(byte[]? imageData)
    {
        // TODO
        throw new NotImplementedException();
    }

    [RelayCommand(CanExecute = nameof(CanBeDeleted))]
    private async Task DeletePuppyAsync()
    {
        // TODO
        await ValueTask.CompletedTask;
        throw new NotImplementedException();
    }

    [RelayCommand(CanExecute = nameof(CanBeAdopted))]
    private async Task AdoptPuppyAsync()
    {
        // TODO
        await ValueTask.CompletedTask;
        throw new NotImplementedException();
    }

    private void SetDialogData<TResult>(PuppyOperationDialogViewModel<TResult> viewModel)
    {
        viewModel.PuppyId = Id;
        viewModel.PuppyName = Name;
    }
}

public sealed class DesignPuppyDetailsViewModel : PuppyDetailsViewModel
{
    public DesignPuppyDetailsViewModel() : base(new DesignFacades.DummyPuppyService(),
                                                new DesignFacades.DummyToastService(),
                                                new DesignFacades.DummyDialogService(),
                                                new DesignFacades.DummySettings())
    {
        Name = "Buddy";
        Breed = "Golden Retriever";
        DateOfBirth = new LocalDate(2024, 04, 02);
        DateArrived = new LocalDate(2025, 05, 12);
        Image = null;

        var baseType = typeof(PuppyDetailsViewModel);
        SetPrivateProperty("Id", 1);
        SetPrivateProperty("DataSet", true);
        SetPrivateProperty("DateAdopted", (LocalDate?) null);
        SetPrivateProperty("AdoptedBy", string.Empty);

        return;

        void SetPrivateProperty<T>(string propertyName, T value)
        {
            baseType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(this, value);
        }
    }
}
