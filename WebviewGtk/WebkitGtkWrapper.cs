using System.Runtime.InteropServices;
using WebviewGtk.Interop;
using WebviewGtk.Models;
using GBoolean = WebviewGtk.Interop.GObject.GBoolean;

namespace WebviewGtk;

public static partial class WebkitGtkWrapper
{
    private static WebViewConfig? _config;

    private static IntPtr _window = IntPtr.Zero;
    private static IntPtr _webView = IntPtr.Zero;
    private static IntPtr _overlay = IntPtr.Zero;
    private static IntPtr _animation = IntPtr.Zero;
    private static IntPtr _loadingAnimation = IntPtr.Zero;
    private static bool _splashVisible;
    private static GCHandle? _jsCallbackHandle;
    private static GCHandle _onLoadChangedHandle;

    /// <summary>
    /// Launches WebKit with the specified configuration.
    /// </summary>
    /// <param name="config">WebKit configuration options.</param>
    /// <param name="loadChangedCallback">
    /// Callback invoked on page load events, navigation changes, and window close.
    /// </param>
    /// <param name="token">Cancellation token for gracefully shutting down GTK.</param>
    /// <exception cref="InvalidOperationException">Thrown if WebKit fails to initialize.</exception>
    /// <exception cref="NullReferenceException">Thrown if GTK initialization fails.</exception>
    public static void RunWebkit(
        WebViewConfig config,
        Action<WebKitLoadEvent, string?> loadChangedCallback,
        CancellationToken token)
    {
        if (!GtkWrapper.Initialized.Value)
        {
            throw new NullReferenceException("Gtk is not initialized.");
        }

        if (_window != IntPtr.Zero) // Если уже открыто
        {
            throw new InvalidOperationException("Webkit is already running.");
        }

        _config = config;

        _window = Gtk.Window.New(Gtk.WindowType.GtkWindowToplevel);

        if (_window == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GtkWindow.");
        }

        Gtk.Window.SetTitle(_window, _config.WindowTitle);
        Gtk.Window.SetDefaultSize(_window, config.Width, config.Height);

        // Подписываемся на событие закрытия окна.
        GObject.GSignalConnectData(
            _window, Gtk.Events.Destroy,
            Marshal.GetFunctionPointerForDelegate(WindowDestroySignalHandler),
            IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);

        _overlay = Gtk.Overlay.New();

        Gtk.Container.Add(_window, _overlay);

        _webView = WebKitGtk.WebView.New();
        if (_webView == IntPtr.Zero)
        {
            GObject.Unref(_window);
            _window = IntPtr.Zero;
            throw new InvalidOperationException("Failed to create webview.");
        }

        if (!_config.AllowSelection)
        {
            DisableWebViewSelection();
        }

        Gtk.Container.Add(_overlay, _webView);

        // Подписываемся на событие закрытия WebView
        GObject.GSignalConnectData(
            _webView, WebKitGtk.Events.Close,
            Marshal.GetFunctionPointerForDelegate(WebviewCloseSignalHandler),
            IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);

        // Подписываемся на событие навигации
        _onLoadChangedHandler = (webView, loadEventPtr, _) =>
        {
            int loadEventInt = loadEventPtr.ToInt32();
            var loadEvent = (WebKitLoadEvent)loadEventInt;

            // Выключение splash загрузки
            if (_splashVisible && loadEvent is WebKitLoadEvent.WebkitLoadStarted)
            {
                Gtk.Widget.Hide(_loadingAnimation);
                _splashVisible = false;
            }

            IntPtr uriPtr = WebKitGtk.WebView.GetUri(webView);
            string? currentUri = uriPtr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(uriPtr);
            loadChangedCallback.Invoke(loadEvent, currentUri);
        };
        // Предотвращаем сборку GC
        _onLoadChangedHandle = GCHandle.Alloc(_webView);

        GObject.GSignalConnectData(
            _webView, WebKitGtk.Events.LoadChanged, Marshal.GetFunctionPointerForDelegate(_onLoadChangedHandler),
            IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);

        if (config.StrictMode) // Строгий режим. Только базовый адрес.
        {
            GObject.GSignalConnectData(_webView, WebKitGtk.Events.DecidePolicy,
                Marshal.GetFunctionPointerForDelegate(DecidePolicyHandler),
                IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);
        }

        if (config.DebugMode)
        {
            IntPtr settings = WebKitGtk.WebView.GetSettings(_webView);
            WebKitGtk.Settings.SetEnableDeveloperExtras(settings, GBoolean.True);
        }
        else
        {
            // Отключение контекстного меню
            GObject.GSignalConnectData(_webView, WebKitGtk.Events.Contextmenu,
                Marshal.GetFunctionPointerForDelegate(ContextMenuHandler),
                IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);
        }

        // Если задан splash, то устанавливаем.
        if (!string.IsNullOrWhiteSpace(config.SplashFilename) && File.Exists(config.SplashFilename))
        {
            _animation = GdkPixbuf.AnimationNewFromFile(config.SplashFilename, IntPtr.Zero);
            if (_animation != IntPtr.Zero)
            {
                _loadingAnimation = Gtk.Image.NewFromAnimation(_animation);
                Gtk.Overlay.AddOverlay(_overlay, _loadingAnimation);
                Gtk.Widget.SetHAlign(_loadingAnimation, Gtk.GtkAlign.GtkAlignCenter);
                Gtk.Widget.SetVAlign(_loadingAnimation, Gtk.GtkAlign.GtkAlignCenter);
                _splashVisible = true;
            }
        }

        WebKitGtk.WebView.LoadUri(_webView, config.StartUri.ToString());

        Gtk.Window.SetPosition(_window, _config.WindowPosition);
        Gtk.Widget.ShowAll(_window);
        
        // Регистрация метода отмены.
        token.Register(() =>
        {
            Console.WriteLine("Token canceled, shutting down GTK...");
            GLib.IdleAdd(static _ =>
            {
                Gtk.MainQuit();
                return false;
            }, IntPtr.Zero);
        });
        
        SetWindowInitialState(_window, _config.WindowStartupState);
        
        Gtk.Main();
        FreeHandles();
    }

    /// <summary>
    /// Launches WebKit with the specified configuration and a JavaScript command handler.
    /// </summary>
    /// <param name="config">WebKit configuration options.</param>
    /// <param name="loadChangedCallback">
    /// Callback invoked on page load events, navigation changes, and when the window is closed.
    /// </param>
    /// <param name="commandHandler">
    /// Function that receives messages from JavaScript (via `window.webkit.messageHandlers.backend.postMessage`)
    /// and returns a response string.
    /// </param>
    /// <param name="token">Cancellation token for gracefully shutting down GTK.</param>
    /// <exception cref="InvalidOperationException">Thrown if WebKit fails to initialize.</exception>
    /// <exception cref="NullReferenceException">Thrown if GTK is not initialized.</exception>
    public static void RunWebkitWithHandler(
        WebViewConfig config,
        Action<WebKitLoadEvent, string?> loadChangedCallback,
        Func<string, string> commandHandler,
        CancellationToken token)
    {
        if (!GtkWrapper.Initialized.Value)
        {
            throw new NullReferenceException("Gtk is not initialized.");
        }

        if (_window != IntPtr.Zero) // Если уже открыто
        {
            throw new InvalidOperationException("Webkit is already running.");
        }

        _config = config;

        _window = Gtk.Window.New(Gtk.WindowType.GtkWindowToplevel);

        if (_window == IntPtr.Zero)
        {
            throw new InvalidOperationException("Failed to create GtkWindow.");
        }

        Gtk.Window.SetTitle(_window, _config.WindowTitle);
        Gtk.Window.SetDefaultSize(_window, config.Width, config.Height);

        // Подписываемся на событие закрытия окна.
        GObject.GSignalConnectData(
            _window, Gtk.Events.Destroy, Marshal.GetFunctionPointerForDelegate(WindowDestroySignalHandler),
            IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);

        _overlay = Gtk.Overlay.New();

        Gtk.Container.Add(_window, _overlay);

        IntPtr manager = WebKitGtk.UserContentManager.UserContentManagerNew();
        if (WebKitGtk.UserContentManager.UserContentManagerRegisterScriptMessageHandler(manager, "backend") ==
            GBoolean.False)
        {
            throw new InvalidOperationException("Backend registration failed.");
        }

        _webView = WebKitGtk.WebView.NewWithUserContentManager(manager);
        if (_webView == IntPtr.Zero)
        {
            GObject.Unref(_window);
            _window = IntPtr.Zero;
            throw new InvalidOperationException("Failed to create webview.");
        }

        if (!_config.AllowSelection)
        {
            DisableWebViewSelection();
        }

        Gtk.Container.Add(_overlay, _webView);

        // Подписываемся на событие закрытия WebView
        GObject.GSignalConnectData(
            _webView, WebKitGtk.Events.Close, Marshal.GetFunctionPointerForDelegate(WebviewCloseSignalHandler),
            IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);

        // Подписываемся на событие навигации
        _onLoadChangedHandler = (webView, loadEventPtr, _) =>
        {
            int loadEventInt = loadEventPtr.ToInt32();
            var loadEvent = (WebKitLoadEvent)loadEventInt;

            // Выключение splash загрузки
            if (_splashVisible && loadEvent is WebKitLoadEvent.WebkitLoadStarted)
            {
                Gtk.Widget.Hide(_loadingAnimation);
                _splashVisible = false;
            }

            IntPtr uriPtr = WebKitGtk.WebView.GetUri(webView);
            string? currentUri = uriPtr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(uriPtr);
            loadChangedCallback.Invoke(loadEvent, currentUri);
        };
        // Предотвращаем сборку GC
        _onLoadChangedHandle = GCHandle.Alloc(_onLoadChangedHandler);

        GObject.GSignalConnectData(
            _webView, WebKitGtk.Events.LoadChanged, Marshal.GetFunctionPointerForDelegate(_onLoadChangedHandler),
            IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);

        if (config.StrictMode) // Строгий режим. Только базовый адрес.
        {
            GObject.GSignalConnectData(_webView, WebKitGtk.Events.DecidePolicy,
                Marshal.GetFunctionPointerForDelegate(DecidePolicyHandler),
                IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);
        }

        if (config.DebugMode)
        {
            IntPtr settings = WebKitGtk.WebView.GetSettings(_webView);
            WebKitGtk.Settings.SetEnableDeveloperExtras(settings, GBoolean.True);
        }
        else
        {
            // Отключение контекстного меню
            GObject.GSignalConnectData(_webView, WebKitGtk.Events.Contextmenu,
                Marshal.GetFunctionPointerForDelegate(ContextMenuHandler),
                IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);
        }

        // Если задан splash, то устанавливаем.
        if (!string.IsNullOrWhiteSpace(config.SplashFilename) && File.Exists(config.SplashFilename))
        {
            _animation = GdkPixbuf.AnimationNewFromFile(config.SplashFilename, IntPtr.Zero);
            if (_animation != IntPtr.Zero)
            {
                _loadingAnimation = Gtk.Image.NewFromAnimation(_animation);
                Gtk.Overlay.AddOverlay(_overlay, _loadingAnimation);
                Gtk.Widget.SetHAlign(_loadingAnimation, Gtk.GtkAlign.GtkAlignCenter);
                Gtk.Widget.SetVAlign(_loadingAnimation, Gtk.GtkAlign.GtkAlignCenter);
                _splashVisible = true;
            }
        }

        // Bridge
        _onScriptMessageReceivedHandler = (_, jsResult, _) =>
        {
            IntPtr ctx = WebKitGtk.Javascript.GetGlobalContext(jsResult);
            IntPtr value = WebKitGtk.Javascript.ResultGetValue(jsResult);

            IntPtr exception;
            IntPtr jsStr = JavascriptCoreGtk.JSValueToStringCopy(ctx, value, out exception);
            if (jsStr == IntPtr.Zero)
            {
                throw new InvalidOperationException("JSValueToStringCopy failed");
            }

            int size = (int)JavascriptCoreGtk.JSStringGetMaximumUTF8CStringSize(jsStr);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            JavascriptCoreGtk.JSStringGetUTF8CString(jsStr, buffer, (IntPtr)size);
            string commandRequest = Marshal.PtrToStringUTF8(buffer)!;
            Marshal.FreeHGlobal(buffer);
            JavascriptCoreGtk.JSStringRelease(jsStr);

            string commandResult = commandHandler(commandRequest);
            WebKitGtk.Javascript.RunJavaScript(_webView,
                $"window.__backendCallback?.({commandResult});",
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        };
        // Предотвращаем сборку GC
        _jsCallbackHandle = GCHandle.Alloc(_onScriptMessageReceivedHandler);

        GObject.GSignalConnectData(
            manager, WebKitGtk.Events.ScriptMessageReceivedBackend,
            Marshal.GetFunctionPointerForDelegate(_onScriptMessageReceivedHandler),
            IntPtr.Zero, IntPtr.Zero, GObject.GConnectFlags.GConnectDefault);


        WebKitGtk.WebView.LoadUri(_webView, config.StartUri.ToString());

        Gtk.Window.SetPosition(_window, _config.WindowPosition);
        Gtk.Widget.ShowAll(_window);

        // Регистрация метода отмены.
        token.Register(() =>
        {
            Console.WriteLine("Token canceled, shutting down GTK...");
            GLib.IdleAdd(static _ =>
            {
                Gtk.MainQuit();
                return false;
            }, IntPtr.Zero);
        });

        SetWindowInitialState(_window, config.WindowStartupState);
        
        Gtk.Main();
        FreeHandles();
    }
    
    private static void SetWindowInitialState(
        IntPtr window, 
        WindowStartupState state)
    {
        if (state == WindowStartupState.Normal) return;
        
        WindowStateContext stateInfo = new(window, state);
        GCHandle handle = GCHandle.Alloc(stateInfo);
        
        GLib.IdleAdd(static state =>
        {
            var h = (GCHandle)state;
            if (h.Target is null)
            {
                h.Free();
                return false;
            }
            
            var context = (WindowStateContext)h.Target;
            var win = context.Window;
            
            switch (context.State)
            {
                case WindowStartupState.Minimized:
                    Gtk.Window.Iconify(win);
                    break;
                case WindowStartupState.Maximized:
                    Gtk.Window.Maximize(win);
                    break;
                case WindowStartupState.FullScreen:
                    Gtk.Window.Fullscreen(win);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }

            h.Free();
            return false;
        }, GCHandle.ToIntPtr(handle));
    }

    private static void FreeHandles()
    {
        _onLoadChangedHandle.Free();
        _jsCallbackHandle?.Free();
    }
}