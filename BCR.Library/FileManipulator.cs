using BCR.Library.Models;
using Docnet.Core;
using Docnet.Core.Editors;
using Docnet.Core.Models;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace BCR.Library;
[SupportedOSPlatform("windows")]
public class FileManipulator
{
    public static void SplitPdf(string path, int splitPage)
    {
        using var docReader = DocLib.Instance.GetDocReader(path, new PageDimensions(1080, 1920));

        var pageCount = docReader.GetPageCount();

        int startPage = 0;
        int endPage = splitPage-1;
        PdfSplitter(path, startPage, endPage);
        startPage = splitPage;
        endPage = pageCount-1;
        PdfSplitter(path, startPage, endPage);
    }
    public static void PdfSplitter(string path, int startPage, int endPage)
    {
        byte[] bytes = DocLib.Instance.Split(path, startPage, endPage);
        string newPdfFileName = Path.GetFileNameWithoutExtension(path)+$"s{startPage}e{endPage}";
        string folder = Path.GetDirectoryName(path);
        string extension = Path.GetExtension(path);
        string newPdf = Path.Combine(folder, newPdfFileName+extension);
        File.WriteAllBytes(newPdf, bytes);
    }
    public static List<Barcode> CheckBitmapForBarcodes(List<Bitmap> bitmaps)
    {
        Barcode? foundBarcode = null;
        Barcode? decoded;
        List<Barcode> decodedList = new();
        int startPage = 0;
        for (int i = 0; i < bitmaps.Count; i++)
        {
            //var blurredBitmap = MakeGrayscale(bitmaps);//ImageEnchancement.FilterProcessImage(2, bitmaps[i]);
            //var blurredBitmap = ImageEnchancement.FilterProcessImage(1.6, bitmaps[i]);

            //Rectangle r = new(0, 0, bitmaps[i].Width, bitmaps[i].Height);

            //using (Graphics g = Graphics.FromImage(bitmaps[i]))
            //{
            //    using (Brush cloud_brush = new SolidBrush(Color.FromArgb(128, Color.Black)))
            //    {
            //        g.FillRectangle(cloud_brush, r);
            //    }
            //}

            //Bitmap originalImage;
            //Bitmap adjustedImage;


            

            //var adjustedBitmap = ImageEnchancement.AdjustBCG(bitmaps[i]);
            //var blurredBitmap = ImageEnchancement.FilterProcessImage(1.2, adjustedBitmap);
            //List<Bitmap> blurB = new()
            //{
            //    blurredBitmap
            //};
            //CreateRotatedPdf("\\\\ARCH-FRIGATE\\Scans\\BCR Test\\20230808094920277s1e1.pdf", blurB);
            //var blurredBitmap = ImageEnchancement.FilterProcessImage(1.2, bitmaps[i]);

            decoded = BarCodeReader.ReadBarcodes(bitmaps[i]);
            //decoded = BarCodeReader.ReadBarcodes(blurredBitmap);
            if (decoded is not null && foundBarcode is null)
            {
                foundBarcode = decoded;
                startPage = i;
                //One page PDF logic
                if (bitmaps.Count == 1)
                {
                    //BarCodeReader.HandleFiles(foundBarcode, path, pageCount - 1, i);
                    foundBarcode.StartPage = bitmaps.Count - 1;
                    foundBarcode.EndPage = i;
                    decodedList.Add(foundBarcode);
                }
                //Full_Packet can hold travelers. Let's keep them there
                if (foundBarcode.Folderpath.EndsWith("Full_Packet"))
                {
                    Trace.WriteLine("It's a full packet. Will not recognize more barcodes in this file");
                    foundBarcode.StartPage = i;
                    foundBarcode.EndPage = bitmaps.Count - 1;
                    decodedList.Add(foundBarcode);
                    break;
                }
            }
            else if (decoded is not null && foundBarcode is not null && decoded.Filename != foundBarcode.Filename)
            {
                //BarCodeReader.HandleFiles(foundBarcode, path, startPage, i - 1);

                foundBarcode.StartPage = startPage;
                foundBarcode.EndPage = i - 1;
                decodedList.Add(foundBarcode);
                foundBarcode = decoded;
                startPage = i;

                if (bitmaps.Count - 1 == i)
                {
                    //BarCodeReader.HandleFiles(foundBarcode, path, startPage, i);
                    foundBarcode.StartPage = startPage;
                    foundBarcode.EndPage = i;
                    decodedList.Add(foundBarcode);
                    startPage = i;
                }
            }
            else if (foundBarcode is not null && bitmaps.Count - 1 == i)
            {
                //BarCodeReader.HandleFiles(foundBarcode, path, startPage, i);
                foundBarcode.StartPage = startPage;
                foundBarcode.EndPage = i;
                decodedList.Add(foundBarcode);
                //startPage = i;
            }
            else if (decoded is null && foundBarcode is null)
            {
                Console.WriteLine("WARN: First page does not contain proper barcodes. Moving whole file to FailedScans folder");
                break;
            }
        }
        return decodedList;
    }
    public static string CreateRotatedPdf(string path, List<Bitmap> bmpList)
    {
        int counter = 001;
        string jpegPath = Path.ChangeExtension(path, ".jpeg");
        jpegPath = Path.Combine("../../../../BCR.Library/Data/TempPng", Path.GetFileNameWithoutExtension(jpegPath) + Path.GetExtension(jpegPath));
        string pngFileName = Path.GetFileNameWithoutExtension(jpegPath);
        string jpegFilePath;
        var _fileHandler = new FileRotator();
        int page = 1;
        foreach (var bmp in bmpList)
        {
            using var stream = new MemoryStream();
            Console.WriteLine($"Deciding if page {page} needs rotation...");
            page++;
            var rotatedImage = _fileHandler.Rotate(bmp);

            //bmp.Save(stream, ImageFormat.Png);
            rotatedImage.Save(stream, ImageFormat.Jpeg);

            do
            {
                string tempFileName = $"{pngFileName}({counter++:000})";
                //string tempFileName = string.Format("{0}", counter++.ToString("000"));
                jpegFilePath = Path.Combine("../../../../BCR.Library/Data/TempPng", tempFileName + Path.GetExtension(jpegPath));
            }
            while (File.Exists(jpegFilePath));
            File.WriteAllBytes(jpegFilePath, stream.ToArray());
            stream.Dispose();
            //bmp.Dispose();
        }
        return jpegPath;
    }
    public static void HandleFiles(List<Barcode> barcodes, string path)
    {
        string jpegPath = Path.GetFileNameWithoutExtension(path);
        //var folder = "..\\..\\..\\..\\BCR.Library\\Data\\TempPng";
        var folder = Path.GetDirectoryName(path);
        
        foreach (Barcode b in barcodes)
        {
            int counter = 1;
            List<string> listFiles = new();
            for (int i = b.StartPage + 1; i < b.EndPage + 2; i++)
            {
                listFiles.Add(Path.Combine(folder, jpegPath + $"({i:000}).jpeg"));
            }
            Console.WriteLine($"Pages {b.StartPage + 1} to {b.EndPage + 1} of scanned batch will be used to create pdf");
            Trace.WriteLine($"Pages {b.StartPage + 1} to {b.EndPage + 1} of scanned batch will be used to create pdf");
            Stopwatch stopwatch = Stopwatch.StartNew();
            //var bytes = DocLib.Instance.Split(path, startPage, endPage);

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
                File.Delete(file);
            }
            var pdfFiles = Directory.GetFiles(folder, "*.pdf");
            byte[] bytes = null;
            if (pdfFiles.Length > 1)
            {
                for (int i = 1; i < pdfFiles.Length; i++)
                {
                    string newPdf = Path.Combine(folder + "/" + Path.GetFileNameWithoutExtension(pdfFiles[0]) + "X" + ".pdf");
                    if (i == 1)
                    {
                        bytes = DocLib.Instance.Merge(pdfFiles[i - 1], pdfFiles[i]);
                        if (pdfFiles.Length>2)
                        {
                            File.WriteAllBytes(newPdf, bytes);
                        }
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
            string destination = b.Folderpath + "\\" + b.Filename + ".pdf";

            ////TESTING ONLY
            //destination = destination.Replace("\\\\ARCH-FRIGATE\\Scans\\", "..\\..\\..\\..\\BCR.Library\\Data\\");
            //b.Folderpath = b.Folderpath.Replace("\\\\ARCH-FRIGATE\\Scans\\", "..\\..\\..\\..\\BCR.Library\\Data\\");
            ////END OF TEST
            try
            {
                if (!Directory.Exists(b.Folderpath))
                {
                    Directory.CreateDirectory(b.Folderpath);
                }
                //File.Move(path, destination, true);
                string newFullPath = destination;
                while (File.Exists(newFullPath))
                {
                    string tempFileName = $"{b.Filename}({counter++})";
                    newFullPath = Path.Combine(b.Folderpath, tempFileName + Path.GetExtension(destination));
                }

                File.WriteAllBytes(newFullPath, bytes);
                Console.WriteLine("Copied file to: " + destination + " Named it " + Path.GetFileName(newFullPath));
                Trace.WriteLine("Copied file to: " + destination + " Named it " + Path.GetFileName(newFullPath));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Trace.WriteLine(ex.Message);
                throw;
            }
            stopwatch.Stop();
            Console.WriteLine($"Splitting and copying took {stopwatch.ElapsedMilliseconds} milliseconds");
        }
    }
    public static List<Bitmap> MakeGrayscale(List<Bitmap> originalList)
    {
        List<Bitmap> result = new();
        foreach (Bitmap original in originalList)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new(original.Width, original.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {

                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
             new float[] {.3f, .3f, .3f, 0, 0},
             new float[] {.59f, .59f, .59f, 0, 0},
             new float[] {.11f, .11f, .11f, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {0, 0, 0, 0, 1}
                   });

                //create some image attributes
                using ImageAttributes attributes = new ImageAttributes();

                //set the color matrix attribute
                attributes.SetColorMatrix(colorMatrix);

                //draw the original image on the new image
                //using the grayscale color matrix
                g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                            0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
            }
            result.Add(newBitmap);
        }
        return result;
    }
}
