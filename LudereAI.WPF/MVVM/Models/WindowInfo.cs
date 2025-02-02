namespace LudereAI.WPF.MVVM.Models;

public class WindowInfo
{
    public IntPtr Handle { get; set; }
    public string Title { get; set; }
    public string ProcessName { get; set; }
    public int ProcessId { get; set; }
    
    
    public override string ToString()
    {
        return $"{Title} ({ProcessName})";
    }
}