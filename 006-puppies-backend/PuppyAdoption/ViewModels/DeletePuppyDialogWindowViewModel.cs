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
using PuppyAdoption.Util;
using PuppyAdoption.Views.Dialogs;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="DeletePuppyDialogWindow" />
/// </summary>
/// <param name="transaction">Transaction provider</param>
/// <param name="toastService">Toast notification service</param>
/// <param name="puppyService">Puppy management service</param>
/// <param name="logger">Application logger</param>
public partial class DeletePuppyDialogWindowViewModel(
    IPuppyService puppyService,
    ITransactionProvider transaction,
    IToastService toastService,
    ILogger<DeletePuppyDialogWindowViewModel> logger) : PuppyOperationDialogViewModel<bool>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuppyDeletionConfirmationText))]
    public override partial string PuppyName { get; set; } = string.Empty;

    public string PuppyDeletionConfirmationText => $"Are you sure you want to delete {PuppyName} from the system?";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand), nameof(CancelCommand))]
    public partial bool Processing { get; set; }

    private bool CanExecute => !Processing;

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private void Cancel()
    {
        Close(false);
    }

    [RelayCommand(CanExecute = nameof(CanExecute))]
    private async Task Delete()
    {
        Processing = true;
        try
        {
            await transaction.BeginTransactionAsync();
            OneOf<Success, NotFound> result = await puppyService.DeletePuppyAsync(PuppyId);

            await result.Match<ValueTask>(async success =>
            {
                await transaction.CommitAsync();
                toastService.ShowSuccess("Puppy deleted successfully");
                logger.LogInformation("Successfully deleted puppy with Id {Id}", PuppyId);
                Close(true);
            }, async notFound =>
            {
                await transaction.RollbackAsync();
                toastService.ShowError("Couldn't delete puppy");
                logger.LogWarning("Unable to delete puppy with Id {Id}", PuppyId);
                Close(false);
            });
        }
        catch (Exception ex)
        {
            toastService.ShowError("Unexpected error occurred");
            logger.LogError(ex, "Exception occurred while trying to delete puppy");
            Close(false);
        }
        finally
        {
            Processing = false;
        }
    }
}

public sealed class DesignDeletePuppyDialogWindowViewModel : DeletePuppyDialogWindowViewModel
{
    public DesignDeletePuppyDialogWindowViewModel()
        : base(new DesignFacades.DummyPuppyService(),
               new DesignFacades.DummyTransactionProvider(),
               new DesignFacades.DummyToastService(),
               new LoggerFactory().CreateLogger<DeletePuppyDialogWindowViewModel>())
    {
        PuppyId = 1;
        PuppyName = "Buddy";
    }
}