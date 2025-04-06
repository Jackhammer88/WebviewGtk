
using WebviewGtk;

CancellationTokenSource cts = new();

Console.CancelKeyPress += (_, _) => cts.Cancel();

WebViewConfig viewConfig = new("https://github.com/")
{
    DebugMode = true,
    Height = 800,
    Width = 600,
    AllowSelection = false,
    WindowTitle =  "WebviewGtk Sample"
};

WebkitGtkWrapper.RunWebkit(viewConfig, (navigationEvent, address) =>
{
    Console.WriteLine($"Got navigation event: {navigationEvent}: {address}.");
}, cts.Token);