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
                Console.WriteLine("Found: "+file);
                Trace.WriteLine("\nFound: " + file);
                //ProcessPDF(file);
                //ProcessTiff(file);
                ProcessPDFBetter(file);
            }
        }
        else { Console.WriteLine("No PDF files found"); }
    }
    [SupportedOSPlatform("windows")]
    public static void CheckFailedFolderForPDF(string folder)
    {
        
        Console.WriteLine("Checking if folder contains any PDF...");
        var files = Directory.GetFiles(folder, "*.pdf");
        if (files.Length != 0)
        {
            foreach (var file in files)
            {
                ExampleFixture fixture = new();
                Console.WriteLine("Found: " + file);
                var firstPage = fixture.DocNet.Split(file, 0, 0);
                int pageCount = fixture.DocNet.GetDocReader(file, new Docnet.Core.Models.PageDimensions(1080, 1920)).GetPageCount();
                var remainingPages = fixture.DocNet.Split(file, 1, pageCount-1);
                var newPDF = fixture.DocNet.Merge(remainingPages, firstPage);
                string newFileName = string.Format("{0}Reversed", Path.GetFileNameWithoutExtension(file));
                var newFile = Path.Combine(folder, newFileName+ ".pdf");
                File.WriteAllBytes(newFile, newPDF);
                fixture.Dispose();
                ProcessPDF(newFile);
            }
        }
        else { Console.WriteLine("No PDF files found"); }
    }
    [SupportedOSPlatform("windows")]
    public static void ProcessPDF(string path)
    {
        ExampleFixture fixture = new();
        PdfToImageExamples pdf = new(fixture);

        int pageHeight = 2220;
        for (int pageWidth = 1280; pageWidth<5290; pageWidth += 1000)
        {
            Console.WriteLine($"Page dimensions are set to {pageWidth} by {pageHeight}");
            try
            {
                if (pdf.ConvertPageToSimpleImageWithLetterOutlines_WhenCalled_ShouldSucceed(path, pageWidth, pageHeight))
                {
                    Console.WriteLine($"Processed successfully! Removing original scan at {path}");

                    //File.Move(path, "../../../../BCR.Library/Data/OriginalScans/" + Path.GetFileName(path), true);
                    File.Delete(path);
                    break;
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
                pageHeight += 1000;
            }
        }
        if (pageHeight> 6000)
        {
            Console.WriteLine("ERR: Quality of a scanned image is too poor.");
            File.Move(path, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(path), true);
        }
        
        fixture.Dispose();
    }
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
                        var jpegPath = FileManipulator.CreateRotatedPdf(path, bitmaps);
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
    public static void ProcessFailedFiles(string path)
    {

    }
}
