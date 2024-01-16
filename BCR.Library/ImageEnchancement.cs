using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace BCR.Library;
public class ImageEnchancement
{
    [SupportedOSPlatform("windows")]
    public static Bitmap AdjustBCG(Bitmap bmp, float gamma)
    {
        float brightness = .96f; 
        float contrast = 1.1f; 
        //float gamma = 3.9f; 
        //float brightness = 1.0f; // no change in brightness
        //float contrast = 2.0f; // twice the contrast
        //float gamma = 1.0f; // no change in gamma

        float adjustedBrightness = brightness - 1.0f;
        // create matrix that will brighten and contrast the image
        float[][] ptsArray =[
        [contrast, 0, 0, 0, 0], // scale red
        [0, contrast, 0, 0, 0], // scale green
        [0, 0, contrast, 0, 0], // scale blue
        [0, 0, 0, 1.0f, 0], // don't scale alpha
        [adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1]];

        ImageAttributes imageAttributes = new();
        imageAttributes.ClearColorMatrix();
        imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
        Graphics g = Graphics.FromImage(bmp);
        g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height)
            , 0, 0, bmp.Width, bmp.Height,
            GraphicsUnit.Pixel, imageAttributes);
        return bmp;
    }
    public static double[,] Calculate1DSampleKernel(double deviation, int size)
    {
        double[,] ret = new double[size, 1];
        double sum = 0;
        int half = size / 2;
        for (int i = 0; i < size; i++)
        {
            ret[i, 0] = 1 / (Math.Sqrt(2 * Math.PI) * deviation) * Math.Exp(-(i - half) * (i - half) / (2 * deviation * deviation));
            sum += ret[i, 0];
        }
        return ret;
    }
    public static double[,] Calculate1DSampleKernel(double deviation)
    {
        int size = (int)Math.Ceiling(deviation * 3) * 2 + 1;
        return Calculate1DSampleKernel(deviation, size);
    }
    public static double[,] CalculateNormalized1DSampleKernel(double deviation)
    {
        return NormalizeMatrix(Calculate1DSampleKernel(deviation));
    }
    public static double[,] NormalizeMatrix(double[,] matrix)
    {
        double[,] ret = new double[matrix.GetLength(0), matrix.GetLength(1)];
        double sum = 0;
        for (int i = 0; i < ret.GetLength(0); i++)
        {
            for (int j = 0; j < ret.GetLength(1); j++)
                sum += matrix[i, j];
        }
        if (sum != 0)
        {
            for (int i = 0; i < ret.GetLength(0); i++)
            {
                for (int j = 0; j < ret.GetLength(1); j++)
                    ret[i, j] = matrix[i, j] / sum;
            }
        }
        return ret;
    }
    public static double[,] GaussianConvolution(double[,] matrix, double deviation)
    {
        double[,] kernel = CalculateNormalized1DSampleKernel(deviation);
        double[,] res1 = new double[matrix.GetLength(0), matrix.GetLength(1)];
        double[,] res2 = new double[matrix.GetLength(0), matrix.GetLength(1)];
        //x-direction
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
                res1[i, j] = ProcessPoint(matrix, i, j, kernel, 0);
        }
        //y-direction
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
                res2[i, j] = ProcessPoint(res1, i, j, kernel, 1);
        }
        return res2;
    }
    private static double ProcessPoint(double[,] matrix, int x, int y, double[,] kernel, int direction)
    {
        double res = 0;
        int half = kernel.GetLength(0) / 2;
        for (int i = 0; i < kernel.GetLength(0); i++)
        {
            int cox = direction == 0 ? x + i - half : x;
            int coy = direction == 1 ? y + i - half : y;
            if (cox >= 0 && cox < matrix.GetLength(0) && coy >= 0 && coy < matrix.GetLength(1))
            {
                res += matrix[cox, coy] * kernel[i, 0];
            }
        }
        return res;
    }
    private static Color Grayscale(Color cr)
    {
        return Color.FromArgb(cr.A, (int)(cr.R * .3 + cr.G * .59 + cr.B * 0.11),
           (int)(cr.R * .3 + cr.G * .59 + cr.B * 0.11),
          (int)(cr.R * .3 + cr.G * .59 + cr.B * 0.11));
    }
    public static Bitmap FilterProcessImage(double d, Bitmap image)
    {
        Bitmap ret = new Bitmap(image.Width, image.Height);
        double[,] matrix = new double[image.Width, image.Height];
        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
                matrix[i, j] = Grayscale(image.GetPixel(i, j)).R;
        }
        matrix = GaussianConvolution(matrix, d);
        for (int i = 0; i < image.Width; i++)
        {
            for (int j = 0; j < image.Height; j++)
            {
                int val = (int)Math.Min(255, matrix[i, j]);
                ret.SetPixel(i, j, Color.FromArgb(255, val, val, val));
            }
        }
        return ret;
    }
    //public static double[,] GaussianBlur(int lenght, double weight)
    //{
    //    double[,] kernel = new double[lenght, lenght];
    //    double kernelSum = 0;
    //    int foff = (lenght - 1) / 2;
    //    double distance = 0;
    //    double constant = 1d / (2 * Math.PI * weight * weight);
    //    for (int y = -foff; y <= foff; y++)
    //    {
    //        for (int x = -foff; x <= foff; x++)
    //        {
    //            distance = ((y * y) + (x * x)) / (2 * weight * weight);
    //            kernel[y + foff, x + foff] = constant * Math.Exp(-distance);
    //            kernelSum += kernel[y + foff, x + foff];
    //        }
    //    }
    //    for (int y = 0; y < lenght; y++)
    //    {
    //        for (int x = 0; x < lenght; x++)
    //        {
    //            kernel[y, x] = kernel[y, x] * 1d / kernelSum;
    //        }
    //    }
    //    return kernel;
    //}
    //public static Bitmap Convolve(Bitmap srcImage, double[,] kernel)
    //{
    //    int width = srcImage.Width;
    //    int height = srcImage.Height;
    //    BitmapData srcData = srcImage.LockBits(new Rectangle(0, 0, width, height),
    //        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
    //    int bytes = srcData.Stride * srcData.Height;
    //    byte[] buffer = new byte[bytes];
    //    byte[] result = new byte[bytes];
    //    Marshal.Copy(srcData.Scan0, buffer, 0, bytes);
    //    srcImage.UnlockBits(srcData);
    //    int colorChannels = 3;
    //    double[] rgb = new double[colorChannels];
    //    int foff = (kernel.GetLength(0) - 1) / 2;
    //    int kcenter = 0;
    //    int kpixel = 0;
    //    for (int y = foff; y < height - foff; y++)
    //    {
    //        for (int x = foff; x < width - foff; x++)
    //        {
    //            for (int c = 0; c < colorChannels; c++)
    //            {
    //                rgb[c] = 0.0;
    //            }
    //            kcenter = y * srcData.Stride + x * 4;
    //            for (int fy = -foff; fy <= foff; fy++)
    //            {
    //                for (int fx = -foff; fx <= foff; fx++)
    //                {
    //                    kpixel = kcenter + fy * srcData.Stride + fx * 4;
    //                    for (int c = 0; c < colorChannels; c++)
    //                    {
    //                        rgb[c] += (double)(buffer[kpixel + c]) * kernel[fy + foff, fx + foff];
    //                    }
    //                }
    //            }
    //            for (int c = 0; c < colorChannels; c++)
    //            {
    //                if (rgb[c] > 255)
    //                {
    //                    rgb[c] = 255;
    //                }
    //                else if (rgb[c] < 0)
    //                {
    //                    rgb[c] = 0;
    //                }
    //            }
    //            for (int c = 0; c < colorChannels; c++)
    //            {
    //                result[kcenter + c] = (byte)rgb[c];
    //            }
    //            result[kcenter + 3] = 255;
    //        }
    //    }
    //    Bitmap resultImage = new Bitmap(width, height);
    //    BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
    //        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
    //    Marshal.Copy(result, 0, resultData.Scan0, bytes);
    //    resultImage.UnlockBits(resultData);
    //    return resultImage;
    //}
    //private static Bitmap Blur(Bitmap image, Int32 blurSize)
    //{
    //    return Blur(image, new Rectangle(0, 0, image.Width, image.Height), blurSize);
    //}

    //private unsafe static Bitmap Blur(Bitmap image, Rectangle rectangle, Int32 blurSize)
    //{
    //    Bitmap blurred = new Bitmap(image.Width, image.Height);

    //    // make an exact copy of the bitmap provided
    //    using (Graphics graphics = Graphics.FromImage(blurred))
    //        graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
    //            new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

    //    // Lock the bitmap's bits
    //    BitmapData blurredData = blurred.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, blurred.PixelFormat);

    //    // Get bits per pixel for current PixelFormat
    //    int bitsPerPixel = Image.GetPixelFormatSize(blurred.PixelFormat);

    //    // Get pointer to first line
    //    byte* scan0 = (byte*)blurredData.Scan0.ToPointer();

    //    // look at every pixel in the blur rectangle
    //    for (int xx = rectangle.X; xx < rectangle.X + rectangle.Width; xx++)
    //    {
    //        for (int yy = rectangle.Y; yy < rectangle.Y + rectangle.Height; yy++)
    //        {
    //            int avgR = 0, avgG = 0, avgB = 0;
    //            int blurPixelCount = 0;

    //            // average the color of the red, green and blue for each pixel in the
    //            // blur size while making sure you don't go outside the image bounds
    //            for (int x = xx; (x < xx + blurSize && x < image.Width); x++)
    //            {
    //                for (int y = yy; (y < yy + blurSize && y < image.Height); y++)
    //                {
    //                    // Get pointer to RGB
    //                    byte* data = scan0 + y * blurredData.Stride + x * bitsPerPixel / 8;

    //                    avgB += data[0]; // Blue
    //                    avgG += data[1]; // Green
    //                    avgR += data[2]; // Red

    //                    blurPixelCount++;
    //                }
    //            }

    //            avgR = avgR / blurPixelCount;
    //            avgG = avgG / blurPixelCount;
    //            avgB = avgB / blurPixelCount;

    //            // now that we know the average for the blur size, set each pixel to that color
    //            for (int x = xx; x < xx + blurSize && x < image.Width && x < rectangle.Width; x++)
    //            {
    //                for (int y = yy; y < yy + blurSize && y < image.Height && y < rectangle.Height; y++)
    //                {
    //                    // Get pointer to RGB
    //                    byte* data = scan0 + y * blurredData.Stride + x * bitsPerPixel / 8;

    //                    // Change values
    //                    data[0] = (byte)avgB;
    //                    data[1] = (byte)avgG;
    //                    data[2] = (byte)avgR;
    //                }
    //            }
    //        }
    //    }

    //    // Unlock the bits
    //    blurred.UnlockBits(blurredData);

    //    return blurred;
    //}
    //[Serializable]
    //public enum BlurType
    //{
    //    Both,
    //    HorizontalOnly,
    //    VerticalOnly,
    //}

    //[Serializable]
    //public class GaussianBlur
    //{
    //    private int _radius = 6;
    //    private int[] _kernel;
    //    private int _kernelSum;
    //    private int[,] _multable;
    //    private BlurType _blurType;

    //    public GaussianBlur()
    //    {
    //        PreCalculateSomeStuff();
    //    }

    //    public GaussianBlur(int radius)
    //    {
    //        _radius = radius;
    //        PreCalculateSomeStuff();
    //    }

    //    private void PreCalculateSomeStuff()
    //    {
    //        int sz = _radius * 2 + 1;
    //        _kernel = new int[sz];
    //        _multable = new int[sz, 256];
    //        for (int i = 1; i <= _radius; i++)
    //        {
    //            int szi = _radius - i;
    //            int szj = _radius + i;
    //            _kernel[szj] = _kernel[szi] = (szi + 1) * (szi + 1);
    //            _kernelSum += (_kernel[szj] + _kernel[szi]);
    //            for (int j = 0; j < 256; j++)
    //            {
    //                _multable[szj, j] = _multable[szi, j] = _kernel[szj] * j;
    //            }
    //        }
    //        _kernel[_radius] = (_radius + 1) * (_radius + 1);
    //        _kernelSum += _kernel[_radius];
    //        for (int j = 0; j < 256; j++)
    //        {
    //            _multable[_radius, j] = _kernel[_radius] * j;
    //        }
    //    }

    //    public Image ProcessImage(Image inputImage)
    //    {
    //        Bitmap origin = new(inputImage);
    //        Bitmap blurred = new(inputImage.Width, inputImage.Height);

    //        using (RawBitmap src = new RawBitmap(origin))
    //        {
    //            using (RawBitmap dest = new RawBitmap(blurred))
    //            {
    //                int pixelCount = src.Width * src.Height;
    //                int[] b = new int[pixelCount];
    //                int[] g = new int[pixelCount];
    //                int[] r = new int[pixelCount];

    //                int[] b2 = new int[pixelCount];
    //                int[] g2 = new int[pixelCount];
    //                int[] r2 = new int[pixelCount];

    //                int offset = src.GetOffset();
    //                int index = 0;
    //                //unsafe
    //                {
    //                    byte* ptr = src.Begin;
    //                    for (int i = 0; i < src.Height; i++)
    //                    {
    //                        for (int j = 0; j < src.Width; j++)
    //                        {
    //                            b[index] = *ptr;
    //                            ptr++;
    //                            g[index] = *ptr;
    //                            ptr++;
    //                            r[index] = *ptr;
    //                            ptr++;

    //                            ++index;
    //                        }
    //                        ptr += offset;
    //                    }

    //                    int bsum;
    //                    int gsum;
    //                    int rsum;
    //                    int sum;
    //                    int read;
    //                    int start = 0;
    //                    index = 0;
    //                    if (_blurType != BlurType.VerticalOnly)
    //                    {
    //                        for (int i = 0; i < src.Height; i++)
    //                        {
    //                            for (int j = 0; j < src.Width; j++)
    //                            {
    //                                bsum = gsum = rsum = sum = 0;
    //                                read = index - _radius;

    //                                for (int z = 0; z < _kernel.Length; z++)
    //                                {
    //                                    if (read >= start && read < start + src.Width)
    //                                    {
    //                                        bsum += _multable[z, b[read]];
    //                                        gsum += _multable[z, g[read]];
    //                                        rsum += _multable[z, r[read]];
    //                                        sum += _kernel[z];
    //                                    }
    //                                    ++read;
    //                                }

    //                                b2[index] = (bsum / sum);
    //                                g2[index] = (gsum / sum);
    //                                r2[index] = (rsum / sum);

    //                                if (_blurType == BlurType.HorizontalOnly)
    //                                {
    //                                    byte* pcell = dest[j, i];
    //                                    pcell[0] = (byte)(bsum / sum);
    //                                    pcell[1] = (byte)(gsum / sum);
    //                                    pcell[2] = (byte)(rsum / sum);
    //                                }

    //                                ++index;
    //                            }
    //                            start += src.Width;
    //                        }
    //                    }
    //                    if (_blurType == BlurType.HorizontalOnly)
    //                    {
    //                        return blurred;
    //                    }

    //                    int tempy;
    //                    for (int i = 0; i < src.Height; i++)
    //                    {
    //                        int y = i - _radius;
    //                        start = y * src.Width;
    //                        for (int j = 0; j < src.Width; j++)
    //                        {
    //                            bsum = gsum = rsum = sum = 0;
    //                            read = start + j;
    //                            tempy = y;
    //                            for (int z = 0; z < _kernel.Length; z++)
    //                            {
    //                                if (tempy >= 0 && tempy < src.Height)
    //                                {
    //                                    if (_blurType == BlurType.VerticalOnly)
    //                                    {
    //                                        bsum += _multable[z, b[read]];
    //                                        gsum += _multable[z, g[read]];
    //                                        rsum += _multable[z, r[read]];
    //                                    }
    //                                    else
    //                                    {
    //                                        bsum += _multable[z, b2[read]];
    //                                        gsum += _multable[z, g2[read]];
    //                                        rsum += _multable[z, r2[read]];
    //                                    }
    //                                    sum += _kernel[z];
    //                                }
    //                                read += src.Width;
    //                                ++tempy;
    //                            }

    //                            byte* pcell = dest[j, i];
    //                            pcell[0] = (byte)(bsum / sum);
    //                            pcell[1] = (byte)(gsum / sum);
    //                            pcell[2] = (byte)(rsum / sum);
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        return blurred;
    //    }
    //}
}
