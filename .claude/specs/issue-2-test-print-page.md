# Issue #2: Test Print Page

## Overview
The home page (`/`) is the main operational screen. Users select a printer from a dropdown (loaded
from the database), type or paste content into a text area, and click "Test Print" to send the job.
The page shows a success or error message after the attempt. This feature depends on Issue #1
(Printer Management) being in place first so printers exist in the DB.

## Acceptance Criteria
- [ ] Page loads at `/` with a dropdown populated from the Printers table
- [ ] User can type or paste content into a text area
- [ ] Clicking "Test Print" sends the content to the selected printer
- [ ] A success message is displayed on success
- [ ] An error message is displayed if the print fails (e.g. printer unreachable)
- [ ] `dotnet build` and `dotnet test` pass

## Data Model
No new entities. Reads existing `Printer` records via `IPrinterService`.

## Pages & UI (VSA)

Slice location: `src/POSPrinterTest.Web/Features/TestPrint/`

| Slice | Route | File |
|---|---|---|
| Test Print | GET/POST `/` | `Features/TestPrint/Index.cshtml` |

This replaces the existing `Pages/Index.cshtml` (which becomes the default route after VSA migration
sets `RootDirectory = "/"`). The existing `Pages/Index.cshtml` scaffold file is deleted.

### Page Layout
- Heading: "Test Print"
- **Printer** dropdown (`<select>`): lists all printers as `Name` (value = `Id`). Shows
  "-- Select a printer --" as the default disabled option.
- **Content** text area: multi-line, placeholder "Paste or type content to print…", required
- **Test Print** button (primary, POST)
- Alert div: shown only after a POST — green (success) or red (danger) Bootstrap alert with the
  result message. Hidden on GET.

## Page Model

### `Features/TestPrint/IndexModel`
```csharp
public class IndexModel : PageModel
{
    private readonly IPrinterService _printerService;
    private readonly IPrintService _printService;

    public List<SelectListItem> PrinterOptions { get; set; } = [];

    [BindProperty]
    [Required]
    public int? SelectedPrinterId { get; set; }

    [BindProperty]
    [Required]
    public string Content { get; set; } = string.Empty;

    public bool? PrintSuccess { get; set; }    // null = no attempt yet
    public string? ResultMessage { get; set; }

    public async Task OnGetAsync()
        => PrinterOptions = await BuildPrinterOptionsAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        PrinterOptions = await BuildPrinterOptionsAsync();
        if (!ModelState.IsValid) return Page();

        var result = await _printService.PrintAsync(SelectedPrinterId!.Value, Content);
        PrintSuccess = result.Success;
        ResultMessage = result.Message;
        return Page();
    }

    private async Task<List<SelectListItem>> BuildPrinterOptionsAsync() { ... }
}
```

## Services

### Interface: `IPrintService`
Location: `src/POSPrinterTest.Web/Data/IPrintService.cs`
```csharp
public interface IPrintService
{
    Task<PrintResult> PrintAsync(int printerId, string content);
}

public record PrintResult(bool Success, string Message);
```

### Implementation: `PrintService`
Location: `src/POSPrinterTest.Web/Data/PrintService.cs`

The initial implementation is a **stub** that always returns success, because actual POS printer
communication is out of scope. Add a `// TODO: implement real printer communication` comment.

```csharp
public class PrintService : IPrintService
{
    private readonly IPrinterService _printerService;

    public async Task<PrintResult> PrintAsync(int printerId, string content)
    {
        var printer = await _printerService.GetByIdAsync(printerId);
        if (printer is null)
            return new PrintResult(false, $"Printer with ID {printerId} not found.");

        // TODO: implement real printer communication
        return new PrintResult(true, $"Test print sent to '{printer.Name}' successfully.");
    }
}
```

Registration in `Program.cs`:
```csharp
builder.Services.AddScoped<IPrintService, PrintService>();
```

## Tests

### Unit Tests (`tests/POSPrinterTest.Tests/`)
- `PrintServiceTests`:
  - `PrintAsync_UnknownPrinter_ReturnsFailure`
  - `PrintAsync_KnownPrinter_ReturnsSuccess`
  - Uses mocked `IPrinterService`

### E2E Playwright Tests
Happy path:
1. Navigate to `/` — page loads, dropdown is populated
2. Select first printer, enter text, click "Test Print"
3. Success alert is displayed

Error path:
1. Submit without selecting a printer — validation error shown

## Out of Scope
- Real POS printer communication protocols (ESC/POS, etc.)
- Print history / job queue
- Authentication/authorization
