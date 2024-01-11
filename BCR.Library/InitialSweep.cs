using System.Diagnostics;
using System.Runtime.Versioning;

namespace BCR.Library;
public class InitialSweep
{
    [SupportedOSPlatform("windows")]
    public static void CheckFolderForPDF(string folder)
    {
        Console.WriteLine("Checking if folder contains any PDF...");
        var files = Directory.GetFiles(folder, "*.pdf");
        if (files.Length != 0)
        {
            foreach (var file in files)
            {
                //Console.WriteLine("Found: "+file);
                Trace.WriteLine("\nFound: " + file);
                ProcessPDFBetter(file);
            }
        }
        else { Console.WriteLine("No PDF files found"); }
    }
    [SupportedOSPlatform("windows")]
    public static void ProcessPDFBetter(string path)
    {
        ExampleFixture fixture = new();
        PdfToImageExamples pdf = new(fixture);

        int pageHeight = 2220;
        for (int pageWidth = 1280; pageWidth < 5290; pageWidth += 500)
        {
            Console.WriteLine($"Page dimensions are set to {pageWidth} by {pageHeight}");
            try
            {
                var bitmaps = pdf.GetBitmaps(path, pageWidth, pageHeight);
                // bitmaps = FileManipulator.MakeGrayscale(bitmaps);
                //var bitmaps = pdf.GetBitmaps(path, pageWidth, pageHeight);
                if (bitmaps.Count>0)
                {
                    Console.WriteLine($"Processed {bitmaps.Count} page(s).");
                    var barcodes = FileManipulator.CheckBitmapForBarcodes(bitmaps);
                    if (barcodes.Count>0)
                    {
                        Console.WriteLine("Checking if pages need rotation...");
                        Stopwatch stopwatch = Stopwatch.StartNew();
                        var jpegPath = FileManipulator.CreateRotatedPdf(path, bitmaps);
                        stopwatch.Stop();
                        Console.WriteLine($"Process took {stopwatch.ElapsedMilliseconds} milliseconds");
                        try
                        {
                            FileManipulator.HandleFiles(barcodes, jpegPath);
                        }
                        catch (Exception ex)
                        {
                            pageHeight += 500;
                            continue;
                        }
                        //File.Delete(path);
                        File.Move(path, "../../../../BCR.Library/Data/OriginalScans/" + Path.GetFileName(path), true);
                        break;
                    }
                    else
                    {
                        pageHeight += 500;
                        continue;
                        //File.Move(path, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(path), true);
                        //break;
                    }
                    //File.Move(path, "../../../../BCR.Library/Data/OriginalScans/" + Path.GetFileName(path), true);
                }
                else
                {
                    //Console.WriteLine("No valid barcodes found. Moving file to FailedScans folder");
                    File.Move(path, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(path), true);
                    break;
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Console.WriteLine(ex.Message);
                pageHeight += 500;
            }
        }
        if (pageHeight > 6220)
        {
            //Console.WriteLine("ERR: Quality of a scanned image is too poor.");
            Trace.WriteLine("ERR: Quality of a scanned image is too poor.");
            //Trace.WriteLine($"Splitting file starting at first page to page number {endPage+1}");
            //string splitted = FileManipulator.SplitPdf(path, 0, endPage);
            //FileManipulator.SplitPdf(path, endPage, endPage);
            //FileManipulator.SplitPdf(path, endPage, lastPage);
            File.Move(path, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(path), true);
            //File.Copy(splitted, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(splitted), true);
        }

        fixture.Dispose();
    }
}
