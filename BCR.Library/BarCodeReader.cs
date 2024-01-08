using BCR.Library.Models;
using Docnet.Core;
using Docnet.Core.Bindings;
using Docnet.Core.Editors;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;

namespace BCR.Library;

public class BarCodeReader
{
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
            int retry = 0;
            Trace.WriteLine($"Found {result.Length} barcode(s)");
            //if (result.Length == 1 && previousSuccessfulResult is not null)
            //    if (!result[0].ToString().Contains('\\'))
            //        if (Array.Exists(result, element => Array.Exists(previousSuccessfulResult, otherElement => otherElement == element)))
            //            result = previousSuccessfulResult;

            while(result is not null && result.Length == 1 && retry<1)
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

                //using var stream = new MemoryStream();

                //adjustedBitmap.Save(stream, ImageFormat.Jpeg);

                //File.WriteAllBytes("../../../../BCR.Library/Data/TempPng/adjusted.jpeg", stream.ToArray());
                //stream.Dispose();


                var adjustedResult = reader.DecodeMultiple(adjustedBitmap);
                if (adjustedResult is not null && adjustedResult.Length == 2)
                {
                    Trace.WriteLine("Gamma adjustment helped");
                    result = adjustedResult;
                    break;
                }
                    //!!!!!!!!!!!!!!!!!!!! why original bitmap
                var blurredBitmap = ImageEnchancement.FilterProcessImage(1.2, adjustedBitmap);

                //using var stream2 = new MemoryStream();

                //blurredBitmap.Save(stream2, ImageFormat.Jpeg);

                //File.WriteAllBytes("../../../../BCR.Library/Data/TempPng/blurred.jpeg", stream2.ToArray());
                //stream2.Dispose();

                var blurredResult = reader.DecodeMultiple(blurredBitmap);
                if (blurredResult is not null && blurredResult.Length == 2)
                {
                    Trace.WriteLine("Thank you, Gauss!");
                    result = blurredResult;
                    break;
                }

                retry++;
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
    public static Barcode? ReadBarcodes(Bitmap oBitmap, string path)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Console.WriteLine("Detecting barcodes...");
        Barcode barcode = new();

        BarcodeReader reader = new(null, bitmap => new BitmapLuminanceSource(bitmap), luminance => new GlobalHistogramBinarizer(luminance))

        //var reader = new BarcodeReader()
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryInverted = true,
                TryHarder = true,
                PureBarcode = false,
                ReturnCodabarStartEnd = true,
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.CODE_128, BarcodeFormat.PDF_417 }
            }
        };

        //Bitmap oBitmap = new(path);

        var result = reader.DecodeMultiple(oBitmap);
        if (result != null)
        {
            Console.WriteLine($"Found {result.Length} barcode(s)");
            if (result.Length == 2)
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
            }
            else
            {
                //put in failed folder
                oBitmap.Dispose();
                File.Move(path, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(path), true);
                Array.ForEach(Directory.GetFiles(Path.GetDirectoryName(path)), File.Delete);
                throw new ArgumentOutOfRangeException(nameof(result), result.Length, "Did not find exactly two valid barcodes on one page");
            }
            
            oBitmap.Dispose();
            //Console.WriteLine("Deleting png...");
            //File.Delete(path);
            stopwatch.Stop();
            Console.WriteLine($"Process took {stopwatch.ElapsedMilliseconds} milliseconds");
            return barcode;
        }
        else
        {
            Console.WriteLine("No valid barcodes detected");
            //string pngPath = PdfToImageExamples.CreateRotatedPdf(path, oBitmap);
            oBitmap.Dispose();
            //Console.WriteLine("Deleting png...");
            //File.Delete(path);
            stopwatch.Stop();

            Console.WriteLine($"Process took {stopwatch.ElapsedMilliseconds} milliseconds");
            return null;
        }
    }
    public static void HandleFiles(Barcode barcode, string path, int startPage, int endPage)
    {
        int counter = 1;

        Console.WriteLine($"Pages {startPage+1} to {endPage+1} of scanned batch will be used to create pdf");
        Stopwatch stopwatch = Stopwatch.StartNew();
        //var bytes = DocLib.Instance.Split(path, startPage, endPage);
        string jpegPath = Path.GetFileNameWithoutExtension(path);
        var folder = "..\\..\\..\\..\\BCR.Library\\Data\\TempPng";
        List<string> listFiles = new();
        for (int i = startPage+1; i < endPage+2; i++)
        {
            //listFiles.Add(Directory.GetFiles(folder, $"*{i.ToString("000")}.jpeg").ToString());
            //listFiles.Add(Path.Combine(folder, $"{i:000}.jpeg"));
            listFiles.Add(Path.Combine(folder, jpegPath + $"({i:000}).jpeg"));
        }
        //var files = listFiles.ToArray();

        foreach (var file in listFiles)
        {
            Bitmap img = new(file);
            var jpImage = new JpegImage
            {
                Bytes = File.ReadAllBytes(file),
                Width = img.Width,
                Height = img.Height
            };

            var firstImage = DocLib.Instance.JpegToPdf(new[] { jpImage });

            string p = Path.Combine(folder + "/" + Path.GetFileNameWithoutExtension(file) + ".pdf");
            File.WriteAllBytes(p, firstImage);
            img.Dispose();
            //File.Delete(file);

        }
        var pdfFiles = Directory.GetFiles(folder, "*.pdf");
        byte[] bytes = null;
        if (pdfFiles.Length >1)
        {
            for (int i = 1; i < pdfFiles.Length; i++)
            {
                string newPdf = Path.Combine(folder + "/" + Path.GetFileNameWithoutExtension(pdfFiles[0]) + "X" + ".pdf");
                if (i == 1)
                {
                    bytes = DocLib.Instance.Merge(pdfFiles[i - 1], pdfFiles[i]);

                    File.WriteAllBytes(newPdf, bytes);
                }
                else if (pdfFiles.Length - i == 1)
                {
                    bytes = DocLib.Instance.Merge(newPdf, pdfFiles[i]);
                    break;
                }
                else
                {
                    bytes = DocLib.Instance.Merge(newPdf, pdfFiles[i]);
                    File.WriteAllBytes(newPdf, bytes);
                }
            }
        }
        else
        {
            bytes = DocLib.Instance.Split(pdfFiles[0], 0, 0);
        }
        
        Array.ForEach(Directory.GetFiles(folder, "*.pdf"), File.Delete);
        string destination = barcode.Folderpath + "\\" + barcode.Filename + ".pdf";

        //TESTING ONLY
        destination = destination.Replace("\\\\ARCH-FRIGATE\\Scans\\", "..\\..\\..\\..\\BCR.Library\\Data\\");
        barcode.Folderpath = barcode.Folderpath.Replace("\\\\ARCH-FRIGATE\\Scans\\", "..\\..\\..\\..\\BCR.Library\\Data\\");
        //END OF TEST
        try
        {
            if (!Directory.Exists(barcode.Folderpath))
            {
                Directory.CreateDirectory(barcode.Folderpath);
            }
            //File.Move(path, destination, true);
            string newFullPath = destination;
            while(File.Exists(newFullPath))
            {
                string tempFileName = string.Format("{0}({1})", barcode.Filename, counter++);
                newFullPath = Path.Combine(barcode.Folderpath, tempFileName + Path.GetExtension(destination));
            }

            File.WriteAllBytes(newFullPath, bytes);
            Console.WriteLine("Copied file to: " + destination + " Named it " + Path.GetFileName(newFullPath));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        stopwatch.Stop();
        Console.WriteLine($"Splitting and copying took {stopwatch.ElapsedMilliseconds} milliseconds");
    }
}

