using System.Drawing;

namespace LudereAI.Core.Interfaces.Services;

public interface IScreenshotService
{
    Bitmap TakeScreenshot(IntPtr handle);
    void SaveScreenshot(IntPtr handle, string path);
    string GetBase64Screenshot(IntPtr handle);
}