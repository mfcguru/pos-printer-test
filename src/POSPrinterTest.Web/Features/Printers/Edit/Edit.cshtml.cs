using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using POSPrinterTest.Web.Data;

namespace POSPrinterTest.Web.Features.Printers.Edit;

public class EditModel : PageModel
{
    private readonly IPrinterService _printerService;

    public EditModel(IPrinterService printerService)
    {
        _printerService = printerService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public PrinterInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var printer = await _printerService.GetByIdAsync(Id);
        if (printer is null)
            return NotFound();

        Input = new PrinterInputModel
        {
            Name = printer.Name,
            ConnectionType = printer.ConnectionType,
            ConnectionString = printer.ConnectionString
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var printer = await _printerService.GetByIdAsync(Id);
        if (printer is null)
            return NotFound();

        await _printerService.UpdateAsync(Id, Input);
        return RedirectToPage("/Features/Printers/List/Index");
    }
}
