using System.Runtime.InteropServices;
using WebviewGtk.Constants;

namespace WebviewGtk.Interop;

internal static partial class GObject
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void SignalHandler(IntPtr instance, IntPtr parameter, IntPtr data);
    
    // Сигнал, который ничего не возвращает (destroy)
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DestroyCallback(IntPtr widget, IntPtr userData);
    
    internal enum GBoolean : int
    {
        True = 1,
        False = 0,
    }
    
    [StructLayout(LayoutKind.Sequential)]
    internal struct GError
    {
        public uint domain;
        public int code;
        public IntPtr message;
    }
    
    [Flags]
    internal enum GConnectFlags : uint
    {
        GConnectDefault = 0,
        GConnectAfter   = 1 << 0,
        GConnectSwapped = 1 << 1
    }
    
    [LibraryImport(Libraries.GObject, EntryPoint = "g_object_unref")]
    internal static partial void Unref(IntPtr obj);

    [LibraryImport(Libraries.GObject, EntryPoint = "g_signal_connect_data", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial ulong GSignalConnectData(
        IntPtr instance,
        string detailedSignal,
        IntPtr cHandler, // Делегат в виде Delegate, чтобы можно было передавать любой тип.
        IntPtr data,
        IntPtr destroyData,
        GConnectFlags connectFlags
    );
}