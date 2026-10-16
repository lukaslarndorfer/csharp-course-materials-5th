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
    // TODO
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
