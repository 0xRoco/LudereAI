using System.Drawing;
using LudereAI.WPF.Models;

namespace LudereAI.WPF.Interfaces;

public interface IScreenshotService
{
    Bitmap TakeScreenshot(IntPtr handle);
    void SaveScreenshot(IntPtr handle, string path);
    string GetBase64Screenshot(IntPtr handle);
}