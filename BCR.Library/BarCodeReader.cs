﻿using BCR.Library.Models;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Intrinsics.X86;
using System.Runtime.Versioning;
using ZXing;
using ZXing.Common;
using ZXing.Multi;
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


                var adjustedBitmap = ImageEnchancement.AdjustBCG(oBitmap, 2.0f);
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
        var _fileHandler = new FileRotator();

        Console.WriteLine("Detecting barcodes...");
        //Stopwatch stopwatch = Stopwatch.StartNew();

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
                PureBarcode = true,
                //ReturnCodabarStartEnd = true,
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
            croppedTopRight = oBitmap.Clone(new RectangleF(oBitmap.Width * .65f, oBitmap.Height * .015f, oBitmap.Width * .35f, oBitmap.Height * .15f), oBitmap.PixelFormat);
            croppedBotRight = oBitmap.Clone(new RectangleF(oBitmap.Width * .43f, oBitmap.Height * .75f, oBitmap.Width * .57f, oBitmap.Height * .2f), oBitmap.PixelFormat);
        }
       
        //var croppedBotRightRotated = _fileHandler.RotateCroped(croppedBotRight);
        //var croppedTopRightRotated = croppedTopRight;//_fileHandler.Rotate(croppedTopRight);
        //var croppedBotRightRotated = croppedBotRight;//_fileHandler.Rotate(croppedBotRight);

        //croppedBotRightRotated.Save("../../../../BCR.Library/Data/TestPng/croppedBotRightRotated.jpeg", ImageFormat.Jpeg);
        croppedTopRight.Save("../../../../BCR.Library/Data/TestPng/croppedTopRight.jpeg", ImageFormat.Jpeg);
        croppedBotRight.Save("../../../../BCR.Library/Data/TestPng/croppedBotRight.jpeg", ImageFormat.Jpeg);

        Barcode barcode = new();

        //var result = reader.DecodeMultiple(oBitmap);
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
            barcode.Folderpath = folderPath.Text;
        }
        
        //stopwatch.Stop();
        //Console.WriteLine($"Process took {stopwatch.ElapsedMilliseconds} milliseconds");

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
            barcode.Folderpath = Console.ReadLine();
            Trace.TraceWarning($"Your input: {barcode.Folderpath}");
            if (!string.IsNullOrEmpty(barcode.Folderpath))
            {
                return barcode;
            }
        }
        if (fileName is null && folderPath is null)
        //else
        {
            Console.WriteLine("No valid barcodes detected");
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