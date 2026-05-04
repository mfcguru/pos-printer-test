using FluentAssertions;
using Moq;
using POSPrinterTest.Web.Data;

namespace POSPrinterTest.Tests;

public class PrintServiceTests
{
    private readonly Mock<IPrinterService> _printerServiceMock = new();
    private readonly Mock<IRawPrinter> _rawPrinterMock = new();
    private readonly PrintService _sut;

    public PrintServiceTests()
    {
        _sut = new PrintService(_printerServiceMock.Object, _rawPrinterMock.Object);
    }

    [Fact]
    public async Task PrintAsync_UnknownPrinter_ReturnsFailure()
    {
        _printerServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Printer?)null);

        var result = await _sut.PrintAsync(42, "test content");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("42");
    }

    [Fact]
    public async Task PrintAsync_KnownPrinter_ReturnsSuccess()
    {
        var printer = new Printer { Id = 1, Name = "Office Printer", ConnectionType = "Network", ConnectionString = "10.0.0.5" };
        _printerServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(printer);

        var result = await _sut.PrintAsync(1, "\\x1B\\x40Hello\\x0A");

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Office Printer");
        _rawPrinterMock.Verify(r => r.Print("10.0.0.5", It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public async Task PrintAsync_RawPrinterThrows_ReturnsFailure()
    {
        var printer = new Printer { Id = 1, Name = "Office Printer", ConnectionType = "Network", ConnectionString = "10.0.0.5" };
        _printerServiceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(printer);
        _rawPrinterMock
            .Setup(r => r.Print(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Throws(new InvalidOperationException("Cannot open printer"));

        var result = await _sut.PrintAsync(1, "\\x1B\\x40Hello\\x0A");

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Print failed");
    }

    [Fact]
    public void BuildPrintBytes_EscapeSequences_ParsedToBinary()
    {
        var bytes = PrintService.BuildPrintBytes(@"\x1B\x40\x48\x65\x6C\x6C\x6F");

        bytes.Should().Equal(0x1B, 0x40, 0x48, 0x65, 0x6C, 0x6C, 0x6F);
    }

    [Fact]
    public void BuildPrintBytes_EscapeSequencesWithBarcode_DecodesAndRendersImage()
    {
        var content = "\\x1B\\x40\nBarcode(576,150,123456)\n\\x1D\\x56\\x41\\x03";
        var bytes = PrintService.BuildPrintBytes(content);

        // ESC @ decoded
        bytes[0].Should().Be(0x1B);
        bytes[1].Should().Be(0x40);

        // Barcode rendered as GS v 0 raster image
        var byteList = bytes.ToList();
        var gsV0 = Enumerable.Range(0, byteList.Count - 2)
            .Any(i => byteList[i] == 0x1D && byteList[i + 1] == 0x76 && byteList[i + 2] == 0x30);
        gsV0.Should().BeTrue("GS v 0 raster image command should be present");

        // GS V A 3 partial cut decoded and appended after barcode
        bytes[^4].Should().Be(0x1D);
        bytes[^3].Should().Be(0x56);
        bytes[^2].Should().Be(0x41);
        bytes[^1].Should().Be(0x03);
    }

    [Fact]
    public void BuildPrintBytes_EmptyContent_ReturnsEmpty()
    {
        var bytes = PrintService.BuildPrintBytes("   ");

        bytes.Should().BeEmpty();
    }

    [Fact]
    public void BuildPrintBytes_BarcodeTagOnly_ContainsGsV0Command()
    {
        var bytes = PrintService.BuildPrintBytes("Barcode(576,150,123456)");

        var byteList = bytes.ToList();
        var hasGsV0 = Enumerable.Range(0, byteList.Count - 2)
            .Any(i => byteList[i] == 0x1D && byteList[i + 1] == 0x76 && byteList[i + 2] == 0x30);

        hasGsV0.Should().BeTrue("GS v 0 raster image command should be present");
        bytes.Length.Should().BeGreaterThan(20, "output should contain image data");
    }

    [Fact]
    public void BuildPrintBytes_BarcodeWithInvalidHriValue_ThrowsFormatException()
    {
        var act = () => PrintService.BuildPrintBytes("Barcode(400,150,123456,XXXX)");
        act.Should().Throw<FormatException>().WithMessage("*Invalid barcode tag*");
    }

    [Theory]
    [InlineData("Barcode(400,150,123456,9)")]
    [InlineData("Barcode(400,150,123456,2)")]
    public void BuildPrintBytes_BarcodeWithHriValueOtherThan0Or1_ThrowsFormatException(string content)
    {
        var act = () => PrintService.BuildPrintBytes(content);
        act.Should().Throw<FormatException>().WithMessage("*Invalid barcode tag*");
    }

    [Fact]
    public void BuildPrintBytes_BarcodeWithoutHriParam_DefaultsToNoHri()
    {
        var omitted   = PrintService.BuildPrintBytes("Barcode(400,150,123456)");
        var explicit0 = PrintService.BuildPrintBytes("Barcode(400,150,123456,0)");

        omitted.Should().Equal(explicit0);
    }

    [Theory]
    [InlineData("barcode(400,150,123456)")]
    [InlineData("BARCODE(400,150,123456)")]
    [InlineData("bArCoDE(400,150,123456)")]
    public void BuildPrintBytes_BarcodeTagCaseInsensitive_ContainsGsV0Command(string content)
    {
        var bytes = PrintService.BuildPrintBytes(content);

        var byteList = bytes.ToList();
        var hasGsV0 = Enumerable.Range(0, byteList.Count - 2)
            .Any(i => byteList[i] == 0x1D && byteList[i + 1] == 0x76 && byteList[i + 2] == 0x30);

        hasGsV0.Should().BeTrue("GS v 0 raster image command should be present");
    }

    [Fact]
    public void BuildPrintBytes_FullReceipt_DecodesFramingAndRendersBarcode()
    {
        // Realistic receipt: init, center-align, barcode image, partial cut
        var content = "\\x1B\\x40\n\\x1B\\x61\\x01\nBarcode(400,150,ABC123,1)\n\\x1D\\x56\\x41\\x03";
        var bytes = PrintService.BuildPrintBytes(content);

        bytes.Should().Contain(0x1B); // ESC @
        bytes.Should().Contain(0x40);
        bytes.Should().Contain(0x61); // ESC a 1 center

        var byteList = bytes.ToList();
        var gsV0 = Enumerable.Range(0, byteList.Count - 2)
            .Any(i => byteList[i] == 0x1D && byteList[i + 1] == 0x76 && byteList[i + 2] == 0x30);
        gsV0.Should().BeTrue("barcode raster image should be present");

        bytes.Should().Contain(0x56); // GS V A 3 partial cut
        bytes.Should().Contain(0x41);
        bytes.Should().Contain(0x03);
    }
}
