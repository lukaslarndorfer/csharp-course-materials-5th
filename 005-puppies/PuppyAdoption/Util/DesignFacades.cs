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
using PuppyAdoption.Core.Services;
using PuppyAdoption.Persistence;
using PuppyAdoption.Persistence.Model;
using PuppyAdoption.Shared;
using PuppyAdoption.ViewModels;

namespace PuppyAdoption.Util;

/// <summary>
///     Provides dummy implementations of various services for design-time use
/// </summary>
public static class DesignFacades
{
    public sealed class DummyLogger<T> : ILogger<T>
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                                Func<TState, Exception?, string> formatter) { }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }

    public sealed class DummyShelterService : IShelterService
    {
        public ValueTask<OneOf<Success<Shelter>, IShelterService.NameAlreadyExists>>
            CreateNewShelterAsync(string name) =>
            throw new NotImplementedException();

        public ValueTask<OneOf<Shelter, NotFound>> GetShelterByIdAsync(int id) => throw new NotImplementedException();

        public ValueTask<IReadOnlyCollection<Shelter>> GetAllSheltersAsync() =>
            ValueTask.FromResult<IReadOnlyCollection<Shelter>>([
                new Shelter { Id = 1, Name = "Happy Paws Shelter", Puppies = []},
                new Shelter { Id = 2, Name = "Best Friends Institution", Puppies = []}
            ]);
    }

    public sealed class DummyPuppyService : IPuppyService
    {
        public ValueTask<IReadOnlyCollection<PuppyInformation>> GetAllPuppiesAsync(Shelter shelter) =>
            ValueTask.FromResult<IReadOnlyCollection<PuppyInformation>>([
                new PuppyInformation(1, "Buddy", false),
                new PuppyInformation(2, "Bella", true)
            ]);

        public ValueTask<Puppy> AdmitPuppyAsync(Shelter shelter, string name, string breed, LocalDate dateOfBirth,
                                                byte[]? imageData) =>
            throw new NotImplementedException();

        public ValueTask<OneOf<Puppy, NotFound>> GetPuppyByIdAsync(int puppyId) => throw new NotImplementedException();
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
                Action<TDialogViewModel>? viewModelConfiguration = null) where TDialogWindow : ScopedWindow, new()
                                                                         where TDialogViewModel :
                                                                         DialogViewModelBase<TResult> =>
            Task.FromResult(default(TResult)!);

        public void SetOwner(Window window) { }
    }

    public sealed class DummyTransactionProvider : ITransactionProvider
    {
        public ValueTask BeginTransactionAsync() => ValueTask.CompletedTask;

        public ValueTask CommitAsync() => ValueTask.CompletedTask;

        public ValueTask RollbackAsync() => ValueTask.CompletedTask;

        public void Dispose()
        {
            // TODO release managed resources here
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    public sealed class DummySettings : IOptions<Settings>
    {
        public Settings Value =>
            new()
            {
                TimeZoneId = "Europe/Vienna",
                MaxImageSizePixel = 400
            };
    }
}
