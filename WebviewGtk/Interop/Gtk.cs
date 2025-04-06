using System.Runtime.InteropServices;
using WebviewGtk.Constants;

namespace WebviewGtk.Interop;

internal static partial class Gtk
{
    internal enum WindowType : int
    {
        GtkWindowToplevel = 0
    }
    
    internal enum GtkAlign : int
    {
        GtkAlignFill,
        GtkAlignStart,
        GtkAlignEnd,
        GtkAlignCenter,
        GtkAlignBaseline
    }

    public static class Events
    {
        public const string Destroy = "destroy";
        public const string Clicked = "clicked";
        public const string Changed = "changed";
    }

    internal static partial class Widget
    {
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_widget_set_halign")]
        internal static partial void SetHAlign(IntPtr widget, GtkAlign align);
    
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_widget_set_valign")]
        internal static partial void SetVAlign(IntPtr widget, GtkAlign align);
    
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_widget_hide")]
        internal static partial void Hide(IntPtr widget);
        
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_widget_show_all")]
        internal static partial void ShowAll(IntPtr widget);
    
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_widget_destroy")]
        internal static partial void Destroy(IntPtr widget);
        
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_widget_set_sensitive")]
        internal static partial void SetSensitive(IntPtr widget, GObject.GBoolean sensitive);
    }
    
    internal static partial class Entry
    {
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_entry_new")]
        internal static partial IntPtr New();
    
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_entry_set_max_length")]
        internal static partial void SetMaxLength(IntPtr entry, int max);
       
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_entry_get_text")]
        internal static partial IntPtr GetText(IntPtr entry);
    }

    internal static partial class Overlay
    {
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_overlay_new")]
        internal static partial IntPtr New();
        
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_overlay_add_overlay")]
        internal static partial void AddOverlay(IntPtr overlay, IntPtr widget);
    }

    internal static partial class Window
    {
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_window_new")]
        internal static partial IntPtr New(WindowType type);
    
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_window_set_title", StringMarshalling = StringMarshalling.Utf8)]
        internal static partial void SetTitle(IntPtr window, string title);
        
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_window_set_position")]
        internal static partial void SetPosition(IntPtr window, WindowPosition position);

        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_window_set_default_size")]
        internal static partial void SetDefaultSize(IntPtr window, int width, int height);
        
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_window_set_icon")]
        internal static partial void SetIcon(IntPtr window, IntPtr icon);
    }

    internal static partial class Image
    {

        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_image_new_from_animation")]
        internal static partial IntPtr NewFromAnimation(IntPtr animation);
    }

    internal static partial class Button
    {
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_button_new_with_label",
            StringMarshalling = StringMarshalling.Utf8)]
        internal static partial IntPtr NewWithLabel(string label);
    }

    internal static partial class Container
    {
        [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_container_add")]
        internal static partial void Add(IntPtr container, IntPtr widget);
    }
    
    [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_init", StringMarshalling = StringMarshalling.Utf8)]
    internal static partial void Init(ref int argc, ref IntPtr argv);

    [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_init_check")]
    internal static partial GObject.GBoolean InitCheck(ref int argc, ref IntPtr argv);

    [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_main")]
    internal static partial void Main();
    
    [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_main_iteration_do")]
    public static partial GObject.GBoolean MainIterationDo([MarshalAs(UnmanagedType.I1)] bool block);

    [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_events_pending")]
    public static partial GObject.GBoolean EventsPending();
    
    [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_main_quit")]
    internal static partial void MainQuit();
    
    
    
    [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_vbox_new")]
    internal static partial IntPtr VboxNew(GObject.GBoolean homogeneous, int spacing);
    
    [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_box_pack_start")]
    internal static partial void BoxPackStart(IntPtr box, IntPtr child, GObject.GBoolean expand, GObject.GBoolean fill, uint padding);
    
    [LibraryImport(Libraries.Gtk, EntryPoint = "gtk_hbox_new")]
    internal static partial IntPtr HBoxNew(GObject.GBoolean homogeneous, uint spacing);
}