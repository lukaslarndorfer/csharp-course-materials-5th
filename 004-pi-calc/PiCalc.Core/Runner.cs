using Microsoft.Extensions.DependencyInjection;

namespace PiCalc.Core;

/// <summary>
///     Allows to run the Monte Carlo simulation for approximating Pi
/// </summary>
public interface IRunner
{
    /// <summary>
    ///     Called to report the results of each experiment.
    ///     Depending on the implementation, the arrays may contain one or multiple values,
    ///     where those of equal index represent a single experiment.
    /// </summary>
    public delegate void ExperimentResultReport(double[] xValues, double[] yValues, bool[] isInsideCircle);

    /// <summary>
    ///     Called to report the current progress of the simulation
    /// </summary>
    public delegate void ProgressReport(long processed, long total);

    /// <summary>
    ///     Runs the simulation configured by the parameters.
    ///     If more threads are requested than experiments, the number of threads will be reduced to the number of experiments.
    /// </summary>
    /// <param name="threads">Number of threads to use</param>
    /// <param name="experiments">Number of experiments to perform (overall)</param>
    /// <param name="vectorized">Flag indicating if the vectorized implementation should be used</param>
    /// <param name="progressReport">Optional callback for progress reports</param>
    /// <param name="experimentResultReport">Optional callback for reporting experiment results</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The approximated value of Pi</returns>
    public ValueTask<double> RunSimulation(int threads, long experiments, bool vectorized,
                                           ProgressReport? progressReport,
                                           ExperimentResultReport? experimentResultReport,
                                           CancellationToken cancellationToken);
}

internal sealed class Runner(IServiceProvider services) : IRunner
{
    public async ValueTask<double> RunSimulation(int threads, long experiments, bool vectorized,
                                                 IRunner.ProgressReport? progressReport,
                                                 IRunner.ExperimentResultReport? experimentResultReport,
                                                 CancellationToken cancellationToken)
    {
        RunProgressCounter? counter;
        IPiCalculation.ProgressReport? reportHandler = null;

        if (progressReport is not null)
        {
            counter = new RunProgressCounter(experiments, progressReport);
            reportHandler = batchProcessed => counter.IncrementProcessed(batchProcessed);
        }

        if (threads > experiments)
        {
            threads = (int) experiments;
        }

        var runners = BatchRunsPerThread(experiments, threads)
                      .Select(ex => Task.Run(() =>
                      {
                          var calculator = services.GetKeyedService<IPiCalculation>(vectorized ? IPiCalculation.VectorCalculation : IPiCalculation.BasicCalculation);

                          double? result = calculator?.RunMonteCarloSimulation(ex, reportHandler, experimentResultReport,
                                                                               cancellationToken);

                          return (approxPi: result  ?? 0, Count: ex);
                      }, cancellationToken)).ToList();
        await Task.WhenAll(runners);

        double weightedResultSum = runners.Sum(r => r.Result.approxPi * r.Result.Count);

        return weightedResultSum / experiments;
    }

    private static IEnumerable<long> BatchRunsPerThread(long experiments, int threads)
    {
        if (threads <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threads), "Threads must be greater than zero.");
        }

        var runsPerThread = experiments / threads;
        var remainder = experiments % threads;

        for (var i = 0; i < threads; i++)
        {
            if (i == threads - 1)
            {
                yield return runsPerThread + remainder;

                continue;
            }

            yield return runsPerThread;
        }
    }

    private sealed class RunProgressCounter(long total, IRunner.ProgressReport uiReport)
    {
        private long _processed;

        public void IncrementProcessed(long batchProcessed)
        {
            var newValue = Interlocked.Add(ref _processed, batchProcessed);
            uiReport(newValue, total);
        }
    }
}
