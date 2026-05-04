using Microsoft.AspNetCore.Mvc.RazorPages;
using POSPrinterTest.Web.Data;

namespace POSPrinterTest.Web.Features.Printers.List;

public class IndexModel : PageModel
{
    private readonly IPrinterService _printerService;

    public IndexModel(IPrinterService printerService)
    {
        _printerService = printerService;
    }

    public List<Printer> Printers { get; set; } = [];

    public async Task OnGetAsync()
    {
        Printers = await _printerService.GetAllAsync();
    }
}
