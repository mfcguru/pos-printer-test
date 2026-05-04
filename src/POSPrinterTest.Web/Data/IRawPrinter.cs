namespace POSPrinterTest.Web.Data;

public interface IRawPrinter
{
    void Print(string printerName, byte[] bytes);
}
