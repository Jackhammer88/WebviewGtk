using WebviewGtk.Interop;

namespace WebviewGtk;

public static class GtkWrapper
{
    internal static Lazy<bool> Initialized { get; } = new (Initialize, true);

    private static bool Initialize()
    {
        int argc = 0;
        IntPtr argv = IntPtr.Zero;
        var result = Gtk.InitCheck(ref argc, ref argv);

        if (result != GObject.GBoolean.True)
        {
            throw new InvalidOperationException("Gtk init failed.");
        }
        
        return true;
    }
}