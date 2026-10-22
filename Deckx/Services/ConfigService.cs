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

    // Nombre del recurso: espacio raiz del proyecto mas el archivo
    private const string SemillaIncrustada = "Deckx.projects.json";

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

    // Semilla de la primera vez, en dos niveles
    private static WidgetConfig LoadSeed()
    {
        // 1. Un projects.json junto al ejecutable gana, para cambiarlo sin recompilar
        var seedPath = Path.Combine(AppContext.BaseDirectory, "projects.json");

        if (File.Exists(seedPath))
        {
            return Deserializar(File.ReadAllText(seedPath));
        }

        // 2. Si no hay, se usa la que viaja dentro del exe: asi se puede compartir suelto
        using var recurso = typeof(ConfigService).Assembly.GetManifestResourceStream(SemillaIncrustada);

        if (recurso is null)
        {
            return new WidgetConfig();
        }

        using var lector = new StreamReader(recurso);
        return Deserializar(lector.ReadToEnd());
    }

    private static WidgetConfig Deserializar(string json)
    {
        try
        {
            var config = JsonSerializer.Deserialize<WidgetConfig>(json) ?? new WidgetConfig();

            CompletarCategorias(config);

            return config;
        }
        catch
        {
            return new WidgetConfig();
        }
    }

    // Una configuracion vieja no trae la lista de categorias: se arma con las que usan los accesos
    private static void CompletarCategorias(WidgetConfig config)
    {
        var usadas = config.Items
            .Select(x => x.Group)
            .Where(x => !string.IsNullOrWhiteSpace(x));

        foreach (var grupo in usadas)
        {
            if (!config.Groups.Contains(grupo))
            {
                config.Groups.Add(grupo);
            }
        }
    }
}
