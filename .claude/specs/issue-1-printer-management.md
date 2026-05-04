# Issue #1: Printer Management Page

## Overview
A CRUD management page at `/Printers` where users can list, add, edit, and delete printers. Printers
are persisted in MSSQL via EF Core with Windows Authentication. This feature depends on Issue #3
(VSA folder structure) being in place first.

## Acceptance Criteria
- [ ] User can navigate to `/Printers` and see a table of all printers
- [ ] User can create a new printer (name, connection type, connection string/IP) and see it appear in the list
- [ ] User can edit an existing printer
- [ ] User can delete a printer (with a confirmation step)
- [ ] Data persists across app restarts (MSSQL, Windows Auth)
- [ ] `dotnet build` and `dotnet test` pass

## Data Model

### Entity: `Printer`
```csharp
public class Printer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty;  // e.g. "Network", "USB", "Serial"
    public string ConnectionString { get; set; } = string.Empty; // IP address or port/path
}
```

### DbContext: `AppDbContext`
Location: `src/POSPrinterTest.Web/Data/AppDbContext.cs`
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Printer> Printers => Set<Printer>();
}
```

### Connection String (appsettings.json)
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=POSPrinterTest;Integrated Security=True;Trusted_Connection=True;"
}
```
Use `(localdb)\mssqllocaldb` as the default local development server.

### Migration
Run after entity creation:
```
dotnet ef migrations add InitialCreate --project src/POSPrinterTest.Web --output-dir Data/Migrations
dotnet ef database update --project src/POSPrinterTest.Web
```

## Pages & UI (VSA)

All pages live under `src/POSPrinterTest.Web/Features/Printers/`.

| Slice | Route | File |
|---|---|---|
| List | GET `/Printers` | `Features/Printers/List/Index.cshtml` |
| Create | GET/POST `/Printers/Create` | `Features/Printers/Create/Create.cshtml` |
| Edit | GET/POST `/Printers/Edit/{id}` | `Features/Printers/Edit/Edit.cshtml` |
| Delete | GET/POST `/Printers/Delete/{id}` | `Features/Printers/Delete/Delete.cshtml` |

Navigation: add a "Printers" nav link in `Pages/Shared/_Layout.cshtml`.

### List Page
- Bootstrap table with columns: Name, Connection Type, Connection String, Actions (Edit | Delete links)
- "Add Printer" button linking to `/Printers/Create`
- Empty state message when no printers exist

### Create Page
- Form fields: Name (required), Connection Type (required, dropdown: Network / USB / Serial), Connection String (required)
- Submit button: "Create"
- Cancel link back to `/Printers`
- Server-side validation via data annotations; display `asp-validation-for` tags

### Edit Page
- Same form as Create, pre-populated with existing values
- Submit button: "Save"
- Cancel link back to `/Printers`

### Delete Page
- Displays the printer's Name for confirmation
- "Delete" confirmation button (POST)
- Cancel link back to `/Printers`

## Page Models

### `Features/Printers/List/IndexModel`
```csharp
public class IndexModel : PageModel
{
    private readonly IPrinterService _printerService;
    public List<Printer> Printers { get; set; } = [];

    public async Task OnGetAsync()
        => Printers = await _printerService.GetAllAsync();
}
```

### `Features/Printers/Create/CreateModel`
```csharp
[BindProperty] public PrinterInputModel Input { get; set; } = new();

public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid) return Page();
    await _printerService.CreateAsync(Input);
    return RedirectToPage("/Printers/List/Index");
}
```

### `Features/Printers/Edit/EditModel`
```csharp
[BindProperty] public PrinterInputModel Input { get; set; } = new();

public async Task<IActionResult> OnGetAsync(int id) { ... }
public async Task<IActionResult> OnPostAsync(int id) { ... }
```

### `Features/Printers/Delete/DeleteModel`
```csharp
public Printer? Printer { get; set; }

public async Task<IActionResult> OnGetAsync(int id) { ... }
public async Task<IActionResult> OnPostAsync(int id)
{
    await _printerService.DeleteAsync(id);
    return RedirectToPage("/Printers/List/Index");
}
```

### Shared Input Model
```csharp
public class PrinterInputModel
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string ConnectionType { get; set; } = string.Empty;
    [Required] public string ConnectionString { get; set; } = string.Empty;
}
```

## Services

### Interface: `IPrinterService`
Location: `src/POSPrinterTest.Web/Data/IPrinterService.cs`
```csharp
public interface IPrinterService
{
    Task<List<Printer>> GetAllAsync();
    Task<Printer?> GetByIdAsync(int id);
    Task CreateAsync(PrinterInputModel input);
    Task UpdateAsync(int id, PrinterInputModel input);
    Task DeleteAsync(int id);
}
```

### Implementation: `PrinterService`
Location: `src/POSPrinterTest.Web/Data/PrinterService.cs`
- Uses `AppDbContext` directly
- Registered as `Scoped` in `Program.cs`

## Program.cs Additions
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPrinterService, PrinterService>();
```
NuGet package required: `Microsoft.EntityFrameworkCore.SqlServer`
Also add: `Microsoft.EntityFrameworkCore.Tools` (for migrations)

## Tests

### Unit Tests (`tests/POSPrinterTest.Tests/`)
- `PrinterServiceTests`: GetAllAsync returns all printers, CreateAsync adds printer, UpdateAsync updates printer, DeleteAsync removes printer
- Use `Microsoft.EntityFrameworkCore.InMemory` for test DbContext

### E2E Playwright Tests
Happy path scenarios:
1. Navigate to `/Printers` — list renders (empty state)
2. Create a printer — appears in list
3. Edit the printer — changes are reflected
4. Delete the printer — removed from list

## Out of Scope
- Printer connectivity testing (that is Issue #2)
- Authentication/authorization
- Pagination
