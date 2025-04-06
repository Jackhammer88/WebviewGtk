using System.Runtime.InteropServices;
using WebviewGtk.Constants;

namespace WebviewGtk.Interop;

internal static partial class GdkPixbuf
{
    [LibraryImport(Libraries.GdkPixbuf, EntryPoint = "gdk_pixbuf_new_from_file",
        StringMarshalling =  StringMarshalling.Utf8)]
    internal static partial IntPtr NewFromFile(string filename, ref IntPtr error);
    
    [LibraryImport(Libraries.Gtk, EntryPoint = "gdk_pixbuf_animation_new_from_file",
        StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr AnimationNewFromFile(string filename, IntPtr error);
}