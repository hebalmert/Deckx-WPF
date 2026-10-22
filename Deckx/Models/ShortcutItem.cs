namespace Deckx.Models;

// Un acceso del widget: puede ser una solucion, un programa, una carpeta o una pagina
public class ShortcutItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    // Ruta a ejecutar: .slnx, .exe, carpeta o url
    public string Path { get; set; } = string.Empty;

    public string Arguments { get; set; } = string.Empty;

    // Glifo de la fuente Segoe Fluent Icons (viene con Windows)
    public string IconGlyph { get; set; } = "\uE700";

    // Color del mosaico en formato #RRGGBB
    public string Color { get; set; } = "#378ADD";

    public string Group { get; set; } = "General";

    public int Order { get; set; }
}
