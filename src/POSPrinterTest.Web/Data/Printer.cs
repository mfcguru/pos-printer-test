namespace POSPrinterTest.Web.Data;

public class Printer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ConnectionType { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}
