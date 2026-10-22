namespace Deckx.Models;

// Estado del widget: donde quedo, como se ve y que accesos tiene
public class WidgetConfig
{
    public double Left { get; set; } = 80;

    public double Top { get; set; } = 80;

    public int Columns { get; set; } = 4;

    public bool Topmost { get; set; } = true;

    public bool Collapsed { get; set; }

    public double Opacity { get; set; } = 0.96;

    // Las categorias existen por si mismas: se pueden crear antes de tener accesos dentro
    public List<string> Groups { get; set; } = new();

    public List<ShortcutItem> Items { get; set; } = new();
}
