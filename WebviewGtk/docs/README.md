# WebviewGtk

A lightweight C# wrapper around WebKitGTK to embed a native web browser in your Linux desktop apps with minimal overhead.

| ⚠️ This project is primarily intended for personal use, but might be helpful as a base for other GTK-powered .NET apps for linux.

## Features

- Splash screen overlay while the page loads
- StrictMode: limit navigation to a single base URI
- JavaScript ↔ C# command bridge (via window.__backendCallback)
- Context menu disabling
- Developer tools toggle (DebugMode)
- Optional text selection blocking
- Safe shutdown via CancellationToken

## Quickstart

### Basic usage

```csharp
WebViewConfig config = new()
{
    StartUri = new Uri("https://example.com"),
    WindowTitle = "My App",
    Width = 1280,
    Height = 720,
    AllowSelection = false,
    SplashFilename = "loading.gif",
    DebugMode = false,
    StrictMode = true,
    WindowPosition = Gtk.WindowPosition.Center
};

WebkitGtkWrapper.RunWebkit(config, (evt, uri) =>
{
    Console.WriteLine($"Navigation event: {evt}, URI: {uri}");
}, CancellationToken.None);
```
### With JS command handler
```csharp
WebkitGtkWrapper.RunWebkitWithHandler(config,
    (evt, uri) => Console.WriteLine($"Changed: {evt}, URI: {uri}"),
    command =>
    {
        Console.WriteLine($"Received from JS: {command}");
        return "\"response from backend\"";
    },
    CancellationToken.None);
```
On the JS side, you can send messages like:

```javascript
window.webkit.messageHandlers.backend.postMessage("your-command");
```

And receive responses via:

```javascript
window.__backendCallback?.("your-result");
```

## Native Dependencies

| Library           | Shared Object Name            | Description                        |
|-------------------|-------------------------------|------------------------------------|
| GLib              | libglib-2.0.so.0	             | Core GNOME utility library         |
| GObject           | libgobject-2.0.so.0	          | GObject type system                |
| Gtk               | libgtk-3.so.0	                | GTK+ 3 UI toolkit                  |
| GdkPixbuf         | libgdk_pixbuf-2.0.so          | Image loading for splash animation |
| JavascriptCoreGtk | libjavascriptcoregtk-4.1.so.0 | JavaScript engine behind WebKitGTK |
| WebKit            | libwebkit2gtk-4.1.so.0        | WebView rendering engine           |


**.NET 8+ is required.**

## Architecture Notes

- Uses low-level P/Invoke to call GTK and WebKitGTK APIs directly.
- IntPtr-based interop, with manual GCHandle pinning to prevent delegate collection.
- No third-party UI frameworks — clean low-level interop only.
- Compatible with Linux desktops, and potentially usable via WSL/X with X-server (not officially tested).

## WebViewConfig Fields (summary)

| Field          | Type                | Description                               |
|----------------|---------------------|-------------------------------------------|
| StartUri       | 	Uri                | 	The initial web address to load          |
| WindowTitle    | 	string             | 	Window title text                        |
| Width/Height   | 	int                | 	Default window size                      |
| AllowSelection | 	bool               | 	Allow text selection in the WebView      |
| StrictMode     | 	bool               | 	Restrict navigation to the base URI      |
| DebugMode      | 	bool               | 	Enable WebKit developer tools            |
| SplashFilename | 	string?            | 	Optional path to loading animation (GIF) |
| WindowPosition | 	Gtk.WindowPosition | 	GTK window placement enum                |

## License

This project is licensed under the GNU General Public License v3.0 (GPL-3.0).

You are free to use, modify, and distribute this software — but any distributed binaries or derived works must also be open-sourced under the same license.

**TL;DR** 

If you use this in your own project and distribute it — you must also share your source code under GPL-3.0.

Full license text is available in the LICENSE file or at https://www.gnu.org/licenses/gpl-3.0.html

## Author

[Jackhammer88](https://github.com/Jackhammer88) C# developer, interop enjoyer, and fan of unsafe elegance.