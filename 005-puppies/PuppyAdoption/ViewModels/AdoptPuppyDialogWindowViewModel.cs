using System;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using NodaTime;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence;
using PuppyAdoption.Util;
using PuppyAdoption.Views.Dialogs;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="AdoptPuppyDialogWindow" />
/// </summary>
/// <param name="transaction">Transaction provider</param>
/// <param name="toastService">Toast notification service</param>
/// <param name="puppyService">Puppy management service</param>
/// <param name="logger">Application logger</param>
public partial class AdoptPuppyDialogWindowViewModel(
    ITransactionProvider transaction,
    IToastService toastService,
    IPuppyService puppyService,
    ILogger<AdoptPuppyDialogWindowViewModel> logger)
    : PuppyOperationDialogViewModel<AdoptPuppyDialogWindowViewModel.AdoptionInformation?>
{
    // TODO

    /// <summary>
    ///     Represents the adoption information for a puppy
    /// </summary>
    /// <param name="AdoptedByName">Name of the person who adopted the puppy</param>
    /// <param name="AdoptionDate">The date the adoption went through</param>
    public sealed record AdoptionInformation(string AdoptedByName, LocalDate AdoptionDate);

    [ObservableProperty]
    public override partial string PuppyName { get; set; } = string.Empty;
}

public sealed class DesignAdoptPuppyDialogWindowViewModel : AdoptPuppyDialogWindowViewModel
{
    public DesignAdoptPuppyDialogWindowViewModel()
        : base(new DesignFacades.DummyTransactionProvider(),
               new DesignFacades.DummyToastService(),
               new DesignFacades.DummyPuppyService(),
               new LoggerFactory().CreateLogger<AdoptPuppyDialogWindowViewModel>())
    {
        PuppyId = 1;
        PuppyName = "Buddy";
    }
}
