using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Util;
using PuppyAdoption.Views.Dialogs;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="CreateShelterDialogWindow" />
/// </summary>
/// <param name="shelterService">Service for managing shelters</param>
/// <param name="transaction">Transaction provider</param>
/// <param name="toastService">Toast notification service</param>
/// <param name="logger">Application logger</param>
public partial class CreateShelterDialogWindowViewModel(
    IShelterService shelterService,
    ITransactionProvider transaction,
    IToastService toastService,
    ILogger<CreateShelterDialogWindowViewModel> logger) : DialogViewModelBase<Shelter?>
{
    /// <summary>
    ///     Flag indicating whether the dialog is currently processing
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand), nameof(CancelCommand))]
    public partial bool Processing { get; set; } = false;

    private bool CanCancel => !Processing;
    private bool CanCreate => !string.IsNullOrWhiteSpace(ShelterName) && !Processing;

    /// <summary>
    ///     Name of the shelter to be created
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CreateCommand))]
    public partial string ShelterName { get; set; } = string.Empty;

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private async Task Create()
    {
        if (string.IsNullOrWhiteSpace(ShelterName))
        {
            return;
        }

        Processing = true;
        await transaction.BeginTransactionAsync();
        try
        {
            OneOf<Success<Shelter>, IShelterService.NameAlreadyExists> shelterResult
                = await shelterService.CreateNewShelterAsync(ShelterName);
            await shelterResult.Match<ValueTask>(async success =>
                                                 {
                                                     await transaction.CommitAsync();
                                                     toastService.ShowSuccess("Shelter created successfully!");
                                                     Close(success.Value);
                                                 },
                                                 async nameExists =>
                                                 {
                                                     await transaction.RollbackAsync();
                                                     toastService
                                                         .ShowMessage($"Shelter with name '{ShelterName}' already exists",
                                                                      "Name exists", NotificationType.Warning);
                                                 });
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

    [RelayCommand(CanExecute = nameof(CanCancel))]
    private void Cancel()
    {
        Close(null);
    }
}

public sealed class DesignCreateShelterDialogWindowViewModel()
    : CreateShelterDialogWindowViewModel(new DesignFacades.DummyShelterService(),
                                         new DesignFacades.DummyTransactionProvider(),
                                         new DesignFacades.DummyToastService(),
                                         new DesignFacades.DummyLogger<CreateShelterDialogWindowViewModel>());
