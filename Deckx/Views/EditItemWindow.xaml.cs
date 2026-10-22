using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Deckx.Models;
using Deckx.Services;

namespace Deckx.Views;

public partial class EditItemWindow : Window
{
    private readonly ShortcutItem _item;

    private string _glyph;
    private string _color;

    public EditItemWindow(ShortcutItem item)
    {
        InitializeComponent();

        _item = item;
        _glyph = item.IconGlyph;
        _color = item.Color;

        LoadItem();
        RenderIcons();
        RenderColors();
        RefreshPreview();
    }

    // Trae a la pantalla lo que ya tenga el acceso
    private void LoadItem()
    {
        TitleText.Text = string.IsNullOrWhiteSpace(_item.Name) ? "Nuevo acceso" : "Editar acceso";

        NameBox.Text = _item.Name;
        SubtitleBox.Text = _item.Subtitle;
        GroupBox.Text = string.IsNullOrWhiteSpace(_item.Group) ? "General" : _item.Group;
        PathBox.Text = _item.Path;
        ArgumentsBox.Text = _item.Arguments;

        NameBox.TextChanged += (_, _) => RefreshPreview();
        SubtitleBox.TextChanged += (_, _) => RefreshPreview();
    }

    // Paleta de iconos: un cuadrito por glifo
    private void RenderIcons()
    {
        IconsPanel.Children.Clear();

        foreach (var icon in CatalogService.Icons)
        {
            var button = new Button
            {
                Width = 34,
                Height = 34,
                Margin = new Thickness(0, 0, 6, 6),
                Content = icon.Glyph,
                FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
                FontSize = 15,
                ToolTip = icon.Name,
                Tag = icon.Glyph,
                Style = (Style)FindResource("HeaderButton"),
                Foreground = icon.Glyph == _glyph
                    ? (Brush)FindResource("TextBrush")
                    : (Brush)FindResource("TextMutedBrush")
            };

            button.Click += IconButton_Click;

            IconsPanel.Children.Add(button);
        }
    }

    // Paleta de colores: un circulo por color
    private void RenderColors()
    {
        ColorsPanel.Children.Clear();

        foreach (var color in CatalogService.Colors)
        {
            var dot = new Border
            {
                Width = 26,
                Height = 26,
                CornerRadius = new CornerRadius(13),
                Margin = new Thickness(0, 0, 8, 0),
                Background = ToBrush(color.Hex),
                Cursor = Cursors.Hand,
                ToolTip = color.Name,
                Tag = color.Hex,
                BorderThickness = new Thickness(color.Hex == _color ? 2 : 0),
                BorderBrush = Brushes.White
            };

            dot.MouseLeftButtonDown += ColorDot_Click;

            ColorsPanel.Children.Add(dot);
        }
    }

    private void RefreshPreview()
    {
        PreviewIcon.Text = _glyph;
        PreviewTile.Background = ToBrush(_color);
        PreviewName.Text = string.IsNullOrWhiteSpace(NameBox.Text) ? "Nombre" : NameBox.Text;
        PreviewSubtitle.Text = SubtitleBox.Text;
    }

    private void IconButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string glyph })
        {
            return;
        }

        _glyph = glyph;

        RenderIcons();
        RefreshPreview();
    }

    private void ColorDot_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border { Tag: string hex })
        {
            return;
        }

        _color = hex;

        RenderColors();
        RefreshPreview();
    }

    // Cualquier archivo del PC: solucion, exe, acceso directo, documento
    private void BrowseFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Selecciona el archivo a abrir",
            Filter = "Todos los archivos|*.*|Soluciones|*.sln;*.slnx|Programas|*.exe;*.lnk;*.bat;*.cmd"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        PathBox.Text = dialog.FileName;

        // Si aun no hay nombre se propone el del archivo
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            NameBox.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
        }
    }

    private void BrowseFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Selecciona la carpeta a abrir" };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        PathBox.Text = dialog.FolderName;

        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            NameBox.Text = new DirectoryInfo(dialog.FolderName).Name;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Validacion minima: sin nombre y sin ruta el mosaico no sirve
        if (string.IsNullOrWhiteSpace(NameBox.Text))
        {
            MessageBox.Show("Ponle un nombre al acceso.", "Deckx",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (string.IsNullOrWhiteSpace(PathBox.Text))
        {
            MessageBox.Show("Falta la ruta o el programa que debe abrir.", "Deckx",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _item.Name = NameBox.Text.Trim();
        _item.Subtitle = SubtitleBox.Text.Trim();
        _item.Group = string.IsNullOrWhiteSpace(GroupBox.Text) ? "General" : GroupBox.Text.Trim();
        _item.Path = PathBox.Text.Trim();
        _item.Arguments = ArgumentsBox.Text.Trim();
        _item.IconGlyph = _glyph;
        _item.Color = _color;

        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    private static Brush ToBrush(string hex)
    {
        try
        {
            return (Brush)new BrushConverter().ConvertFromString(hex)!;
        }
        catch
        {
            return new SolidColorBrush(Color.FromRgb(0x37, 0x8A, 0xDD));
        }
    }

    // El dialogo tambien se arrastra porque no tiene barra de titulo
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        try
        {
            DragMove();
        }
        catch
        {
            // DragMove revienta si el boton ya se solto
        }
    }
}
