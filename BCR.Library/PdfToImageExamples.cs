using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
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
            List<Bitmap> bitmaps = [];

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

                    bitmaps.Add(bmp);
                }
            }
            stopwatch.Stop();
            Console.WriteLine($"Whole process took {stopwatch.ElapsedMilliseconds} milliseconds");
            return bitmaps;
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