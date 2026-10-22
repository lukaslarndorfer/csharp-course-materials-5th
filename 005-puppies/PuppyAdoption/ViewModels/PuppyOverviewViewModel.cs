using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Shared;
using PuppyAdoption.Util;
using PuppyAdoption.Views;

namespace PuppyAdoption.ViewModels;

/// <summary>
///     View model for the <see cref="PuppyOverview" /> control
/// </summary>
/// <param name="puppyService">Service for managing puppies</param>
/// <param name="logger">Application logger</param>
public partial class PuppyOverviewViewModel(IPuppyService puppyService, ILogger<PuppyOverviewViewModel> logger)
    : ShelterOperationViewModel
{
    // TODO

    [ObservableProperty]
    public partial PuppyListViewModel? PuppyListViewModel { get; set; }
    
    [ObservableProperty]
    public partial PuppyDetailsViewModel? PuppyDetailsViewModel { get; set; }
}

public sealed class DesignPuppyOverviewViewModel()
    : PuppyOverviewViewModel(new DesignFacades.DummyPuppyService(),
                             new DesignFacades.DummyLogger<PuppyOverviewViewModel>());
