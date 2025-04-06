using System.Runtime.InteropServices;
using WebviewGtk.Constants;

namespace WebviewGtk.Interop;

public static partial class GLib
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool GSourceFunc(IntPtr data);

    [LibraryImport(Libraries.GLib, EntryPoint = "g_idle_add")]
    internal static partial uint IdleAdd(GSourceFunc func, IntPtr data);

    
    [LibraryImport(Libraries.GLib, EntryPoint = "g_free")]
    private static partial void Free(IntPtr ptr);

    internal static string GetGErrorMessageAndFree(IntPtr errorPtr)
    {
        if (errorPtr == IntPtr.Zero)
            return "Unknown error (null GError)";

        var error = Marshal.PtrToStructure<GObject.GError>(errorPtr);
        string msg = Marshal.PtrToStringUTF8(error.message) ?? "Unknown error";

        // Освобождаем GError
        Free(errorPtr);
        return msg;
    }
}