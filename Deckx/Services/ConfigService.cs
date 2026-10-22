using System.IO;
using System.Text.Json;
using Deckx.Models;

namespace Deckx.Services;

// Carga y guarda el widget.json en AppData para que no se pierda al recompilar
public static class ConfigService
{
    private static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Deckx");

    private static readonly string FilePath = Path.Combine(Folder, "widget.json");

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string ConfigFolder => Folder;

    public static WidgetConfig Load()
    {
        try
        {
            // Primera vez: se copia la semilla que viene junto al ejecutable
            if (!File.Exists(FilePath))
            {
                return LoadSeed();
            }

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<WidgetConfig>(json) ?? new WidgetConfig();
        }
        catch
        {
            return new WidgetConfig();
        }
    }

    public static void Save(WidgetConfig config)
    {
        try
        {
            Directory.CreateDirectory(Folder);

            var json = JsonSerializer.Serialize(config, Options);
            File.WriteAllText(FilePath, json);
        }
        catch
        {
            // Si no se puede guardar no se tumba el widget
        }
    }

    private static WidgetConfig LoadSeed()
    {
        var seedPath = Path.Combine(AppContext.BaseDirectory, "projects.json");

        if (!File.Exists(seedPath))
        {
            return new WidgetConfig();
        }

        var json = File.ReadAllText(seedPath);
        return JsonSerializer.Deserialize<WidgetConfig>(json) ?? new WidgetConfig();
    }
}
