using BCR.Library.Models;
using Docnet.Core;
using Docnet.Core.Editors;
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
    public static Barcode? ReadBarcodes(Bitmap oBitmap)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Detecting barcodes...");
        Barcode barcode = new();

        //var reader = new BarcodeReader(null, null, ls => new GlobalHistogramBinarizer(ls))
        //BarcodeReader reader = new(null, bitmap => new BitmapLuminanceSource(bitmap), luminance => new GlobalHistogramBinarizer(luminance))
        BarcodeReader reader = new(null, bitmap => new BitmapLuminanceSource(bitmap), luminance => new HybridBinarizer(luminance))
        //var reader = new BarcodeReader()
        {
            //AutoRotate = true,
            Options = new DecodingOptions
            {
                //TryInverted = true,
                TryHarder = true,
                PureBarcode = false,
                //ReturnCodabarStartEnd = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_128, BarcodeFormat.PDF_417 }
            }
        };

        //Bitmap oBitmap = new(path);
        //var adjustedBitmap = ImageEnchancement.AdjustBCG(oBitmap);


        //using var stream = new MemoryStream();

        //adjustedBitmap.Save(stream, ImageFormat.Jpeg);

        //File.WriteAllBytes("../../../../BCR.Library/Data/TempPng/adjusted.jpeg", stream.ToArray());
        //stream.Dispose();


        var result = reader.DecodeMultiple(oBitmap);
        Result[] previousSuccessfulResult = null;
        if (result != null)
        {
            var blur = 1.2;
            Trace.WriteLine($"Found {result.Length} barcode(s)");
            //if (result.Length == 1 && previousSuccessfulResult is not null)
            //    if (!result[0].ToString().Contains('\\'))
            //        if (Array.Exists(result, element => Array.Exists(previousSuccessfulResult, otherElement => otherElement == element)))
            //            result = previousSuccessfulResult;

            while(result is not null && result.Length == 1 && blur<1.5)
            {

                //using (Bitmap sourceImage = oBitmap)
                //{
                //    Rectangle cropRect = new(0, 0, -200, 200);
                //    Bitmap croppedImage = new Bitmap(cropRect.Width, cropRect.Height);

                //    using (Graphics g = Graphics.FromImage(croppedImage))
                //    {
                //        g.DrawImage(sourceImage, new Rectangle(0, 0, croppedImage.Width, croppedImage.Height), cropRect, GraphicsUnit.Pixel);
                //    }

                //    croppedImage.Save("../../../../BCR.Library/Data/TempPng/croppedImage.jpg");
                //}


                var adjustedBitmap = ImageEnchancement.AdjustBCG(oBitmap);
                //debugging
                adjustedBitmap.Save("../../../../BCR.Library/Data/TempPng/adjusted.jpeg", ImageFormat.Jpeg);

                var adjustedResult = reader.DecodeMultiple(adjustedBitmap);
                if (adjustedResult is not null && adjustedResult.Length == 2)
                {
                    Trace.WriteLine("Gamma adjustment helped");
                    result = adjustedResult;
                    break;
                }
                    //!!!!!!!!!!!!!!!!!!!! why original bitmap
                var blurredBitmap = ImageEnchancement.FilterProcessImage(blur, adjustedBitmap);
                //debugging
                blurredBitmap.Save("../../../../BCR.Library/Data/TempPng/blurred.jpeg", ImageFormat.Jpeg);

                var blurredResult = reader.DecodeMultiple(blurredBitmap);
                if (blurredResult is not null && blurredResult.Length == 2)
                {
                    Trace.WriteLine("Thank you, Gauss!");
                    result = blurredResult;
                    break;
                }

                blur += 0.1 ;
                if (adjustedResult is not null && blurredResult is not null && adjustedResult[0].BarcodeFormat != blurredResult[0].BarcodeFormat)
                    result = adjustedResult.Concat(blurredResult).ToArray();
                else if (adjustedResult is not null && adjustedResult[0].BarcodeFormat != result[0].BarcodeFormat)
                    result = adjustedResult.Concat(result).ToArray();
                else if (blurredResult is not null && result[0].BarcodeFormat != blurredResult[0].BarcodeFormat)
                    result = result.Concat(blurredResult).ToArray();
            }
            if (result is not null && result.Length == 2)
            {
                //string pngPath = PdfToImageExamples.CreateRotatedPdf(path, oBitmap);
                if (result[0].ToString().Contains('\\'))
                {
                    barcode.Folderpath = result[0].ToString();
                    barcode.Filename = result[1].ToString();
                }
                else
                {
                    barcode.Folderpath = result[1].ToString();
                    barcode.Filename = result[0].ToString();
                }
                previousSuccessfulResult = result;
            }
            else
            {
                //put in failed folder
                //oBitmap.Dispose();
                //File.Move(path, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(path), true);
                //Array.ForEach(Directory.GetFiles(Path.GetDirectoryName(path)), File.Delete);
                throw new ArgumentOutOfRangeException(nameof(result), "Did not find exactly two valid barcodes on one page");
            }

            //oBitmap.Dispose();
            //Console.WriteLine("Deleting png...");
            //File.Delete(path);
            stopwatch.Stop();
            Console.WriteLine($"Process took {stopwatch.ElapsedMilliseconds} milliseconds");
            return barcode;
        }
        else
        {
            Trace.WriteLine("No valid barcodes detected");
            //string pngPath = PdfToImageExamples.CreateRotatedPdf(path, oBitmap);
            //oBitmap.Dispose();
            //Console.WriteLine("Deleting png...");
            //File.Delete(path);
            stopwatch.Stop();

            Console.WriteLine($"Process took {stopwatch.ElapsedMilliseconds} milliseconds");
            return null;
        }
    }
    [SupportedOSPlatform("windows")]
    public static Barcode? ReadBarcodesOnOnePage(Bitmap oBitmap)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Detecting barcodes...");

        //var reader = new BarcodeReader(null, null, ls => new GlobalHistogramBinarizer(ls))
        //BarcodeReader reader = new(null, bitmap => new BitmapLuminanceSource(bitmap), luminance => new GlobalHistogramBinarizer(luminance))
        BarcodeReader reader = new(null, bitmap => new BitmapLuminanceSource(bitmap), luminance => new HybridBinarizer(luminance))
        //var reader = new BarcodeReader()
        {
            //AutoRotate = true,
            Options = new DecodingOptions
            {
                //TryInverted = true,
                TryHarder = true,
                PureBarcode = false,
                //ReturnCodabarStartEnd = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_128, BarcodeFormat.PDF_417 }
            }
        };
        Bitmap croppedTopRight = null;
        Bitmap croppedBotRight = null;
        if (oBitmap.Width > oBitmap.Height)
        {
            croppedTopRight = oBitmap.Clone(new RectangleF(oBitmap.Width * .28f, 0, oBitmap.Width * .32f, oBitmap.Height * .15f), oBitmap.PixelFormat);
            croppedBotRight = oBitmap.Clone(new RectangleF(oBitmap.Width * .23f, oBitmap.Height * .7f, oBitmap.Width * .37f, oBitmap.Height * .3f), oBitmap.PixelFormat);
        }
        else
        {
            croppedTopRight = oBitmap.Clone(new RectangleF(oBitmap.Width * .68f, 0, oBitmap.Width * .32f, oBitmap.Height * .15f), oBitmap.PixelFormat);
            croppedBotRight = oBitmap.Clone(new RectangleF(oBitmap.Width * .43f, oBitmap.Height * .7f, oBitmap.Width * .57f, oBitmap.Height * .3f), oBitmap.PixelFormat);
        }
        croppedTopRight.Save("../../../../BCR.Library/Data/TestPng/croppedTopRight.jpeg", ImageFormat.Jpeg);
        croppedBotRight.Save("../../../../BCR.Library/Data/TestPng/croppedBotRight.jpeg", ImageFormat.Jpeg);

        Barcode barcode = new();

        //var result = reader.DecodeMultiple(oBitmap);
        Result fileName = reader.Decode(croppedTopRight);
        var folderPath = reader.Decode(croppedBotRight);
        var blur = 1.2;
        while (fileName is null && blur <1.5)
        {
            var adjustedBitmap = ImageEnchancement.AdjustBCG(croppedTopRight);
            //debugging
            adjustedBitmap.Save("../../../../BCR.Library/Data/TestPng/adjustedTop.jpeg", ImageFormat.Jpeg);

            fileName = reader.Decode(adjustedBitmap);
            if (fileName is not null)
            {
                Trace.WriteLine("Gamma adjustment helped");
                barcode.Filename = fileName.ToString();
                break;
            }

            var blurredBitmap = ImageEnchancement.FilterProcessImage(blur, adjustedBitmap);
            //debugging
            blurredBitmap.Save("../../../../BCR.Library/Data/TestPng/blurredTop.jpeg", ImageFormat.Jpeg);

            fileName = reader.Decode(blurredBitmap);
            if (fileName is not null)
            {
                Trace.WriteLine("Thank you, Gauss!");
                barcode.Filename = fileName.ToString();
                break;
            }
            blur += 0.1;
        }
        blur = 1.2;
        while (folderPath is null && blur < 1.5)
        {
            var adjustedBitmap = ImageEnchancement.AdjustBCG(croppedBotRight);
            //debugging
            adjustedBitmap.Save("../../../../BCR.Library/Data/TestPng/adjustedBot.jpeg", ImageFormat.Jpeg);

            folderPath = reader.Decode(adjustedBitmap);
            if (folderPath is not null)
            {
                Trace.WriteLine("Gamma adjustment helped");
                barcode.Folderpath = folderPath.ToString();
                break;
            }

            var blurredBitmap = ImageEnchancement.FilterProcessImage(blur, adjustedBitmap);
            //debugging
            blurredBitmap.Save("../../../../BCR.Library/Data/TestPng/blurredBot.jpeg", ImageFormat.Jpeg);

            folderPath = reader.Decode(blurredBitmap);
            if (folderPath is not null)
            {
                Trace.WriteLine("Thank you, Gauss!");
                barcode.Folderpath = folderPath.ToString();
                break;
            }
            blur += 0.1;
        }
        if (fileName is not null)
        {
            barcode.Filename = fileName.ToString();
        }
        if (folderPath is not null)
        {
            barcode.Folderpath = folderPath.ToString();
        }
        
        stopwatch.Stop();
        Console.WriteLine($"Process took {stopwatch.ElapsedMilliseconds} milliseconds");

        if (folderPath is not null && fileName is not null)
        {
            return barcode;
        }
        if (fileName is null && folderPath is null)
        {
            Trace.WriteLine("No valid barcodes detected");
            //string pngPath = PdfToImageExamples.CreateRotatedPdf(path, oBitmap);
            //oBitmap.Dispose();
            //Console.WriteLine("Deleting png...");
            //File.Delete(path);
            return null;
        }
        else
        {
            //put in failed folder
            //oBitmap.Dispose();
            //File.Move(path, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(path), true);
            //Array.ForEach(Directory.GetFiles(Path.GetDirectoryName(path)), File.Delete);
            throw new ArgumentOutOfRangeException("Did not find exactly two valid barcodes on one page");
        }
    }
}

