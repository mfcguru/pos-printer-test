using Microsoft.EntityFrameworkCore;

namespace POSPrinterTest.Web.Data;

public class PrinterService : IPrinterService
{
    private readonly AppDbContext _db;

    public PrinterService(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Printer>> GetAllAsync() =>
        _db.Printers.OrderBy(p => p.Name).ToListAsync();

    public Task<Printer?> GetByIdAsync(int id) =>
        _db.Printers.FirstOrDefaultAsync(p => p.Id == id);

    public async Task CreateAsync(PrinterInputModel input)
    {
        var printer = new Printer
        {
            Name = input.Name,
            ConnectionType = input.ConnectionType,
            ConnectionString = input.ConnectionString
        };
        _db.Printers.Add(printer);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(int id, PrinterInputModel input)
    {
        var printer = await _db.Printers.FindAsync(id);
        if (printer is null) return;

        printer.Name = input.Name;
        printer.ConnectionType = input.ConnectionType;
        printer.ConnectionString = input.ConnectionString;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var printer = await _db.Printers.FindAsync(id);
        if (printer is null) return;

        _db.Printers.Remove(printer);
        await _db.SaveChangesAsync();
    }
}
