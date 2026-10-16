using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Util;
using PuppyAdoption.Views;
using PuppyAdoption.Views.Dialogs;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="MainWindow" /> main window of the application
/// </summary>
/// <param name="shelterService">Shelter management service</param>
/// <param name="toastService">Toast notification service</param>
/// <param name="dialogService">Dialog management service</param>
/// <param name="logger">Application logger</param>
public partial class MainWindowViewModel(
    IShelterService shelterService,
    IDialogService dialogService,
    IToastService toastService,
    ILogger<MainWindowViewModel> logger) : ViewModelBase
{
    /// <summary>
    ///     The tab name for the overview tab
    /// </summary>
    public static string OverviewTab => "Overview";

    /// <summary>
    ///     The tab name for the admissions tab
    /// </summary>
    public static string AdmissionsTab => "Admission";

    /// <summary>
    ///     The view model for the overview tab
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowTabContent))]
    public partial PuppyOverviewViewModel? OverviewViewModel { get; set; }

    /// <summary>
    ///     The view model for the admissions tab
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowTabContent))]
    public partial PuppyAdmissionViewModel? AdmissionViewModel { get; set; }

    /// <summary>
    ///     An observable collection of all shelters
    /// </summary>
    public ObservableCollection<Shelter> Shelters { get; } = [];

    /// <summary>
    ///     The currently selected shelter
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanActivateTab))]
    public partial Shelter? SelectedShelter { get; set; }

    /// <summary>
    ///     A flag indicating whether the application is currently processing an operation
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanActivateTab), nameof(CanCreateShelter), nameof(ShowMainContent))]
    [NotifyCanExecuteChangedFor(nameof(CreateShelterCommand))]
    public partial bool Processing { get; set; } = false;

    /// <summary>
    ///     A flag indicating whether tabs can currently be activated
    /// </summary>
    public bool CanActivateTab => SelectedShelter is not null && !Processing;

    private bool CanCreateShelter => !Processing;

    /// <summary>
    ///     A flag indicating whether the tab content should be shown
    /// </summary>
    public bool ShowTabContent => OverviewViewModel is not null && AdmissionViewModel is not null;

    /// <summary>
    ///     A flag indicating whether the main content should be shown
    /// </summary>
    public bool ShowMainContent => !Processing;

    [RelayCommand]
    private async Task InitAsync()
    {
        try
        {
            Processing = true;
            IReadOnlyCollection<Shelter> allShelters = await shelterService.GetAllSheltersAsync();
            foreach (var shelter in allShelters)
            {
                Shelters.Add(shelter);
            }

            if (Shelters.Count > 0)
            {
                SelectedShelter = Shelters[0];
                OverviewViewModel = App.CreateViewModel<PuppyOverviewViewModel>();
                AdmissionViewModel = App.CreateViewModel<PuppyAdmissionViewModel>();
                await ActivateTabAsync(OverviewTab, SelectedShelter);
            }
            else
            {
                toastService.ShowMessage("Please add a shelter to get started.", "No shelters found");
                logger.LogInformation("No shelters found, selected shelter not set");
            }
        }
        finally
        {
            Processing = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateShelter))]
    private async Task CreateShelterAsync()
    {
        var result = await dialogService
            .ShowModalDialogAsync<CreateShelterDialogWindow, CreateShelterDialogWindowViewModel, Shelter?>();
        if (result is null)
        {
            return;
        }

        Shelters.Add(result);
        SelectedShelter = result;
    }

    [RelayCommand]
    private async Task HandleTabSelectionChanged(SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count < 1
            || e.AddedItems[0] is not TabItem { Tag: string tab }
            || string.IsNullOrWhiteSpace(tab)
            || SelectedShelter is null)
        {
            return;
        }

        try
        {
            Processing = true;
            await ActivateTabAsync(tab, SelectedShelter);
        }
        finally
        {
            Processing = false;
        }
    }

    private async ValueTask ActivateTabAsync(string tab, Shelter selectedShelter)
    {
        ShelterOperationViewModel? operationViewModel = null;
        if (tab == OverviewTab)
        {
            if (OverviewViewModel is not null)
            {
                await OverviewViewModel.DisposeAsync();
            }

            OverviewViewModel = App.CreateViewModel<PuppyOverviewViewModel>();
            operationViewModel = OverviewViewModel;
        }
        else if (tab == AdmissionsTab)
        {
            if (AdmissionViewModel is not null)
            {
                await AdmissionViewModel.DisposeAsync();
            }

            AdmissionViewModel = App.CreateViewModel<PuppyAdmissionViewModel>();
            operationViewModel = AdmissionViewModel;
        }
        else
        {
            logger.LogError("Tab selection changed but an invalid tab {Tab} was selected", tab);
            toastService.ShowError("No valid tab selected");
        }

        if (operationViewModel is not null)
        {
            await operationViewModel.InitAsync(selectedShelter);
        }
    }

    async partial void OnSelectedShelterChanged(Shelter? value)
    {
        try
        {
            logger.LogDebug("Selected shelter changed to {Shelter}", value?.Id);
            if (value is null)
            {
                return;
            }

            await ActivateTabAsync(OverviewTab, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling selected shelter change");
        }
    }
}

public sealed class DesignMainWindowViewModel()
    : MainWindowViewModel(new DesignFacades.DummyShelterService(),
                          new DesignFacades.DummyDialogService(),
                          new DesignFacades.DummyToastService(),
                          new DesignFacades.DummyLogger<MainWindowViewModel>());
