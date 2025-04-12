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

                if (bitmaps.Count>0)
                {
                    Console.WriteLine($"Processed {bitmaps.Count} page(s).");
                    var barcodes = FileManipulator.CheckBitmapForBarcodes(bitmaps);
                    if (barcodes.Count>0)
                    {
                        Console.WriteLine("Checking if pages need rotation...");

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
                        //File.Move(path, "../../../../BCR.Library/Data/OriginalScans/" + Path.GetFileName(path), true);
                        File.Move(path, "C:\\Users\\vladimir\\Downloads\\OriginalScans/" + Path.GetFileName(path), true);
                        break;
                    }
                    else
                    {
                        pageHeight += 500;
                        continue;
                    }
                }
                else
                {
                    File.Copy(path, "\\\\ARCH-FRIGATE\\Scans\\Failed Scans" + Path.GetFileName(path), true);
                    //File.Move(path, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(path), true);
                    File.Move(path, "C:\\Users\\vladimir\\Downloads\\FailedScans/" + Path.GetFileName(path), true);
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
            Trace.WriteLine("ERR: Quality of a scanned image is too poor.");

            File.Copy(path, "\\\\ARCH-FRIGATE\\Scans\\Failed Scans\\" + Path.GetFileName(path), true);
            //File.Move(path, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(path), true);
            File.Move(path, "C:\\Users\\vladimir\\Downloads\\FailedScans/" + Path.GetFileName(path), true);
            //File.Copy(splitted, "../../../../BCR.Library/Data/FailedScans/" + Path.GetFileName(splitted), true);
        }

        fixture.Dispose();
    }
}
