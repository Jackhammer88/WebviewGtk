using System.Runtime.InteropServices;
using WebviewGtk.Constants;

namespace WebviewGtk.Interop;

internal static partial class WebKitGtk
{
    public static class Events
    {
        public const string Close = "close";
        public const string LoadChanged = "load-changed";
        public const string DecidePolicy = "decide-policy";
        public const string Contextmenu = "context-menu";
        public const string ScriptMessageReceivedBackend = "script-message-received::backend";
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void ScriptMessageReceivedCallback(IntPtr manager, IntPtr jsResult, IntPtr userData);

    // Сигнал, который возвращает gboolean (close)
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)] // gboolean => bool
    internal delegate bool CloseCallback(IntPtr webView, IntPtr userData);

    // Сигнал decide-policy
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)] // gboolean => bool
    internal delegate bool DecidePolicyCallback(IntPtr webView, IntPtr decision, IntPtr type, IntPtr userData);


    // Сигнал context-menu
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)] // gboolean => bool
    internal delegate bool ContextMenuCallback(IntPtr webView, IntPtr menu, IntPtr @event, IntPtr hitTestResult,
        IntPtr userData);

    internal enum WebKitUserContentInjectedFrames : uint
    {
        AllFrames = 0,
        TopFrame = 1
    }

    internal enum WebKitUserScriptInjectionTime : uint
    {
        AtDocumentStart = 0,
        AtDocumentEnd = 1
    }

    public enum WebkitPolicyDecisionType : int
    {
        NavigationAction,
        NewWindowAction,
        Response,
    }

    public enum MenuAction : int
    {
        NoAction = 0,

        OpenLink,
        OpenLinkInNewWindow,
        DownloadLinkToDisk,
        CopyLinkToClipboard,
        OpenImageInNewWindow,
        DownloadImageToDisk,
        CopyImageToClipboard,
        CopyImageUrlToClipboard,
        OpenFrameInNewWindow,
        GoBack,
        GoForward,
        Stop,
        Reload,
        Copy,
        Cut,
        Paste,
        Delete,
        SelectAll,
        InputMethods,
        Unicode,
        SpellingGuess,
        NoGuessesFound,
        IgnoreSpelling,
        LearnSpelling,
        IgnoreGrammar,
        FontMenu,
        Bold,
        Italic,
        Underline,
        Outline,
        InspectElement,
        OpenVideoInNewWindow,
        OpenAudioInNewWindow,
        CopyVideoLinkToClipboard,
        CopyAudioLinkToClipboard,
        ToggleMediaControls,
        ToggleMediaLoop,
        EnterVideoFullscreen,
        MediaPlay,
        MediaPause,
        MediaMute,
        DownloadVideoToDisk,
        DownloadAudioToDisk,
        InsertEmoji,
        PasteAsPlainText,

        Custom = 10000
    }

    // Делегат для обработки сигнала destroy
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DestroyHandler(IntPtr widget, IntPtr data);

    internal static partial class WebView
    {
        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_web_view_new")]
        internal static partial IntPtr New();

        [LibraryImport(Libraries.WebKit,
            EntryPoint = "webkit_web_view_new_with_user_content_manager",
            StringMarshalling = StringMarshalling.Utf8)]
        internal static partial IntPtr NewWithUserContentManager(IntPtr contentManager);


        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_web_view_load_uri",
            StringMarshalling = StringMarshalling.Utf8)]
        internal static partial void LoadUri(IntPtr webView, string uri);

        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_web_view_get_uri")]
        internal static partial IntPtr GetUri(IntPtr webView);

        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_web_view_get_settings")]
        internal static partial IntPtr GetSettings(IntPtr webView);

        [LibraryImport(Libraries.WebKit,
            EntryPoint = "webkit_web_view_get_user_content_manager")]
        internal static partial IntPtr GetUserContentManager(IntPtr webView);
    }

    internal static partial class Navigation
    {
        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_navigation_policy_decision_get_navigation_action")]
        internal static partial IntPtr PolicyDecisionGetNavigationAction(IntPtr navDecision);

        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_navigation_action_get_request")]
        internal static partial IntPtr ActionGetRequest(IntPtr navAction);

        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_navigation_policy_decision_get_request")]
        internal static partial IntPtr PolicyDecisionGetRequest(IntPtr navDecision);
    }

    internal static partial class Settings
    {
        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_settings_set_enable_developer_extras")]
        internal static partial void SetEnableDeveloperExtras(IntPtr settings, GObject.GBoolean enabled);
    }

    internal static partial class ContextMenu
    {
        // Возвращает количество пунктов меню
        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_context_menu_get_n_items")]
        public static partial uint GetNItems(IntPtr menu);

        // Возвращает элемент меню по индексу
        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_context_menu_get_item_at_position")]
        public static partial IntPtr GetItemAtPosition(IntPtr menu, uint position);

        // Удаляет элемент из меню
        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_context_menu_remove")]
        public static partial void Remove(IntPtr menu, IntPtr item);

        // Получить «stock action» (типовой пункт, вроде Copy, Paste и т.п.)
        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_context_menu_item_get_stock_action")]
        public static partial MenuAction ItemGetStockAction(IntPtr item);
    }

    internal static partial class Javascript
    {
        [LibraryImport(Libraries.WebKit,
            EntryPoint = "webkit_javascript_result_get_value")]
        internal static partial IntPtr ResultGetValue(IntPtr jsResult);

        [LibraryImport(Libraries.WebKit,
            EntryPoint = "webkit_javascript_result_get_global_context")]
        internal static partial IntPtr GetGlobalContext(IntPtr jsResult);

        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_web_view_run_javascript", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial void RunJavaScript(
            IntPtr webView,
            string script,
            IntPtr cancellable,
            IntPtr callback,
            IntPtr userData);
    }

    internal static partial class UserContentManager
    {
        [LibraryImport(Libraries.WebKit,
            EntryPoint = "webkit_user_content_manager_add_script")]
        internal static partial void AddScript(
            IntPtr manager,
            IntPtr userScript);


        [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_user_content_manager_new")]
        internal static partial IntPtr UserContentManagerNew();

        [LibraryImport(Libraries.WebKit,
            EntryPoint = "webkit_user_content_manager_register_script_message_handler",
            StringMarshalling = StringMarshalling.Utf8)]
        internal static partial GObject.GBoolean UserContentManagerRegisterScriptMessageHandler(
            IntPtr manager,
            string name);

        [LibraryImport(Libraries.WebKit,
            EntryPoint = "webkit_web_view_new_with_user_content_manager",
            StringMarshalling = StringMarshalling.Utf8)]
        internal static partial GObject.GBoolean NewWithUserContentManager(IntPtr contentManager);
    }

    [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_uri_request_get_uri")]
    internal static partial IntPtr UriRequestGetUri(IntPtr uriRequest);

    [LibraryImport(Libraries.WebKit,
        EntryPoint = "webkit_user_script_new",
        StringMarshalling = StringMarshalling.Utf8)]
    internal static partial IntPtr UserScriptNew(
        string source,
        WebKitUserContentInjectedFrames injectedFrames,
        WebKitUserScriptInjectionTime injectionTime,
        IntPtr allowList,
        IntPtr blockList);

    [LibraryImport(Libraries.WebKit, EntryPoint = "webkit_policy_decision_ignore")]
    internal static partial void PolicyDecisionIgnore(IntPtr policyDecision);
}