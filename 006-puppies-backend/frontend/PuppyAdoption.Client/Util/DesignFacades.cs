using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using OneOf;
using OneOf.Types;
using PuppyAdoption.Client.Core.Services;
using PuppyAdoption.Client.Shared;
using PuppyAdoption.Client.ViewModels;
using PuppyAdoption.Shared;

namespace PuppyAdoption.Client.Util;

/// <summary>
///     Provides dummy implementations of various services for design-time use
/// </summary>
public static class DesignFacades
{
    public sealed class DummyLogger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                                Func<TState, Exception?, string> formatter)
        { }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }

    public sealed class DummyShelterService : IShelterService
    {
        public ValueTask<OneOf<Success<ShelterDto>, IShelterService.NameAlreadyExists>>
            CreateNewShelterAsync(string name) =>
            throw new NotImplementedException();

        public ValueTask<IReadOnlyList<ShelterDto>> GetAllSheltersAsync() =>
            ValueTask.FromResult<IReadOnlyList<ShelterDto>>([
                new ShelterDto(1, "Happy Paws Shelter"),
                new ShelterDto(2, "Best Friends Institution")
            ]);
    }

    public sealed class DummyPuppyService : IPuppyService
    {
        public ValueTask<OneOf<Success<PuppyInformationResponse>, NotFound>> GetAllPuppiesAsync(int shelterId) =>
            ValueTask.FromResult<OneOf<Success<PuppyInformationResponse>, NotFound>>(
                new Success<PuppyInformationResponse>(new PuppyInformationResponse([
                    new PuppyInformationDto(1, "Buddy", false),
                    new PuppyInformationDto(2, "Bella", true)
                ])));

        public ValueTask<OneOf<Success<PuppyDto>, NotFound>> AdmitPuppyAsync(
            int shelterId, string name, string breed, LocalDate dateOfBirth, byte[]? imageData) =>
            throw new NotImplementedException();

        public ValueTask<OneOf<PuppyDto, NotFound>> GetPuppyByIdAsync(int puppyId) =>
            throw new NotImplementedException();

        public ValueTask<OneOf<Success, NotFound>> DeletePuppyAsync(int puppyId) => throw new NotImplementedException();

        public ValueTask<OneOf<Success<LocalDate>, NotFound, IPuppyService.AlreadyAdopted>>
            AdoptPuppyAsync(int puppyId, string adopterName) =>
            throw new NotImplementedException();
    }

    public sealed class DummyToastService : IToastService
    {
        public void ShowMessage(string message, string title, NotificationType type)
        {
            Console.WriteLine($"Toast: [{title}] {message}");
        }

        public void ShowError(string message) => ShowMessage(message, "Error", NotificationType.Error);

        public void ShowWarning(string message) => ShowMessage(message, "Warning", NotificationType.Warning);

        public void ShowSuccess(string message) => ShowMessage(message, "Success", NotificationType.Success);

        public void SetWindow(Window window) { }
    }

    public sealed class DummyDialogService : IDialogService
    {
        public Task<TResult>
            ShowModalDialogAsync<TDialogWindow, TDialogViewModel, TResult>(
                Action<TDialogViewModel>? viewModelConfiguration = null) where TDialogWindow : Window, new()
                                                                         where TDialogViewModel :
                                                                         DialogViewModelBase<TResult> =>
            Task.FromResult(default(TResult)!);

        public void SetOwner(Window window) { }
    }

    public sealed class DummySettings : IOptions<Settings>
    {
        public Settings Value =>
            new()
            {
                TimeZoneId = "Europe/Vienna",
                MaxImageSizePixel = 400,
                ApiBaseUrl = "http://localhost:5200/api/"
            };
    }
}