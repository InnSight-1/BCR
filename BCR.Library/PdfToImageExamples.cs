using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using BCR.Library.Models;
using Docnet.Core.Models;

namespace BCR.Library
{
    [SupportedOSPlatform("windows")]
    public class PdfToImageExamples
    {
       // private const string Path = "../../../../BCR.Library/Data/173328.pdf";

        private readonly ExampleFixture _fixture;
       // private readonly FileHandler _fileHandler;

        public PdfToImageExamples(ExampleFixture fixture) //,FileHandler fileHandler)
        {
            _fixture = fixture;
            //_fileHandler = fileHandler;
        }
        public List<Bitmap> GetBitmaps(string path, int pageWidth, int pageHeight)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool fileIsNotReady = true;
            List<Bitmap> bitmaps = new();

            Console.WriteLine("Attempting to read the file...");
            do
            {
                try
                {
                    using var open = File.OpenRead(path);
                    fileIsNotReady = false;
                }
                catch (IOException)
                {
                    Console.WriteLine("File is still being scanned. Pausing for 2 sec.");
                    Task.Delay(2000).Wait();
                    continue;
                }
            } while (fileIsNotReady);

            using var docReader = _fixture.DocNet.GetDocReader(
                path,
                //new PageDimensions(1080, 1920));
                //new PageDimensions(1900, 2900));
                //new PageDimensions(5000, 6000));
                new PageDimensions(pageWidth, pageHeight));

            var pageCount = docReader.GetPageCount();
            Trace.WriteLine($"{pageCount} page(s) detected. Getting bitmaps...");
            if (pageCount > 0)
            {
                for (int i = 0; i < pageCount; i++)
                {
                    //Console.WriteLine($"Reading page {i + 1}...");
                    using var pageReader = docReader.GetPageReader(i);

                    var rawBytes = pageReader.GetImage();

                    var width = pageReader.GetPageWidth();
                    var height = pageReader.GetPageHeight();
                    var characters = pageReader.GetCharacters();
                   // Console.WriteLine($"Getting bitmap of page {i + 1}...");
                    var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                    AddBytes(bmp, rawBytes);
                    DrawRectangles(bmp, characters);
                    //int x = (int)(height * 1.2);
                    //Bitmap newBitmap = new(x, height, bmp.PixelFormat);
                    //using (Graphics g = Graphics.FromImage(newBitmap))
                    //{
                    //    RectangleF dst = new(0, 0, x, height * 2);
                    //    RectangleF src = new(0, 0, bmp.Width, bmp.Height);
                    //    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    //    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    //    g.DrawImage(bmp, dst, src, GraphicsUnit.Pixel);
                    //}

                    //AddBytes(newBitmap, rawBytes);
                    //DrawRectangles(newBitmap, characters);
                    //bitmaps.Add(newBitmap);
                    //using var stream = new MemoryStream();
                    //bmp.Save(stream, ImageFormat.Jpeg);
                    //string jpegFilePath;
                    //int counter = 001;
                    //string jpegPath = Path.ChangeExtension(path, ".jpeg");
                    //jpegPath = Path.Combine("../../../../BCR.Library/Data/TempPng", Path.GetFileNameWithoutExtension(jpegPath) + Path.GetExtension(jpegPath));
                    //string pngFileName = Path.GetFileNameWithoutExtension(jpegPath);
                    //do
                    //{
                    //    string tempFileName = string.Format("{0}({1})", pngFileName, counter++.ToString("000"));
                    //    //string tempFileName = string.Format("{0}", counter++.ToString("000"));
                    //    jpegFilePath = Path.Combine("../../../../BCR.Library/Data/TempPng", tempFileName + Path.GetExtension(jpegPath));
                    //}
                    //while (File.Exists(jpegFilePath));
                    //File.WriteAllBytes(jpegFilePath, stream.ToArray());
                    //stream.Dispose();

                    bitmaps.Add(bmp);
                }
            }
            stopwatch.Stop();
            Console.WriteLine($"Whole process took {stopwatch.ElapsedMilliseconds} milliseconds");
            return bitmaps;
        }
        public bool ConvertPageToSimpleImageWithLetterOutlines_WhenCalled_ShouldSucceed(string path, int pageWidth, int pageHeight)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool fileIsNotReady = true;
            bool resultSuccess = false;
            Barcode? foundBarcode = null;
            Barcode? decoded;
            //List<Barcode> decodedList = new();
            int startPage = 0;
            Console.WriteLine("Attempting to read the file...");
            do
            {
                try
                {
                    using var open = File.OpenRead(path);
                    fileIsNotReady = false;
                }
                catch (IOException)
                {
                    Console.WriteLine("File is still being scanned. Pausing for 2 sec.");
                    Task.Delay(2000).Wait();
                    continue;
                }
            } while (fileIsNotReady);

            using var docReader = _fixture.DocNet.GetDocReader(
                path,
                //new PageDimensions(1080, 1920));
                //new PageDimensions(1900, 2900));
                //new PageDimensions(5000, 6000));
                new PageDimensions(pageWidth, pageHeight));

            var pageCount = docReader.GetPageCount();
            Console.WriteLine($"{pageCount} page(s) detected");
            if (pageCount > 0)
            {
                for (int i = 0; i < pageCount; i++)
                {
                    Console.WriteLine($"Reading page {i+1}...");
                    using var pageReader = docReader.GetPageReader(i);

                    var rawBytes = pageReader.GetImage();

                    var width = pageReader.GetPageWidth();
                    var height = pageReader.GetPageHeight();
                    var characters = pageReader.GetCharacters();
                    Console.WriteLine("Drawing png...");
                    using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                    AddBytes(bmp, rawBytes);
                    DrawRectangles(bmp, characters);
                    Console.WriteLine("Saving png to file...");

                    string pngPath = CreateRotatedPdf(path, bmp);

                    //using var stream = new MemoryStream();

                    //bmp.Save(stream, ImageFormat.Png);
                    //rotatedImage.Save(stream, ImageFormat.Png);

                    //string pngPath = Path.ChangeExtension(path, ".png");
                    //File.WriteAllBytes(pngPath, stream.ToArray());

                    //stream.Dispose();

                    try
                    {
                        decoded = BarCodeReader.ReadBarcodes(bmp, pngPath);
                        //decoded = BarCodeReader.ReadBarcodes(bmp, path);
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        throw ex;
                    }

                    if (decoded is not null && foundBarcode is null)
                    {
                        foundBarcode = decoded;
                        startPage = i;
                        if (pageCount == 1)
                        {
                            BarCodeReader.HandleFiles(foundBarcode, path, pageCount - 1, i);
                            //decodedList.Add(foundBarcode);
                            resultSuccess = true;
                        }
                    }
                    else if (decoded is not null && foundBarcode is not null && decoded.Filename != foundBarcode.Filename)
                    {
                        BarCodeReader.HandleFiles(foundBarcode, path, startPage, i - 1);
                        //decodedList.Add(foundBarcode);
                        foundBarcode = decoded;
                        startPage = i;
                        resultSuccess = true;

                        if (pageCount - 1 == i)
                        {
                            BarCodeReader.HandleFiles(foundBarcode, path, startPage, i);
                            //decodedList.Add(foundBarcode);
                        }
                    }
                    else if (foundBarcode is not null && pageCount - 1 == i)
                    {
                        BarCodeReader.HandleFiles(foundBarcode, path, startPage, i);
                        //decodedList.Add(foundBarcode);
                        resultSuccess = true;
                    }
                    else if (decoded is null && foundBarcode is null)
                    {
                        Console.WriteLine("WARN: First page does not contain proper barcodes. Moving whole file to FailedScans folder");
                        resultSuccess = false;
                        break;
                    }
                }
            }
            var folder = "..\\..\\..\\..\\BCR.Library\\Data\\TempPng";
            Array.ForEach(Directory.GetFiles(folder, "*.jpeg"), File.Delete);
            stopwatch.Stop();
            Console.WriteLine($"Whole process took {stopwatch.ElapsedMilliseconds} milliseconds");
            return resultSuccess;
            //return decodedList;
        }
        public static string CreateRotatedPdf(string path, Bitmap bmp)
        {
            int counter = 001;
            using var stream = new MemoryStream();

            var _fileHandler = new FileRotator();
            var rotatedImage = _fileHandler.Rotate(bmp);

            //bmp.Save(stream, ImageFormat.Png);
            rotatedImage.Save(stream, ImageFormat.Jpeg);

            string pngPath = Path.ChangeExtension(path, ".jpeg");
            pngPath = Path.Combine("../../../../BCR.Library/Data/TempPng", Path.GetFileNameWithoutExtension(pngPath) + Path.GetExtension(pngPath));
            string pngFileName = Path.GetFileNameWithoutExtension(pngPath);

            do
            {
                string tempFileName = string.Format("{0}({1})", pngFileName, counter++.ToString("000"));
                //string tempFileName = string.Format("{0}", counter++.ToString("000"));
                pngPath = Path.Combine("../../../../BCR.Library/Data/TempPng", tempFileName + Path.GetExtension(pngPath));
            }
            while (File.Exists(pngPath));
            File.WriteAllBytes(pngPath, stream.ToArray());
            stream.Dispose();
            return pngPath;
        }

        private static void AddBytes(Bitmap bmp, byte[] rawBytes)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

            var bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            var pNative = bmpData.Scan0;

            Marshal.Copy(rawBytes, 0, pNative, rawBytes.Length);
            bmp.UnlockBits(bmpData);
        }
        private static void DrawRectangles(Bitmap bmp, IEnumerable<Character> characters)
        {
            var pen = new Pen(Color.Red);

            using var graphics = Graphics.FromImage(bmp);

            foreach (var c in characters)
            {
                var rect = new Rectangle(c.Box.Left, c.Box.Top, c.Box.Right - c.Box.Left, c.Box.Bottom - c.Box.Top);
                graphics.DrawRectangle(pen, rect);
            }
        }
    }
}