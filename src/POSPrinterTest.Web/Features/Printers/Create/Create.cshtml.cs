using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using POSPrinterTest.Web.Data;

namespace POSPrinterTest.Web.Features.Printers.Create;

public class CreateModel : PageModel
{
    private readonly IPrinterService _printerService;

    public CreateModel(IPrinterService printerService)
    {
        _printerService = printerService;
    }

    [BindProperty]
    public PrinterInputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _printerService.CreateAsync(Input);
        return RedirectToPage("/Features/Printers/List/Index");
    }
}
