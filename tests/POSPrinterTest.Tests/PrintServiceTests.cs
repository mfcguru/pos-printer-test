using FluentAssertions;
using Moq;
using POSPrinterTest.Web.Data;

namespace POSPrinterTest.Tests;

public class PrintServiceTests
{
    private readonly Mock<IPrinterService> _printerServiceMock = new();
    private readonly PrintService _sut;

    public PrintServiceTests()
    {
        _sut = new PrintService(_printerServiceMock.Object);
    }

    [Fact]
    public async Task PrintAsync_UnknownPrinter_ReturnsFailure()
    {
        // Arrange
        _printerServiceMock
            .Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Printer?)null);

        // Act
        var result = await _sut.PrintAsync(42, "test content");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("42");
    }

    [Fact]
    public async Task PrintAsync_KnownPrinter_ReturnsSuccess()
    {
        // Arrange
        var printer = new Printer { Id = 1, Name = "Office Printer", ConnectionType = "Network", ConnectionString = "10.0.0.5" };
        _printerServiceMock
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(printer);

        // Act
        var result = await _sut.PrintAsync(1, "Hello POS");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Office Printer");
    }
}
