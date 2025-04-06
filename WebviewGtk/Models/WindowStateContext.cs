namespace WebviewGtk.Models;

internal sealed class WindowStateContext
{
    public WindowStateContext(IntPtr window, WindowStartupState state)
    {
        if (window == IntPtr.Zero)
        {
            throw new InvalidOperationException("WindowStateContext: Window pointer is null.");
        }
        
        Window = window;
        State = state;
    }
    
    public IntPtr Window { get; }
    public WindowStartupState State { get; }
}