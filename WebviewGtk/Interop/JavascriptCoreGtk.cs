using System.Runtime.InteropServices;
using WebviewGtk.Constants;

namespace WebviewGtk.Interop;

internal partial class JavascriptCoreGtk
{
    [LibraryImport(Libraries.JavascriptCoreGtk)]
    internal static partial IntPtr JSValueToStringCopy(IntPtr ctx, IntPtr value, out IntPtr exception);

    [LibraryImport(Libraries.JavascriptCoreGtk)]
    internal static partial IntPtr JSStringGetMaximumUTF8CStringSize(IntPtr jsStr);

    [LibraryImport(Libraries.JavascriptCoreGtk)]
    internal static partial IntPtr JSStringGetUTF8CString(IntPtr jsStr, IntPtr buffer, IntPtr bufferSize);

    [LibraryImport(Libraries.JavascriptCoreGtk)]
    internal static partial void JSStringRelease(IntPtr jsStr);

}