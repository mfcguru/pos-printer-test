using Microsoft.EntityFrameworkCore;

namespace POSPrinterTest.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<Printer> Printers => Set<Printer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Printer>().HasData(
            new Printer { Id = 1, Name = "Epson TM-T82", ConnectionType = "USB", ConnectionString = "EPSON TM-T82" }
        );
    }
}
