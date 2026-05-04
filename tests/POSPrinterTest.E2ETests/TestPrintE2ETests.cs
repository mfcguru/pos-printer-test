using Microsoft.Playwright;

namespace POSPrinterTest.E2ETests;

/// <summary>
/// Playwright E2E tests for the Test Print feature (Issue #2).
///
/// PREREQUISITES before running:
///   1. Build the E2E test project: dotnet build tests/POSPrinterTest.E2ETests
///   2. Install browser binaries: pwsh tests/POSPrinterTest.E2ETests/bin/Debug/net8.0/playwright.ps1 install
///   3. Set the BASE_URL environment variable to the running app URL (default: https://localhost:7000)
///   4. Start the web app: dotnet run --project src/POSPrinterTest.Web
/// </summary>
public class TestPrintE2ETests : IAsyncLifetime
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
    public async Task TestPrintPage_LoadsWithDropdownAndTextarea()
    {
        await _page.GotoAsync(BaseUrl);

        await _page.WaitForSelectorAsync("h1");
        var heading = await _page.TextContentAsync("h1");

        Assert.Equal("Test Print", heading?.Trim());
        Assert.True(await _page.IsVisibleAsync("select[name='SelectedPrinterId']"),
            "Printer dropdown should be visible");
        Assert.True(await _page.IsVisibleAsync("textarea[name='PrintContent']"),
            "Content textarea should be visible");
        Assert.True(await _page.IsVisibleAsync("button[type='submit']"),
            "Submit button should be visible");
    }

    [Fact]
    public async Task TestPrintPage_WithEpsonPrinter_ShowsSuccess()
    {
        await _page.GotoAsync(BaseUrl);
        await _page.WaitForSelectorAsync("select[name='SelectedPrinterId']");

        await _page.SelectOptionAsync("select[name='SelectedPrinterId']", new SelectOptionValue { Label = "Epson TM-T82" });
        await _page.FillAsync("textarea[name='PrintContent']", "Hello POS Printer!\nPlaywright E2E test print.");
        await _page.ClickAsync("button[type='submit']");

        // Wait for any result alert to appear
        await _page.WaitForSelectorAsync(".alert-success, .alert-danger");

        var successVisible = await _page.IsVisibleAsync(".alert-success");
        var alertText = await _page.TextContentAsync(".alert-success, .alert-danger");

        Assert.True(successVisible, $"Expected print success but got: {alertText?.Trim()}");
    }
}
