using AwesomeAssertions;
using NodaTime;
using NSubstitute;
using PiCalc.Core;
using PiCalc.ViewModels;

namespace PiCalc.Test;

public sealed class MainWindowsViewModelTests
{
    [Fact]
    public async Task StartCalculation_SetsValues()
    {
        // a bit flaky, testing with threads is always 'fun'

        var (sut, resultTrigger) = CreateSut();

        sut.StartCalculationCommand.CanExecute(null).Should().BeTrue();
        sut.StopCalculationCommand.CanExecute(null).Should().BeFalse();

        // deliberately *not* awaited here, the mocked runner only completes once resultTrigger is
        // called => awaiting now would block forever, see the readme for the details
        var execution = sut.StartCalculationCommand.ExecuteAsync(null);

        sut.StartCalculationCommand.CanExecute(null).Should().BeFalse();
        sut.StopCalculationCommand.CanExecute(null).Should().BeTrue();

        resultTrigger(3.14D);
        await execution;

        sut.StartCalculationCommand.CanExecute(null).Should().BeTrue();
        sut.StopCalculationCommand.CanExecute(null).Should().BeFalse();

        // only set if the view model dispatched to the UI thread *synchronously* => Invoke, not
        // InvokeAsync/Post, because nothing pumps the dispatcher queue in a unit test
        sut.ResultValue.Should().Be("3.14");
    }

    private static SutObjects CreateSut()
    {
        var runner = CreateMockRunner(out var resultTrigger);
        var clock = Substitute.For<IClock>();
        var vm = new MainWindowViewModel(runner, clock);

        return new SutObjects(vm, resultTrigger);
    }

    private static IRunner CreateMockRunner(out Action<double> resultTrigger)
    {
        var tcs = new TaskCompletionSource<double>();
        resultTrigger = result => tcs.SetResult(result);

        var mock = Substitute.For<IRunner>();
        mock.RunSimulation(Arg.Any<int>(),
                           Arg.Any<long>(),
                           Arg.Any<bool>(),
                           Arg.Any<IRunner.ProgressReport?>(),
                           Arg.Any<IRunner.ExperimentResultReport?>(),
                           Arg.Any<CancellationToken>())
            .Returns(new ValueTask<double>(tcs.Task));

        return mock;
    }

    private readonly record struct SutObjects(MainWindowViewModel Sut, Action<double> ResultTrigger);
}