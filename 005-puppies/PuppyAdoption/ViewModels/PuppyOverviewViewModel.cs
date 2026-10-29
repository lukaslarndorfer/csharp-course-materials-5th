using System;
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

    private ObservableCollection<PuppyInformation> _puppies = [];
    

    public override async ValueTask InitAsync(Shelter shelter)
    {
        await base.InitAsync(shelter);
        PuppyListViewModel = App.CreateViewModel<PuppyListViewModel>();
        PuppyDetailsViewModel = App.CreateViewModel<PuppyDetailsViewModel>();
        PuppyListViewModel.Loading = true;
        _puppies = [.. await puppyService.GetAllPuppiesAsync(shelter)];
        PuppyListViewModel.Loading = false;
        PuppyListViewModel.Puppies = _puppies;

        logger.LogDebug("Received {Count} puppies",_puppies.Count);
        PuppyListViewModel.SelectedPuppyChanged = async void (puppyId) =>
        {
            try
            {
                if (puppyId is null)
                {
                    PuppyDetailsViewModel.Clear();

                    return;
                }

                await PuppyDetailsViewModel.InitDetailsAsync((int) puppyId, puppyAdoptedHandler: () =>
                {
                    var puppy = _puppies.FirstOrDefault(p => p.Id == puppyId);
                    if (puppy is null)
                    {
                        logger.LogError("No Puppy with Id {Id} found", puppyId);

                        return;
                    }

                    var copy = puppy with { Adopted = true };
                    int idx = _puppies.IndexOf(puppy);
                    _puppies[idx] = copy;
                    PuppyListViewModel.SelectedPuppy = copy;
                }, puppyDeletedHandler: () =>
                {
                    var puppy = _puppies.FirstOrDefault(p => p.Id == puppyId);
                    if (puppy is null)
                    {
                        logger.LogError("No Puppy with Id {Id} found", puppyId);

                        return;
                    }
                    _puppies.Remove(puppy);
                    PuppyListViewModel.SelectedPuppy = null;
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle selected puppy changed event");
            }
        };
        
        
    }

    public override async ValueTask DisposeAsync()
    {
        if (PuppyListViewModel != null)
        {
            await PuppyListViewModel.DisposeAsync();
        }

        if (PuppyDetailsViewModel != null)
        {
            await PuppyDetailsViewModel.DisposeAsync();
        }

        await base.DisposeAsync();
        
    }

    [ObservableProperty]
    public partial PuppyListViewModel? PuppyListViewModel { get; set; }
    
    [ObservableProperty]
    public partial PuppyDetailsViewModel? PuppyDetailsViewModel { get; set; }
}

public sealed class DesignPuppyOverviewViewModel()
    : PuppyOverviewViewModel(new DesignFacades.DummyPuppyService(),
                             new DesignFacades.DummyLogger<PuppyOverviewViewModel>());
