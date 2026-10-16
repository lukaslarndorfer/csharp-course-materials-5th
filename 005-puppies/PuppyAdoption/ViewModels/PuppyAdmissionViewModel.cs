using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using PuppyAdoption.Core.Services;
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
    IOptions<Settings> options) : ShelterOperationViewModel
{
    // TODO
}
