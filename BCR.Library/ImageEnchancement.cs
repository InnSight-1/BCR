using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

namespace BCR.Library;
public class ImageEnchancement
{
    [SupportedOSPlatform("windows")]
    public static Bitmap AdjustBCG(Bitmap bmp, float gamma)
    {
        float brightness = .96f;//1.2f; //1.2f is for brighter image (helps when black marker interferes with barcode)
        float contrast = 1.1f; //1.0f; //1.0f is for brighter image (helps when black marker interferes with barcode)
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
}
