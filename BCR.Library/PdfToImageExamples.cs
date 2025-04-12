using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Docnet.Core.Models;

namespace BCR.Library
{
    [SupportedOSPlatform("windows")]
    public class PdfToImageExamples(ExampleFixture fixture)
    {
        private readonly ExampleFixture _fixture = fixture;

        public List<Bitmap> GetBitmaps(string path, int pageWidth, int pageHeight)
        {
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

            using var docReader = _fixture.DocNet.GetDocReader(path, new PageDimensions(pageWidth, pageHeight));

            var pageCount = docReader.GetPageCount();
            Trace.WriteLine($"{pageCount} page(s) detected. Getting bitmaps...");
            if (pageCount > 0)
            {
                for (int i = 0; i < pageCount; i++)
                {
                    using var pageReader = docReader.GetPageReader(i);

                    var rawBytes = pageReader.GetImage();

                    var width = pageReader.GetPageWidth();
                    var height = pageReader.GetPageHeight();
                    var characters = pageReader.GetCharacters();

                    var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);

                    AddBytes(bmp, rawBytes);
                    DrawRectangles(bmp, characters);

                    bitmaps.Add(bmp);
                }
            }
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