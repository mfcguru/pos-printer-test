using Neodynamic.SDK.Printing;

namespace POSPrinterTest.Web.Data;

public class WindowsRawPrinter : IRawPrinter
{
    public void Print(string printerName, byte[] bytes)
    {
        PrintUtils.PrinterSettings = new PrinterSettings { PrinterName = printerName };
        PrintUtils.ExecuteCommand(bytes);
    }
}
