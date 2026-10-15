using AwesomeAssertions;
using PiCalc.ViewModels;

namespace PiCalc.Test;

public sealed class ExperimentRendererViewModelTests
{
    private const int RedColorInt = -16776961;
    private const int BlueColorInt = -65536;
    private readonly ExperimentRendererViewModel _sut = new();

    [Fact]
    public void Clear_Simple()
    {
        _sut.AddExperimentResult(0.5D, 0.6D, true);
        _sut.RenderResults();
        var dots1 = _sut.RenderDots;

        _sut.Clear();
        var dots2 = _sut.RenderDots;

        dots1.Should().NotBeEmpty()
             .And.HaveCount(1);
        dots1.Should().NotBeSameAs(dots2);
    }

    [Fact]
    public void Clear_Empty()
    {
        // mind the empty case: an empty ToArray() hands back the cached Array.Empty singleton,
        // so RenderResults has to build a list to return a fresh instance every time
        _sut.RenderResults();
        var dots1 = _sut.RenderDots;

        _sut.Clear();
        var dots2 = _sut.RenderDots;

        dots1.Should().NotBeSameAs(dots2);
    }

    [Fact]
    public void AddExperimentResult_Single()
    {
        const double X = 0.123456780D;
        const double Y = 0.201111119D;
        const bool IsInsideCircle = false;

        _sut.AddExperimentResult(X, Y, IsInsideCircle);
        _sut.RenderResults();
        var dots = _sut.RenderDots;

        dots.Should().NotBeEmpty()
            .And.HaveCount(1);
        ValidateBitmapDot(0.123F, 0.201F, RedColorInt, dots.First());
    }

    [Fact]
    public void AddExperimentResult_Multiple()
    {
        const double X1 = 0.25D;
        const double X2 = 0.253333D;
        const double Y1 = 0.75D;
        const double Y2 = 0.756666D;
        const bool IsInsideCircle1 = true;
        const bool IsInsideCircle2 = false;

        _sut.AddExperimentResult(X1, Y1, IsInsideCircle1);
        _sut.AddExperimentResult(X2, Y2, IsInsideCircle2);
        _sut.RenderResults();
        var dots = _sut.RenderDots;

        dots.Should().NotBeEmpty()
            .And.HaveCount(2);
        var firstDot = dots.First(d => d.Color == BlueColorInt);
        var secondDot = dots.First(d => d.Color == RedColorInt);
        ValidateBitmapDot((float) X1, (float) Y1, BlueColorInt, firstDot);
        ValidateBitmapDot(0.253F, 0.757F, RedColorInt, secondDot);
    }

    [Theory]
    [InlineData(-0.1D, 0.5D)]
    [InlineData(1.1D, 0.5D)]
    [InlineData(0.5D, -0.1D)]
    [InlineData(0.5D, 1.1D)]
    public void AddExperimentResult_InvalidValues(double x, double y)
    {
        _sut.AddExperimentResult(x, y, true);
        _sut.RenderResults();
        _sut.RenderDots.Should().BeEmpty("invalid value supplied, should be ignored");
    }

    private static void ValidateBitmapDot(float expectedX, float expectedY, int expectedColorInt,
                                          ExperimentRendererViewModel.BitmapDot dot)
    {
        const float Tolerance = 0.001F;

        dot.X.Should().BeApproximately(expectedX, Tolerance);
        dot.Y.Should().BeApproximately(expectedY, Tolerance);
        dot.Color.Should().Be(expectedColorInt);
    }
}
