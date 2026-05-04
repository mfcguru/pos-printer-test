using System.Text;
using ZXing;
using ZXing.Common;

namespace POSPrinterTest.Web.Data;

public static class EscPosBarcodeHelper
{
    public static byte[] BuildCode39ImageBytes(string value, int widthPx = 576, int heightPx = 150, bool showHri = false)
    {
        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_39,
            Options = new EncodingOptions { Width = widthPx, Height = heightPx, Margin = 0, PureBarcode = true }
        };

        var pixelData = writer.Write(value.ToUpperInvariant());

        var result = new List<byte>(BuildGsV0Command(pixelData));

        if (showHri)
            result.AddRange(Encoding.ASCII.GetBytes(value.ToUpperInvariant()));

        result.Add(0x0A);
        return [.. result];
    }

    // GS v 0 — raster bit image command (ESC/POS)
    private static byte[] BuildGsV0Command(ZXing.Rendering.PixelData pixelData)
    {
        int width = pixelData.Width;
        int height = pixelData.Height;
        int bytesPerRow = (width + 7) / 8;
        var pixels = pixelData.Pixels; // RGBA32: R=+0, G=+1, B=+2, A=+3

        var imageBytes = new byte[bytesPerRow * height];
        for (int y = 0; y < height; y++)
        {
            for (int byteIdx = 0; byteIdx < bytesPerRow; byteIdx++)
            {
                byte packed = 0;
                for (int bit = 0; bit < 8; bit++)
                {
                    int x = byteIdx * 8 + bit;
                    if (x < width)
                    {
                        int offset = (y * width + x) * 4;
                        if (pixels[offset] + pixels[offset + 1] + pixels[offset + 2] < 384)
                            packed |= (byte)(0x80 >> bit);
                    }
                }
                imageBytes[y * bytesPerRow + byteIdx] = packed;
            }
        }

        List<byte> result =
        [
            0x1D, 0x76, 0x30, 0x00,
            (byte)(bytesPerRow & 0xFF),
            (byte)((bytesPerRow >> 8) & 0xFF),
            (byte)(height & 0xFF),
            (byte)((height >> 8) & 0xFF)
        ];
        result.AddRange(imageBytes);
        return [.. result];
    }
}
