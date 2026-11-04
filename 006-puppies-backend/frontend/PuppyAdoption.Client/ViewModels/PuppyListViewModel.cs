using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using PuppyAdoption.Client.Views;
using PuppyAdoption.Client.Shared;

namespace PuppyAdoption.Client.ViewModels;

/// <summary>
///     View model for the <see cref="PuppyList" /> control
/// </summary>
public partial class PuppyListViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<PuppyInformation> Puppies { get; set; } = [];

    [ObservableProperty]
    public partial PuppyInformation? SelectedPuppy { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyHint))]
    public partial bool Loading { get; set; }

    public bool HasEntries => Puppies.Count > 0;

    public Action<int?>? SelectedPuppyChanged { get; set; }

    public bool ShowEmptyHint => !HasEntries && !Loading;

    partial void OnSelectedPuppyChanged(PuppyInformation? oldValue, PuppyInformation? newValue)
    {
        SelectedPuppyChanged?.Invoke(newValue?.Id);
    }

    partial void OnPuppiesChanged(ObservableCollection<PuppyInformation> oldValue,
                                  ObservableCollection<PuppyInformation> newValue)
    {
        OnPropertyChanged(nameof(HasEntries));
        OnPropertyChanged(nameof(ShowEmptyHint));
        oldValue.CollectionChanged -= OnCollectionChanged;
        newValue.CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasEntries));
        OnPropertyChanged(nameof(ShowEmptyHint));
    }
}

public sealed class DesignPuppyListViewModel : PuppyListViewModel;