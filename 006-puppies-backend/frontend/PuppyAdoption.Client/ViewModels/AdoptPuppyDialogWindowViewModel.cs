using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NodaTime;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Client.Util;
using PuppyAdoption.Client.Views.Dialogs;
using PuppyAdoption.Client.Core.Services;

namespace PuppyAdoption.Client.ViewModels;

/// <summary>
///     View model for the <see cref="AdoptPuppyDialogWindow" />
/// </summary>
/// <param name="toastService">Toast notification service</param>
/// <param name="puppyService">Puppy management service</param>
/// <param name="logger">Application logger</param>
public partial class AdoptPuppyDialogWindowViewModel(
    IToastService toastService,
    IPuppyService puppyService,
    ILogger<AdoptPuppyDialogWindowViewModel> logger)
    : PuppyOperationDialogViewModel<AdoptPuppyDialogWindowViewModel.AdoptionInformation?>
{
    /// <summary>
    ///     Represents the adoption information for a puppy
    /// </summary>
    /// <param name="AdoptedByName">Name of the person who adopted the puppy</param>
    /// <param name="AdoptionDate">The date the adoption went through</param>
    public sealed record AdoptionInformation(string AdoptedByName, LocalDate AdoptionDate);

    [ObservableProperty]
    public override partial string PuppyName { get; set; } = string.Empty;

    /// <summary>
    ///     Name of the person adopting the puppy
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AdoptCommand))]
    public partial string AdopterName { get; set; } = string.Empty;

    /// <summary>
    ///     Flag indicating whether the dialog is currently processing
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AdoptCommand), nameof(CancelCommand))]
    public partial bool Processing { get; set; }

    private bool CanCancel => !Processing;
    private bool CanAdopt => !string.IsNullOrWhiteSpace(AdopterName) && !Processing;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        Close(null);
    }

    [RelayCommand(CanExecute = nameof(CanAdopt))]
    private async Task Adopt()
    {
        Processing = true;
        string adopterName = AdopterName.Trim();
        try
        {
            OneOf<Success<LocalDate>, NotFound, IPuppyService.AlreadyAdopted> result
                = await puppyService.AdoptPuppyAsync(PuppyId, adopterName);
            result.Switch(success =>
            {
                logger.LogInformation("Puppy with id {Id} successfully adopted at {Date}", PuppyId, success.Value);
                toastService.ShowMessage($"Puppy {PuppyName} has been adopted successfully!",
                                         "Adoption Successful", NotificationType.Success);
                Close(new AdoptionInformation(adopterName, success.Value));
            }, notFound =>
            {
                logger.LogWarning("Puppy with id {Id} not found", PuppyId);
                toastService.ShowError("Puppy wasn't found");
                Close(null);
            }, alreadyAdopted =>
            {
                logger.LogWarning("Puppy with id {Id} is already adopted", PuppyId);
                toastService.ShowError("Puppy is already adopted!");
                Close(null);
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while trying to adopt puppy");
            toastService.ShowError("Error occurred while trying to adopt puppy");
            Close(null);
        }
        finally
        {
            Processing = false;
        }
    }
}

public sealed class DesignAdoptPuppyDialogWindowViewModel : AdoptPuppyDialogWindowViewModel
{
    public DesignAdoptPuppyDialogWindowViewModel()
        : base(new DesignFacades.DummyToastService(),
               new DesignFacades.DummyPuppyService(),
               new LoggerFactory().CreateLogger<AdoptPuppyDialogWindowViewModel>())
    {
        PuppyId = 1;
        PuppyName = "Buddy";
    }
}
