namespace POSPrinterTest.Web.Data;

public interface IPrinterService
{
    Task<List<Printer>> GetAllAsync();
    Task<Printer?> GetByIdAsync(int id);
    Task CreateAsync(PrinterInputModel input);
    Task UpdateAsync(int id, PrinterInputModel input);
    Task DeleteAsync(int id);
}
