namespace POSPrinterTest.Web.Data;

public interface IPrintService
{
    Task<PrintResult> PrintAsync(int printerId, string content);
}

public record PrintResult(bool Success, string Message);
