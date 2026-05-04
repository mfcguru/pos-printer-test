using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using POSPrinterTest.Web.Data;

namespace POSPrinterTest.Web.Features.Printers.Delete;

public class DeleteModel : PageModel
{
    private readonly IPrinterService _printerService;

    public DeleteModel(IPrinterService printerService)
    {
        _printerService = printerService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public string PrinterName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var printer = await _printerService.GetByIdAsync(Id);
        if (printer is null)
            return NotFound();

        PrinterName = printer.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var printer = await _printerService.GetByIdAsync(Id);
        if (printer is null)
            return NotFound();

        await _printerService.DeleteAsync(Id);
        return RedirectToPage("/Features/Printers/List/Index");
    }
}
