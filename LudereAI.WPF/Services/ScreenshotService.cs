using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using LudereAI.Core.Interfaces.Services;
using LudereAI.WPF.Infrastructure;
using Microsoft.Extensions.Logging;
using Encoder = System.Drawing.Imaging.Encoder;

namespace LudereAI.WPF.Services;

public class ScreenshotService(ILogger<IScreenshotService> logger) : IScreenshotService
{
    private const int TargetWidth = 1024;
    private const int TargetHeight = 768;
    private const long MaxScreenshotSize = 2 * 1024 * 1024; // 2 MB
    private const int DefaultScreenshotQuality = 75;

    public Bitmap TakeScreenshot(IntPtr handle)
    {
        if (handle == IntPtr.Zero)
            throw new ArgumentException("Window handle cannot be zero", nameof(handle));

        GetWindowRect(handle, out var rect);
        
        if (rect.Width <= 0 || rect.Height <= 0)
            throw new InvalidOperationException("Invalid window dimensions");
        
        var bmp = CaptureWindow(handle, rect);
        return ResizeImage(bmp, TargetWidth, TargetHeight);
    }
    public void SaveScreenshot(IntPtr handle, string path)
    {
        try
        {
            using var screenshot = TakeScreenshot(handle);
            var outputPath = Path.ChangeExtension(path, ".jpg");
            var imageBytes = GetOptimizedImageBytes(screenshot);
            
            using var fs = new FileStream(outputPath, FileMode.Create);
            fs.Write(imageBytes, 0, imageBytes.Length);
            
            Console.WriteLine($"Screenshot saved: Size={imageBytes.Length / 1024} KB, Path={outputPath}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save screenshot to {Path}", path);
        }
    }

    public string GetBase64Screenshot(IntPtr handle)
    {
        using var screenshot = TakeScreenshot(handle);
        var imageBytes = GetOptimizedImageBytes(screenshot);
        return Convert.ToBase64String(imageBytes);
    }
    
    private Bitmap CaptureWindow(IntPtr handle, Rect rect)
    {
        var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
        try
        {
            using var gfxBmp = Graphics.FromImage(bmp);
            var hdcBitmap = gfxBmp.GetHdc();
            
            try
            {
                if (!PrintWindow(handle, hdcBitmap, 2))
                {
                    throw new InvalidOperationException("Failed to capture window content");
                }
            }
            finally
            {
                gfxBmp.ReleaseHdc(hdcBitmap);
            }
        }
        catch
        {
            bmp.Dispose();
            throw;
        }

        return bmp; 
    }

    private byte[] GetOptimizedImageBytes(Bitmap image)
    {
        var jpegEncoder = GetJpegEncoder();
        var encoderParameters = new EncoderParameters(1);
        var quality = DefaultScreenshotQuality;
        
        using var ms = new MemoryStream();
        
        while (true)
        {
            using var qualityParam = new EncoderParameter(Encoder.Quality, quality);
            encoderParameters.Param[0] = qualityParam;
            
            ms.SetLength(0);
            image.Save(ms, jpegEncoder, encoderParameters);
            
            if (ms.Length <= MaxScreenshotSize || quality <= 50)
                break;
                
            quality -= 2; 
        }
        
        return ms.ToArray();
    }

    private void SaveAsJpeg(Bitmap image, string path, int quality)
    {
        var jpegEncoder = GetJpegEncoder();
        var encoderParameters = new EncoderParameters(1);
        var qualityParam = new EncoderParameter(Encoder.Quality, quality);

        try
        {
            using var ms = new MemoryStream();

            encoderParameters.Param[0] = qualityParam;
            image.Save(ms, jpegEncoder, encoderParameters);

            while (ms.Length > MaxScreenshotSize && quality > 50)
            {
                quality -= 5;
                ms.SetLength(0);
                qualityParam.Dispose();
                qualityParam = new EncoderParameter(Encoder.Quality, quality);
                encoderParameters.Param[0] = qualityParam;
                image.Save(ms, jpegEncoder, encoderParameters);
            }

            using var fs = new FileStream(path, FileMode.Create);
            ms.WriteTo(fs);
        }
        finally
        {
            qualityParam.Dispose();
            encoderParameters.Dispose();
        }
    }
    
    private Bitmap ResizeImage(Bitmap image, int targetWidth, int targetHeight)
    {
        var resized = new Bitmap(targetWidth, targetHeight);
        using var gfx = Graphics.FromImage(resized);
        gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        gfx.DrawImage(image, 0, 0, targetWidth, targetHeight);
        return resized;
    }
    
    private ImageCodecInfo GetJpegEncoder()
    {
        var codecs = ImageCodecInfo.GetImageEncoders();
        return codecs.First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
    }
    
    //TODO: Use LibraryImport instead of DllImport attribute for better performance + compatibility
    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out Rect lpRect);
    [DllImport("user32.dll")]
    private static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);
    
}