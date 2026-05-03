using Microsoft.EntityFrameworkCore;

namespace POSPrinterTest.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Printer> Printers => Set<Printer>();
}
