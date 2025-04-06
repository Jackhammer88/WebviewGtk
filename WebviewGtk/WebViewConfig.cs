namespace WebviewGtk;

public record WebViewConfig
{
    public WebViewConfig(string startUri)
    {
        StartUri = new(startUri);
    }
    
    /// <summary>
    /// Стартовый адрес.
    /// </summary>
    public Uri StartUri { get; init; }
    
    /// <summary>
    /// Путь к картинке splash окна загрузки.
    /// </summary>
    public string? SplashFilename { get; init; }
    
    /// <summary>
    /// Ширина окна.
    /// </summary>
    public int Width { get; init; } = 800;
    
    /// <summary>
    /// Высота окна.
    /// </summary>
    public int Height { get; init; } = 600;
    
    
    /// <summary>
    /// Начальная позиция окна.
    /// </summary>
    public WindowPosition WindowPosition { get; init; } = WindowPosition.CenterAlways;
    
    /// <summary>
    /// Title окна webkit.
    /// </summary>
    public string WindowTitle { get; init; } = "";
    
    /// <summary>
    /// Строгий режим - разрешает переход только по указанным адресам в AllowedUrls.
    /// </summary>
    public bool StrictMode { get; init; }
    
    /// <summary>
    /// Режим отладки.
    /// </summary>
    public bool DebugMode { get; init; }
    
    /// <summary>
    /// Включает возможность выделения контента страниц.
    /// </summary>
    public bool AllowSelection { get; init; }

    /// <summary>
    /// Разрешённые адреса. Работает только при StrictMode == true.
    /// </summary>
    public IList<Uri> AllowedUrls { get; init; } = [];
}