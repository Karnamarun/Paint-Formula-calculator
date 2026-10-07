using PaintTintCalculator.Domain.Exceptions;
using PaintTintCalculator.Domain.ValueObjects;
using Xunit;

namespace PaintTintCalculator.Domain.Tests;

public class CanSizeTests
{
    [Theory]
    [InlineData(1.0, 1000.0)]
    [InlineData(4.0, 4000.0)]
    [InlineData(10.0, 10000.0)]
    [InlineData(20.0, 20000.0)]
    public void Create_AcceptsSupportedCanSizes_AndComputesVolumeInMl(decimal litres, decimal expectedMl)
    {
        var canSize = CanSize.Create(litres);

        Assert.Equal(litres, canSize.Litres);
        Assert.Equal(expectedMl, canSize.VolumeInMl);
        Assert.Equal($"{litres}L", canSize.ToString());
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(2.0)]
    [InlineData(5.0)]
    [InlineData(15.0)]
    [InlineData(-4.0)]
    public void Create_ThrowsInvalidCanSizeException_ForUnsupportedSizes(decimal unsupportedSize)
    {
        var ex = Assert.Throws<InvalidCanSizeException>(() => CanSize.Create(unsupportedSize));

        Assert.Equal(unsupportedSize, ex.CanSizeLitres);
        Assert.Contains("1L, 4L, 10L, 20L", ex.Message);
        Assert.Equal("INVALID_CAN_SIZE", ex.Code);
    }

    [Fact]
    public void SupportedLitres_ContainsExactlyFourSizes()
    {
        Assert.Equal(new[] { 1.0m, 4.0m, 10.0m, 20.0m }, CanSize.SupportedLitres);
    }
}

