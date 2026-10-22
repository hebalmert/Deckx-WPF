namespace Deckx.Services;

// Listas fijas que alimentan los combos del dialogo
public static class CatalogService
{
    // Glifos de Segoe Fluent Icons con su nombre en espanol
    public static List<IconOption> Icons { get; } = new()
    {
        new IconOption("Codigo",     "\uE943"),
        new IconOption("Carpeta",    "\uE8B7"),
        new IconOption("Programa",   "\uE756"),
        new IconOption("Web",        "\uE774"),
        new IconOption("Base datos", "\uE968"),
        new IconOption("Nube",       "\uE753"),
        new IconOption("Red",        "\uE968"),
        new IconOption("Telefono",   "\uE717"),
        new IconOption("Correo",     "\uE715"),
        new IconOption("Calendario", "\uE787"),
        new IconOption("Documento",  "\uE8A5"),
        new IconOption("Reporte",    "\uE9D9"),
        new IconOption("Tienda",     "\uE719"),
        new IconOption("Auto",       "\uE804"),
        new IconOption("Casa",       "\uE80F"),
        new IconOption("Salud",      "\uE95E"),
        new IconOption("Herramienta","\uE90F"),
        new IconOption("Persona",    "\uE77B"),
        new IconOption("Candado",    "\uE72E"),
        new IconOption("Favorito",   "\uE735")
    };

    // Paleta del widget
    public static List<ColorOption> Colors { get; } = new()
    {
        new ColorOption("Azul",     "#378ADD"),
        new ColorOption("Verde",    "#1D9E75"),
        new ColorOption("Morado",   "#7F77DD"),
        new ColorOption("Coral",    "#D85A30"),
        new ColorOption("Rosa",     "#D4537E"),
        new ColorOption("Ambar",    "#BA7517"),
        new ColorOption("Oliva",    "#639922"),
        new ColorOption("Rojo",     "#E24B4A"),
        new ColorOption("Gris",     "#5F5E5A"),
        new ColorOption("Turquesa", "#0F8FA5")
    };
}

public record IconOption(string Name, string Glyph);

public record ColorOption(string Name, string Hex);
