using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using OneOf.Types;
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Shared;
using PuppyAdoption.Util;
using PuppyAdoption.Views;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="PuppyAdmission" /> control
/// </summary>
/// <param name="imageProcessor">Service for processing images</param>
/// <param name="puppyService">Puppy management service</param>
/// <param name="dateTimeProvider">A provider for current date & time</param>
/// <param name="logger">Application logger</param>
/// <param name="toastService">Toast notification service</param>
/// <param name="options">Application settings</param>
public partial class PuppyAdmissionViewModel(
    IImageProcessor imageProcessor,
    IPuppyService puppyService,
    IDateTimeProvider dateTimeProvider,
    ILogger<PuppyAdmissionViewModel> logger,
    IToastService toastService,
    IOptions<Settings> options,
    ITransactionProvider transaction) : ShelterOperationViewModel
{
    public int MaxImageSize { get; } = options.Value.MaxImageSizePixel;

    [ObservableProperty]
    public partial Bitmap? ImagePreview { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAdmissionCommand))]
    public partial string PuppyName { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAdmissionCommand))]
    public partial string Breed { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAdmissionCommand))]
    public partial LocalDate DateOfBirth { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddAdmissionCommand))]
    public partial bool Processing { get; set; } = false;

    private byte[]? _imageData;

    private bool CanAddAdmission =>
        !Processing && SelectedShelter is not null
                    && PuppyName.Trim().Length > 0
                    && Breed.Trim().Length > 0;

    public override async ValueTask InitAsync(Shelter shelter)
    {
        await base.InitAsync(shelter);
        DateOfBirth = dateTimeProvider.GetLocalDate(); // today
    }

    [RelayCommand(CanExecute = nameof(CanAddAdmission))]
    private async Task AddAdmission()
    {
        Processing = true;

        if (SelectedShelter is null)
        {
            toastService.ShowError("No Shelter is selected");
            Processing = false;
            return;
        }

        await transaction.BeginTransactionAsync();
        try
        {
            var admitResult
                = await puppyService.AdmitPuppyAsync(SelectedShelter, PuppyName, Breed, DateOfBirth, _imageData);
            toastService.ShowSuccess("Shelter admitted puppy!");

            await transaction.CommitAsync();
            logger.LogInformation("Successfully admitted puppy with Id {Id}", admitResult.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create shelter");
            toastService.ShowError("Failed to create shelter. Please try again.");
            await transaction.RollbackAsync();
        }
        finally
        {
            Processing = false;
        }
    }

    public void SetImageData(byte[] imageData)
    {
        _imageData = imageProcessor.ProcessImage(imageData);
        
        // convert the processed image bytes into a Bitmap to update the preview
        using var input = new MemoryStream(_imageData);
        var decoded = new Bitmap(input);
        var oldPreview = ImagePreview;
        ImagePreview = decoded;
        oldPreview?.Dispose(); // dispose the old bitmap after replacing it to release its resources
    }
}
