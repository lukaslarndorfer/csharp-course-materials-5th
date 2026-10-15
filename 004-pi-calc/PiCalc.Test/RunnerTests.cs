using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using PiCalc.Core;

namespace PiCalc.Test;

public sealed class RunnerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RunSimulation_EndToEndThroughDi(bool vectorized)
    {
        var services = new ServiceCollection();
        services.AddCoreServices();
        var provider = services.BuildServiceProvider();
        var runner = provider.GetRequiredService<IRunner>();

        var reported = 0L;
        var lastProgress = 0D;
        var pi = await runner.RunSimulation(8, 4_000_000L, vectorized,
                                            (p, t) => lastProgress = (double) p / t,
                                            (x, _, _) => Interlocked.Add(ref reported, x.Length),
                                            CancellationToken.None);

        pi.Should().BeApproximately(Math.PI, 0.01D);
        reported.Should().Be(4_000_000L);
        lastProgress.Should().Be(1D);

        // separate instances must not share generator state
        var a = provider.GetRequiredService<IRandomProvider>();
        var b = provider.GetRequiredService<IRandomProvider>();
        a.Should().NotBeSameAs(b);
        a.NextDoublePair().Should().NotBe(b.NextDoublePair());
    }
}
