using System.ComponentModel.DataAnnotations;

namespace POSPrinterTest.Web.Data;

public class PrinterInputModel
{
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string ConnectionType { get; set; } = string.Empty;
    [Required] public string ConnectionString { get; set; } = string.Empty;
}
