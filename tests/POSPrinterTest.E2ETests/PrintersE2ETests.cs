using Microsoft.Playwright;

namespace POSPrinterTest.E2ETests;

/// <summary>
/// Playwright E2E tests for the Printer Management feature (Issue #1).
///
/// PREREQUISITES before running:
///   1. Build the E2E test project: dotnet build tests/POSPrinterTest.E2ETests
///   2. Install browser binaries: pwsh tests/POSPrinterTest.E2ETests/bin/Debug/net8.0/playwright.ps1 install
///   3. Set the BASE_URL environment variable to the running app URL (default: https://localhost:7000)
///   4. Start the web app: dotnet run --project src/POSPrinterTest.Web
/// </summary>
public class PrintersE2ETests : IAsyncLifetime
{
    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    private static string BaseUrl =>
        Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost:5050";

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
        _page = await _context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }

    [Fact]
    public async Task PrintersList_LoadsSuccessfully()
    {
        await _page.GotoAsync($"{BaseUrl}/Printers");

        await _page.WaitForSelectorAsync("h1");
        var heading = await _page.TextContentAsync("h1");

        Assert.Equal("Printers", heading?.Trim());
        Assert.True(await _page.IsVisibleAsync("text=Add Printer"));
    }

    [Fact]
    public async Task CreatePrinter_HappyPath_AddsAndShowsInList()
    {
        var uniqueName = $"E2E Printer {Guid.NewGuid():N}";

        // Navigate to Create
        await _page.GotoAsync($"{BaseUrl}/Printers/Create");
        await _page.WaitForSelectorAsync("h1");

        // Fill the form
        await _page.FillAsync("input[name='Input.Name']", uniqueName);
        await _page.SelectOptionAsync("select[name='Input.ConnectionType']", "Network");
        await _page.FillAsync("input[name='Input.ConnectionString']", "192.168.1.100");

        // Submit
        await _page.ClickAsync("button[type='submit']");

        // Should redirect to the list
        await _page.WaitForURLAsync($"{BaseUrl}/Printers");
        var bodyText = await _page.TextContentAsync("body");

        Assert.Contains(uniqueName, bodyText);
    }

    [Fact]
    public async Task EditPrinter_HappyPath_UpdatesAndShowsInList()
    {
        // First create a printer to edit
        var originalName = $"Edit Test {Guid.NewGuid():N}";
        var updatedName = $"Updated {Guid.NewGuid():N}";

        await _page.GotoAsync($"{BaseUrl}/Printers/Create");
        await _page.FillAsync("input[name='Input.Name']", originalName);
        await _page.SelectOptionAsync("select[name='Input.ConnectionType']", "USB");
        await _page.FillAsync("input[name='Input.ConnectionString']", "USB001");
        await _page.ClickAsync("button[type='submit']");
        await _page.WaitForURLAsync($"{BaseUrl}/Printers");

        // Find edit link for our printer
        var row = _page.Locator("tr", new() { HasText = originalName });
        await row.Locator("a:has-text('Edit')").ClickAsync();

        // Edit the name
        await _page.WaitForSelectorAsync("input[name='Input.Name']");
        await _page.FillAsync("input[name='Input.Name']", updatedName);
        await _page.ClickAsync("button[type='submit']");

        // Should redirect to the list and show updated name
        await _page.WaitForURLAsync($"{BaseUrl}/Printers");
        var bodyText = await _page.TextContentAsync("body");

        Assert.Contains(updatedName, bodyText);
    }

    [Fact]
    public async Task DeletePrinter_HappyPath_RemovesFromList()
    {
        // First create a printer to delete
        var printerName = $"Delete Test {Guid.NewGuid():N}";

        await _page.GotoAsync($"{BaseUrl}/Printers/Create");
        await _page.FillAsync("input[name='Input.Name']", printerName);
        await _page.SelectOptionAsync("select[name='Input.ConnectionType']", "Serial");
        await _page.FillAsync("input[name='Input.ConnectionString']", "COM3");
        await _page.ClickAsync("button[type='submit']");
        await _page.WaitForURLAsync($"{BaseUrl}/Printers");

        // Find delete link for our printer
        var row = _page.Locator("tr", new() { HasText = printerName });
        await row.Locator("a:has-text('Delete')").ClickAsync();

        // Confirm deletion
        await _page.WaitForSelectorAsync("button[type='submit']");
        await _page.ClickAsync("button[type='submit']");

        // Should redirect to the list and the printer should be gone
        await _page.WaitForURLAsync($"{BaseUrl}/Printers");
        var bodyText = await _page.TextContentAsync("body");

        Assert.DoesNotContain(printerName, bodyText);
    }
}
