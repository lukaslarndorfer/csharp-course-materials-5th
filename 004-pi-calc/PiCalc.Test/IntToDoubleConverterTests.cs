using System.Globalization;
using AwesomeAssertions;
using PiCalc.Util;

namespace PiCalc.Test;

public sealed class IntToDoubleConverterTests
{
    private readonly IntToDoubleConverter _sut = new();

    [Fact]
    public void Convert_Success()
    {
        const int Input = 42;
        const double Expected = 42D;

        var result = _sut.Convert(Input, typeof(double), null, CultureInfo.InvariantCulture);

        result.Should()
              .BeOfType<double>()
              .And.Be(Expected);
    }

    [Theory]
    [MemberData(nameof(InvalidDataTypeData), false, true)]
    public void Convert_InvalidDataType(object value)
    {
        var result = _sut.Convert(value, value.GetType(), null, CultureInfo.InvariantCulture);

        result.Should()
              .BeOfType<double>()
              .And.Be(0D, "For invalid input, the converter should always return 0D");
    }

    [Fact]
    public void ConvertBack_Success()
    {
        const double Input = 41.5D;
        const int Expected = 42;

        var result = _sut.ConvertBack(Input, typeof(int), null, CultureInfo.InvariantCulture);

        result.Should()
              .BeOfType<int>()
              .And.Be(Expected, "Rounded before casting");
    }

    [Theory]
    [MemberData(nameof(InvalidDataTypeData), true, false)]
    public void ConvertBack_InvalidDataType(object value)
    {
        var result = _sut.ConvertBack(value, value.GetType(), null, CultureInfo.InvariantCulture);

        result.Should()
              .BeOfType<int>()
              .And.Be(0, "For invalid input, the converter should always return 0");
    }

    public static IEnumerable<TheoryDataRow<object>> InvalidDataTypeData(bool skipDouble, bool skipInt)
    {
        IEnumerable<TheoryDataRow<object>> tests =
        [
            new(1L) { TestDisplayName = CreateDisplayName("long") },
            new((short) 2) { TestDisplayName = CreateDisplayName("short") },
            new((byte) 4) { TestDisplayName = CreateDisplayName("byte") },
            new((uint) 8) { TestDisplayName = CreateDisplayName("uint") },
            new(true) { TestDisplayName = CreateDisplayName("bool") },
            new(64M) { TestDisplayName = CreateDisplayName("decimal") },
            new("hello") { TestDisplayName = CreateDisplayName("string") },
            new(new object()) { TestDisplayName = CreateDisplayName("object") }
        ];
        foreach (var test in tests)
        {
            yield return test;
        }

        if (!skipDouble)
        {
            yield return new TheoryDataRow<object>(16D) { TestDisplayName = CreateDisplayName("double") };
        }

        if (!skipInt)
        {
            yield return new TheoryDataRow<object>(32) { TestDisplayName = CreateDisplayName("int") };
        }

        yield break;

        static string CreateDisplayName(string inputType) => $"{inputType} input";
    }
}
