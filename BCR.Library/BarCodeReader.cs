using BCR.Library.Models;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace BCR.Library;

public class BarCodeReader
{
    [SupportedOSPlatform("windows")]
    public static Barcode? ReadBarcodesOnOnePage(Bitmap oBitmap)
    {
        var _fileHandler = new FileRotator();

        Console.WriteLine("Detecting barcodes...");

        var reader = new BarcodeReader()
        {
            Options = new DecodingOptions
            {
                TryHarder = true,
                PureBarcode = false,
                PossibleFormats = [BarcodeFormat.CODE_128, BarcodeFormat.PDF_417]
            }
        };

        Bitmap croppedTopRight = null;
        Bitmap croppedBotRight = null;

        //                      out of memory
        //                      
        //                         w (size)
        // x,y(0,0)_______________________1,0
        // |                     |          |
        // |                   y |          | l (size)
        // |                     | rectangle|
        // |                     |          |
        // |                     |__________|
        // |         image             x    |
        // |                                |
        // |                                |
        // |0,1__________________________1,1|

        //oBitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);

        if (oBitmap.Width > oBitmap.Height)
        {
            croppedTopRight = oBitmap.Clone(new RectangleF(oBitmap.Width * .28f, oBitmap.Height * .02f, oBitmap.Width * .32f, oBitmap.Height * .15f), oBitmap.PixelFormat);
            croppedBotRight = oBitmap.Clone(new RectangleF(oBitmap.Width * .23f, oBitmap.Height * .76f, oBitmap.Width * .37f, oBitmap.Height * .2f), oBitmap.PixelFormat);
        }
        else
        {
            //  0 + firstWidth(x) + secondWidth =< 1        | starting x point |                          | distance from x |
            croppedTopRight = oBitmap.Clone(new RectangleF(oBitmap.Width * .55f, oBitmap.Height * .015f, oBitmap.Width * .449f, oBitmap.Height * .15f), oBitmap.PixelFormat);
            croppedBotRight = oBitmap.Clone(new RectangleF(oBitmap.Width * .43f, oBitmap.Height * .75f, oBitmap.Width * .57f, oBitmap.Height * .2f), oBitmap.PixelFormat);
        }
       
        croppedTopRight.Save("../../../../BCR.Library/Data/TestPng/croppedTopRight.jpeg", ImageFormat.Jpeg);
        croppedBotRight.Save("../../../../BCR.Library/Data/TestPng/croppedBotRight.jpeg", ImageFormat.Jpeg);

        Barcode barcode = new();

        Result fileName = reader.Decode(croppedTopRight);
        var folderPath = reader.Decode(croppedBotRight);
        if (fileName is not null)
        {
            fileName = ValidateFilename(fileName);
        }
        if (fileName == null)
        {
            fileName = ProcessBarcode(oBitmap, reader, croppedTopRight);
            if (fileName == null)
            {
                var croppedTopRightRotated = _fileHandler.RotateCroped(croppedTopRight);
                croppedTopRightRotated.Save("../../../../BCR.Library/Data/TestPng/croppedTopRightRotated.jpeg", ImageFormat.Jpeg);

                fileName = reader.Decode(croppedTopRightRotated);
                if (fileName is not null)
                {
                    fileName = ValidateFilename(fileName); 
                }
                fileName ??= ProcessBarcode(oBitmap, reader, croppedTopRightRotated);
            }
        }

        if (folderPath == null)
        {
            folderPath = ProcessBarcode(oBitmap, reader, croppedBotRight);
            if (folderPath == null)
            {
                var croppedBotRightRotated = _fileHandler.RotateCroped(croppedBotRight);
                croppedBotRightRotated.Save("../../../../BCR.Library/Data/TestPng/croppedBotRightRotated.jpeg", ImageFormat.Jpeg);

                folderPath = reader.Decode(croppedBotRightRotated);

                folderPath ??= ProcessBarcode(oBitmap, reader, croppedBotRightRotated);
            }
        }

        if (fileName is not null)
        {
            barcode.Filename = fileName.Text;
        }
        if (folderPath is not null)
        {
            barcode.Folderpath = folderPath.Text.Replace("/", "_");
        }

        if (folderPath is not null && fileName is not null)
        {
            Console.WriteLine("Got barcodes");
            return barcode;
        }
        //proved to be too labor intensive leaving here for debugging only
        else if (folderPath is not null && fileName is null && (oBitmap.Width > 6000 || oBitmap.Height > 6000))
        {
            Trace.TraceWarning($"Please enter the file name for the following path: {barcode.Folderpath}");
            barcode.Filename = Console.ReadLine();
            Trace.TraceWarning($"Your input: {barcode.Filename}");
            if (!string.IsNullOrEmpty(barcode.Filename))
            {
                return barcode;
            }
        }
        else if (folderPath is null && fileName is not null && (oBitmap.Width > 6000 || oBitmap.Height > 6000))
        {
            Trace.TraceWarning($"Please enter folder path for this: {barcode.Filename}");
            barcode.Folderpath = "\\\\ARCH-FRIGATE\\Scans\\Customers\\" + Console.ReadLine();
            Trace.TraceWarning($"Your input: {barcode.Folderpath}");
            if (!string.IsNullOrEmpty(barcode.Folderpath))
            {
                return barcode;
            }
        }
        if (fileName is null && folderPath is null)
        {
            Console.WriteLine("No valid barcodes detected");
            return null;
        }
        else
        {
            throw new ArgumentOutOfRangeException("Did not find exactly two valid barcodes on one page");
        }
    }
    public static Result ProcessBarcode(Bitmap oBitmap, BarcodeReader reader, Bitmap croppedRotated)
    {
        Result result = null;
        var blur = 1;
        var gamma = 1.5f;

        Bitmap adjustedBitmap = null;
        while (result is null && gamma < 2.1f)
        {
            adjustedBitmap = ImageEnchancement.AdjustBCG(croppedRotated, gamma);
            //debugging
            //adjustedBitmap.Save("../../../../BCR.Library/Data/TestPng/adjusted.jpeg", ImageFormat.Jpeg);

            result = reader.Decode(adjustedBitmap);
            if (result is not null)
            {
                if (result.Text.Length < 10)
                {
                    result = ValidateFilename(result);
                }
                if (result is not null)
                {
                    Console.WriteLine("Gamma adjustment helped");
                    return result;
                }
            }
            gamma += 0.1f;
        }
        while (result is null && ((blur < 4.8 && oBitmap.Height > 4000) || (blur < 2.8 && oBitmap.Height < 4000)))
        {
            var blurredBitmap = ImageEnchancement.FilterProcessImage(blur, adjustedBitmap);

            //var sfb = new GaussianBlur(adjustedBitmap);
            //var blurredBitmap = sfb.Process(blur);
            //debugging
            //blurredBitmap.Save("../../../../BCR.Library/Data/TestPng/blurred.jpeg", ImageFormat.Jpeg);

            result = reader.Decode(blurredBitmap);
            if (result is not null)
            {
                if (result.Text.Length < 10)
                {
                    result = ValidateFilename(result);
                }
                if (result is not null)
                {
                    Console.WriteLine("Thank you, Gauss!");
                    return result;
                }
            }
            blur += 1;
            if (oBitmap.Height > 4000)
            {
                //blur += 0.3;
            }
        }
        return null;
    }
    public static Result? ValidateFilename(Result result)
    {
        return result.Text.All(c => char.IsLetterOrDigit(c) || c.Equals('-'))  ? result : null;
    }
}