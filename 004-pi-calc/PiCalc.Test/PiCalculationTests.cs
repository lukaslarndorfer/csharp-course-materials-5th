using AwesomeAssertions;
using PiCalc.Core;

namespace PiCalc.Test;

public sealed class PiCalculationTests
{
    private readonly IRandomProvider _randomProvider = new RandomProvider(123456UL);

    [Theory]
    [MemberData(nameof(RunsData))]
    public void RunMonteCarloSimulation_Simple(long runs, double expected)
    {
        var calc = new PiCalculation(_randomProvider);
        var result = calc.RunMonteCarloSimulation(runs, null, null, CancellationToken.None);

        result.Should().Be(expected);
    }

    [Theory]
    [MemberData(nameof(VectorRunsData))]
    public void RunMonteCarloSimulation_Vector(long runs, double tolerance)
    {
        var calc = new PiCalculationSIMD(_randomProvider);
        var result = calc.RunMonteCarloSimulation(runs, null, null, CancellationToken.None);

        result.Should().BeApproximately(Math.PI, tolerance);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RunMonteCarloSimulation_ReportsEveryExperiment(bool vectorized)
    {
        const long Runs = 10_000L;

        var reported = new List<(double X, double Y, bool IsInsideCircle)>();
        var progressed = 0L;
        IPiCalculation calc = vectorized
            ? new PiCalculationSIMD(_randomProvider)
            : new PiCalculation(_randomProvider);

        calc.RunMonteCarloSimulation(Runs, batch => progressed += batch, Collect, CancellationToken.None);

        reported.Should().HaveCount((int) Runs);
        reported.Should().OnlyContain(r => r.X >= 0D && r.X < 1D && r.Y >= 0D && r.Y < 1D);
        reported.Should().OnlyContain(r => r.IsInsideCircle == (r.X * r.X + r.Y * r.Y <= 1D),
                                      "the reported flag has to match the reported coordinates");
        reported.Count(r => r.IsInsideCircle).Should().BeInRange((int) (Runs * 0.7D), (int) (Runs * 0.85D),
                                                                "roughly pi/4 of the points land inside");
        progressed.Should().BePositive().And.BeLessThanOrEqualTo(Runs);

        return;

        void Collect(double[] xs, double[] ys, bool[] isInsideCircle)
        {
            for (var i = 0; i < xs.Length; i++)
            {
                reported.Add((xs[i], ys[i], isInsideCircle[i]));
            }
        }
    }

    /// <summary>
    ///     Fixed seed, fixed sequence, fixed result -- the scalar generator is fully deterministic
    /// </summary>
    public static TheoryData<long, double> RunsData() =>
        new()
        {
            { 1_000L, 3.204D },
            { 10_000L, 3.1584D },
            { 100_000L, 3.14684D },
            { 10_000_000L, 3.1411728D },
            { 100_000_000L, 3.14135096D }
        };

    /// <summary>
    ///     No fixed values for the vectorized run: every lane draws from its own stream, so the sequence
    ///     of experiments -- and with it the exact result -- depends on <see cref="System.Numerics.Vector{T}.Count" />,
    ///     which is 4 with AVX2 and 8 with AVX512
    /// </summary>
    public static TheoryData<long, double> VectorRunsData() =>
        new()
        {
            { 1_000L, 0.25D },
            { 10_000L, 0.08D },
            { 100_000L, 0.03D },
            { 10_000_000L, 0.003D },
            { 100_000_000L, 0.001D }
        };
}
