using System.Runtime.InteropServices;
using WebviewGtk.Interop;

namespace WebviewGtk;

public static partial class WebkitGtkWrapper
{
    private static readonly GObject.DestroyCallback WindowDestroySignalHandler = OnWindowDestroy;
    private static readonly WebKitGtk.CloseCallback WebviewCloseSignalHandler = OnWebviewClose;
    private static readonly WebKitGtk.DecidePolicyCallback DecidePolicyHandler = OnDecidePolicy;
    private static readonly WebKitGtk.ContextMenuCallback ContextMenuHandler = OnContextMenuDisabled;
    private static WebKitGtk.ScriptMessageReceivedCallback? _onScriptMessageReceivedHandler;
    private static GObject.SignalHandler? _onLoadChangedHandler;
    
    
    private static void DisableWebViewSelection()
    {
        IntPtr manager = WebKitGtk.WebView.GetUserContentManager(_webView);
        if (manager == IntPtr.Zero)
        {
            // Если вдруг null, значит нужно проверить, 
            // поддерживает ли сборка user content manager
            Console.Error.WriteLine("No UserContentManager found");
            return;
        }

        // JavaScript, который отключает выделение на всю страницу.
        string disableSelectionJs = """
                                        document.addEventListener('DOMContentLoaded', function() {
                                            document.body.style.userSelect = 'none';
                                            document.body.style.webkitUserSelect = 'none'; // Для старых движков
                                            document.body.style.mozUserSelect = 'none';
                                        });
                                    """;

        // Создаём объект UserScript.
        IntPtr userScript = WebKitGtk.UserScriptNew(
            disableSelectionJs,
            WebKitGtk.WebKitUserContentInjectedFrames.AllFrames,
            WebKitGtk.WebKitUserScriptInjectionTime.AtDocumentStart,
            IntPtr.Zero,
            IntPtr.Zero
        );

        if (userScript != IntPtr.Zero)
        {
            // Добавляем скрипт
            WebKitGtk.UserContentManager.AddScript(manager, userScript);
        }
    }

    private static bool OnDecidePolicy(IntPtr webview, IntPtr decision, IntPtr type, IntPtr userdata)
    {
        if (_config is null || !_config.StrictMode) return false;

        int loadEventInt = type.ToInt32();
        WebKitGtk.WebkitPolicyDecisionType decisionType = (WebKitGtk.WebkitPolicyDecisionType)loadEventInt;

        if (decisionType != WebKitGtk.WebkitPolicyDecisionType.NavigationAction) return false;

        IntPtr navAction = WebKitGtk.Navigation.PolicyDecisionGetNavigationAction(decision);

        if (navAction == IntPtr.Zero) return false;

        IntPtr request = WebKitGtk.Navigation.ActionGetRequest(navAction);

        if (request == IntPtr.Zero) return false;

        IntPtr uriPtr = WebKitGtk.UriRequestGetUri(request);
        string? currentUri = uriPtr == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(uriPtr);

        if (string.IsNullOrWhiteSpace(currentUri) 
            || currentUri.StartsWith("about", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Uri uri = new Uri(currentUri);
        bool canNavigate =
            _config.AllowedUrls.Any(aUri =>
                uri.Scheme == aUri.Scheme
                && uri.Host == aUri.Host
                && uri.Port == aUri.Port);
        
        if (canNavigate)
        {
            return false;
        }

        // Отклоняем
        Console.Error.WriteLine($"Uri is not allowed: {currentUri}:");
        WebKitGtk.PolicyDecisionIgnore(decision);
        return true;
    }
    
    private static bool OnContextMenuDisabled(
        IntPtr webview, IntPtr menu, IntPtr @event, IntPtr hitTestResult, IntPtr userdata)
    {
        // Меняем меню: удаляем все, кроме Copy, Cut, Paste.
        uint nItems = WebKitGtk.ContextMenu.GetNItems(menu);
        uint i = 0;

        while (i < nItems)
        {
            IntPtr item = WebKitGtk.ContextMenu.GetItemAtPosition(menu, i);
            WebKitGtk.MenuAction action = WebKitGtk.ContextMenu.ItemGetStockAction(item);

            // Если это не (Copy, Cut, Paste, SelectAll), убираем.
            if (action != WebKitGtk.MenuAction.Copy &&
                action != WebKitGtk.MenuAction.Cut &&
                action != WebKitGtk.MenuAction.Paste &&
                action != WebKitGtk.MenuAction.SelectAll)
            {
                WebKitGtk.ContextMenu.Remove(menu, item);
                nItems--; 
                // Не инкрементируем i, так как следующий item "сдвинулся" на позицию i.
            }
            else
            {
                i++;
            }
        }

        // Если вернуть true => меню вообще не покажут
        // Возвращаем false => покажется изменённое меню
        return false;
    }

    private static void DestroyWindow()
    {
        if (_window != IntPtr.Zero)
        {
            Gtk.Widget.Destroy(_window);
            _window = IntPtr.Zero;
        }
    }

    private static void OnWindowDestroy(IntPtr widget, IntPtr userdata)
    {
        DestroyWindow();
        Gtk.MainQuit();
    }

    private static bool OnWebviewClose(IntPtr webview, IntPtr userdata)
    {
        DestroyWindow();
        return true;
    }
}