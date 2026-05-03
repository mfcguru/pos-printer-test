using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using POSPrinterTest.Web.Data;

namespace POSPrinterTest.Web.Features.TestPrint;

public class IndexModel : PageModel
{
    private readonly IPrinterService _printerService;
    private readonly IPrintService _printService;

    public IndexModel(IPrinterService printerService, IPrintService printService)
    {
        _printerService = printerService;
        _printService = printService;
    }

    public List<SelectListItem> PrinterOptions { get; set; } = [];

    [BindProperty]
    public int? SelectedPrinterId { get; set; }

    [BindProperty]
    public string PrintContent { get; set; } = string.Empty;

    public bool? PrintSuccess { get; set; }
    public string? ResultMessage { get; set; }

    public async Task OnGetAsync()
    {
        await PopulatePrinterOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await PopulatePrinterOptionsAsync();

        if (!SelectedPrinterId.HasValue)
        {
            ModelState.AddModelError(nameof(SelectedPrinterId), "Please select a printer.");
            return Page();
        }

        if (string.IsNullOrWhiteSpace(PrintContent))
        {
            ModelState.AddModelError(nameof(PrintContent), "Please enter content to print.");
            return Page();
        }

        var result = await _printService.PrintAsync(SelectedPrinterId.Value, PrintContent);
        PrintSuccess = result.Success;
        ResultMessage = result.Message;

        return Page();
    }

    private async Task PopulatePrinterOptionsAsync()
    {
        var printers = await _printerService.GetAllAsync();
        PrinterOptions = printers
            .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
            .ToList();
    }
}
