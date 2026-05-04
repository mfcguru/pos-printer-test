using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using POSPrinterTest.Web.Data;

namespace POSPrinterTest.Tests;

public class PrinterServiceTests
{
    private static AppDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task GetAllAsync_ReturnsAllPrinters()
    {
        // Arrange
        using var db = CreateDbContext();
        db.Printers.AddRange(
            new Printer { Name = "Printer A", ConnectionType = "Network", ConnectionString = "192.168.1.1" },
            new Printer { Name = "Printer B", ConnectionType = "USB", ConnectionString = "USB001" }
        );
        await db.SaveChangesAsync();
        var service = new PrinterService(db);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectPrinter()
    {
        // Arrange
        using var db = CreateDbContext();
        var printer = new Printer { Name = "Test Printer", ConnectionType = "Network", ConnectionString = "10.0.0.1" };
        db.Printers.Add(printer);
        await db.SaveChangesAsync();
        var service = new PrinterService(db);

        // Act
        var result = await service.GetByIdAsync(printer.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Printer");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new PrinterService(db);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_AddsPrinter()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new PrinterService(db);
        var input = new PrinterInputModel
        {
            Name = "New Printer",
            ConnectionType = "Serial",
            ConnectionString = "COM1"
        };

        // Act
        await service.CreateAsync(input);

        // Assert
        var printers = await db.Printers.ToListAsync();
        printers.Should().HaveCount(1);
        printers[0].Name.Should().Be("New Printer");
        printers[0].ConnectionType.Should().Be("Serial");
        printers[0].ConnectionString.Should().Be("COM1");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPrinter()
    {
        // Arrange
        using var db = CreateDbContext();
        var printer = new Printer { Name = "Old Name", ConnectionType = "USB", ConnectionString = "USB001" };
        db.Printers.Add(printer);
        await db.SaveChangesAsync();
        var service = new PrinterService(db);
        var input = new PrinterInputModel
        {
            Name = "New Name",
            ConnectionType = "Network",
            ConnectionString = "192.168.0.1"
        };

        // Act
        await service.UpdateAsync(printer.Id, input);

        // Assert
        var updated = await db.Printers.FindAsync(printer.Id);
        updated.Should().NotBeNull();
        updated!.Name.Should().Be("New Name");
        updated.ConnectionType.Should().Be("Network");
        updated.ConnectionString.Should().Be("192.168.0.1");
    }

    [Fact]
    public async Task DeleteAsync_RemovesPrinter()
    {
        // Arrange
        using var db = CreateDbContext();
        var printer = new Printer { Name = "To Delete", ConnectionType = "USB", ConnectionString = "USB001" };
        db.Printers.Add(printer);
        await db.SaveChangesAsync();
        var service = new PrinterService(db);

        // Act
        await service.DeleteAsync(printer.Id);

        // Assert
        var printers = await db.Printers.ToListAsync();
        printers.Should().BeEmpty();
    }
}
