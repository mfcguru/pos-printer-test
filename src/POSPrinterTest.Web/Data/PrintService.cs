using System.Text.RegularExpressions;

namespace POSPrinterTest.Web.Data;

public class PrintService : IPrintService
{
    private readonly IPrinterService _printerService;
    private readonly IRawPrinter _rawPrinter;

    public PrintService(IPrinterService printerService, IRawPrinter rawPrinter)
    {
        _printerService = printerService;
        _rawPrinter = rawPrinter;
    }

    public async Task<PrintResult> PrintAsync(int printerId, string content)
    {
        var printer = await _printerService.GetByIdAsync(printerId);
        if (printer is null)
            return new PrintResult(false, $"Printer with ID {printerId} not found.");

        try
        {
            byte[] rawBytes = BuildPrintBytes(content);

            if (rawBytes.Length == 0)
                throw new InvalidOperationException("Nothing to print — content is empty.");

            _rawPrinter.Print(printer.ConnectionString, rawBytes);

            return new PrintResult(true, $"Sent to '{printer.Name}' successfully.");
        }
        catch (Exception ex)
        {
            return new PrintResult(false, $"Print failed: {ex.Message}");
        }
    }

    // Decodes \xNN escape sequences to bytes. Barcode(...) tags are rendered
    // as GS v 0 raster images and inserted inline. All other framing (init, cut,
    // alignment, etc.) is the caller's responsibility via their ESC/POS sequences.
    public static byte[] BuildPrintBytes(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return [];

        var bytes = new List<byte>();
        var parts = Regex.Split(content, @"(Barcode\([^)]*\))", RegexOptions.IgnoreCase);

        foreach (var part in parts)
        {
            if (Regex.IsMatch(part, @"^Barcode\(", RegexOptions.IgnoreCase))
                bytes.AddRange(ParseBarcodeTag(part));
            else if (!string.IsNullOrEmpty(part))
                bytes.AddRange(ParseEscapeSequences(part));
        }

        return [.. bytes];
    }

    private static byte[] ParseBarcodeTag(string tag)
    {
        var m = Regex.Match(tag, @"^Barcode\((\d+),(\d+),([^,)]+)(?:,(0|1))?\)$", RegexOptions.IgnoreCase);
        if (!m.Success)
            throw new FormatException($"Invalid barcode tag '{tag}'. Expected: Barcode(width,height,value) or Barcode(width,height,value,0|1)");

        int width    = int.Parse(m.Groups[1].Value);
        int height   = int.Parse(m.Groups[2].Value);
        string value = m.Groups[3].Value.Trim();
        bool showHri = m.Groups[4].Value == "1";
        return EscPosBarcodeHelper.BuildCode39ImageBytes(value, width, height, showHri);
    }

    private static byte[] ParseEscapeSequences(string content)
    {
        var bytes = new List<byte>();
        int i = 0;
        while (i < content.Length)
        {
            if (i + 3 < content.Length
                && content[i] == '\\'
                && content[i + 1] == 'x'
                && IsHex(content[i + 2])
                && IsHex(content[i + 3]))
            {
                bytes.Add(Convert.ToByte(content.Substring(i + 2, 2), 16));
                i += 4;
            }
            else if (content[i] == '\n')
            {
                bytes.Add(0x0A);
                i++;
            }
            else if (content[i] == '\r')
            {
                i++;
            }
            else
            {
                bytes.Add((byte)content[i]);
                i++;
            }
        }
        return [.. bytes];
    }

    private static bool IsHex(char c) =>
        (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
}
