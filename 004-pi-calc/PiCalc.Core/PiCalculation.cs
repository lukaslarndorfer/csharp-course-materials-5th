using System.Numerics;

namespace PiCalc.Core;

/// <summary>
///     Allows to run a Monte Carlo simulation to approximate the value of Pi
/// </summary>
internal interface IPiCalculation
{
    /// <summary>
    ///     Called to report the current progress of the simulation
    /// </summary>
    public delegate void ProgressReport(long batchProcessed);

    public const long ProgressUpdates = 200L;
    public const string BasicCalculation = "Basic";
    public const string VectorCalculation = "SIMD";

    /// <summary>
    ///     Runs a Monte Carlo simulation to approximate the value of Pi
    /// </summary>
    /// <param name="experiments">The number of experiments to run</param>
    /// <param name="progressReport">An optional callback for reporting progress updates</param>
    /// <param name="experimentResultReport">An optional callback for reporting each experiment's result</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The approximated value of Pi</returns>
    public double RunMonteCarloSimulation(long experiments, ProgressReport? progressReport,
                                          IRunner.ExperimentResultReport? experimentResultReport,
                                          CancellationToken cancellationToken);
}

internal sealed class PiCalculation(IRandomProvider random) : IPiCalculation
{
    public double RunMonteCarloSimulation(long experiments, IPiCalculation.ProgressReport? progressReport,
                                          IRunner.ExperimentResultReport? experimentResultReport,
                                          CancellationToken cancellationToken)
    {
        long progressInterval = progressReport is null
            ? 0
            : experiments / Math.Min(experiments, IPiCalculation.ProgressUpdates);

        var lastReportedRuns = 0L;
        long insideCircle = 0;

        for (var i = 0; i < experiments && !cancellationToken.IsCancellationRequested; i++)
        {
            var isInsideCircle = false;
            var coordinates = random.NextDoublePair();

            if (coordinates.X * coordinates.X + coordinates.Y * coordinates.Y <= 1.0)
            {
                isInsideCircle = true;
                insideCircle++;
            }

            experimentResultReport?.Invoke([coordinates.X], [coordinates.Y], [isInsideCircle]);

            long delta = i - lastReportedRuns;
            if (progressReport is null ||
                delta < progressInterval)
            {
                continue;
            }

            progressReport(delta);
            lastReportedRuns = i;
        }
        if (progressReport is not null && experiments > lastReportedRuns)
        {
            progressReport(experiments - lastReportedRuns);
        }
        
        return (double) insideCircle / experiments * 4D;
    }
}

internal sealed class PiCalculationSIMD(IRandomProvider random) : IPiCalculation
{
    private static readonly int vectorLength = Vector<double>.Count;
    private static readonly Vector<double> oneVecFloat = new(1D);
    private static readonly Vector<long> oneVecLong = new(1L);

    public double RunMonteCarloSimulation(long experiments, IPiCalculation.ProgressReport? progressReport,
                                          IRunner.ExperimentResultReport? experimentResultReport,
                                          CancellationToken cancellationToken)
    {
        var progressInterval = progressReport is null
            ? 0
            : experiments / Math.Min(experiments, IPiCalculation.ProgressUpdates);
        var lastReportedRuns = 0L;
        var insideCircle = 0L;
        var runs = experiments / vectorLength * vectorLength;
        var insideCircleVec = Vector<long>.Zero;
        var reportBuffers = experimentResultReport is not null;
        Span<double> xs = reportBuffers ? stackalloc double[vectorLength] : Span<double>.Empty;
        Span<double> ys = reportBuffers ? stackalloc double[vectorLength] : Span<double>.Empty;
        var isInsideCircle = reportBuffers ? stackalloc bool[vectorLength] : Span<bool>.Empty;
        for (long i = 0; !cancellationToken.IsCancellationRequested && i < runs; i += vectorLength)
        {
            var vx = random.NextDoubleVector();
            var vy = random.NextDoubleVector();
            var magnitudeSquared = vx * vx + vy * vy;
            var mask = Vector.LessThanOrEqual(magnitudeSquared, oneVecFloat);
            var bits = Vector.BitwiseAnd(mask, oneVecLong);
            insideCircleVec += bits;
            if (experimentResultReport is not null)
            {
                vx.CopyTo(xs);
                vy.CopyTo(ys);
                for (var j = 0; j < vectorLength; j++)
                {
                    isInsideCircle[j] = bits[j] != 0L;
                }

                experimentResultReport(xs.ToArray(), ys.ToArray(), isInsideCircle.ToArray());
            }

            var delta = i - lastReportedRuns;
            if (progressReport is not null &&
                delta >= progressInterval)
            {
                progressReport(delta);
                lastReportedRuns = i;
            }
        }

        insideCircle += Vector.Sum(insideCircleVec);

        for (var i = runs; i < experiments; i++)
        {
            var (x, y) = random.NextDoublePair();
            var isInside = x * x + y * y <= 1D;
            if (isInside)
            {
                insideCircle++;
            }

            experimentResultReport?.Invoke([x], [y], [isInside]);
            // not reporting progress for the last <8 runs
        }
        if (progressReport is not null && experiments > lastReportedRuns)
        {
            progressReport(experiments - lastReportedRuns);
        }
        
        return (double) insideCircle / experiments * 4D;
    }
}
