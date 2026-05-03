namespace POSPrinterTest.Web.Data;

public class PrintService : IPrintService
{
    private readonly IPrinterService _printerService;

    public PrintService(IPrinterService printerService) => _printerService = printerService;

    public async Task<PrintResult> PrintAsync(int printerId, string content)
    {
        var printer = await _printerService.GetByIdAsync(printerId);
        if (printer is null)
            return new PrintResult(false, $"Printer with ID {printerId} not found.");

        // TODO: implement real printer communication
        return new PrintResult(true, $"Test print sent to '{printer.Name}' successfully.");
    }
}
