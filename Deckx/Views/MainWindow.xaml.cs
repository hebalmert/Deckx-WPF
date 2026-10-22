using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Deckx.Models;
using Deckx.Services;

namespace Deckx.Views;

public partial class MainWindow : Window
{
    private const double TileWidth = 116;
    private const double TileGap = 8;

    private readonly WidgetConfig _config;

    private string _activeGroup = "Todos";

    private EventWaitHandle? _senal;

    // Estado del arrastre para reordenar mosaicos
    private Point _dragStart;
    private ShortcutItem? _dragItem;
    private bool _wasDragging;

    public MainWindow()
    {
        InitializeComponent();

        _config = ConfigService.Load();

        ApplyConfig();
        RenderGroups();
        RenderTiles();
        EscucharSegundaApertura();
    }

    // Volver a abrir el acceso directo no lanza otra copia: trae esta a la pantalla de enfrente
    private void EscucharSegundaApertura()
    {
        _senal = new EventWaitHandle(false, EventResetMode.AutoReset, App.SenalMostrar);

        var hilo = new Thread(() =>
        {
            while (_senal.WaitOne())
            {
                Dispatcher.Invoke(() =>
                {
                    TraerAlCentro();
                    Show();
                    Activate();
                });
            }
        })
        {
            IsBackground = true
        };

        hilo.Start();
    }

    // Deja la ventana donde quedo la ultima vez y con el ancho de columnas elegido
    private void ApplyConfig()
    {
        Left = _config.Left;
        Top = _config.Top;
        Topmost = _config.Topmost;
        Opacity = _config.Opacity;

        // Cada mosaico ocupa su ancho mas el margen derecho, asi que el gap NO se resta
        PanelRoot.Width = _config.Columns * (TileWidth + TileGap);

        AsegurarQueSeVea();

        PinButton.Foreground = _config.Topmost
            ? (Brush)FindResource("TextBrush")
            : (Brush)FindResource("TextMutedBrush");

        BodyPanel.Visibility = _config.Collapsed ? Visibility.Collapsed : Visibility.Visible;
        CollapseButton.Content = _config.Collapsed ? "" : "";
    }

    // Chips: Todos mas las categorias creadas, y al final el boton para crear otra
    private void RenderGroups()
    {
        GroupsPanel.Children.Clear();

        var groups = _config.Groups
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .OrderBy(x => x)
            .ToList();

        groups.Insert(0, "Todos");

        foreach (var group in groups)
        {
            var chip = new Button
            {
                Style = (Style)FindResource("ChipButton"),
                Content = group,
                Tag = group
            };

            if (group == _activeGroup)
            {
                chip.Foreground = (Brush)FindResource("TextBrush");
                chip.Background = new SolidColorBrush(Color.FromRgb(0x24, 0x31, 0x56));
            }

            chip.Click += Chip_Click;

            // Todos no es una categoria de verdad, no se puede renombrar ni borrar
            if (group != "Todos")
            {
                chip.ContextMenu = BuildGroupMenu(group);
            }

            GroupsPanel.Children.Add(chip);
        }

        // Chip final para crear una categoria sin necesidad de tener accesos todavia
        var nueva = new Button
        {
            Style = (Style)FindResource("ChipButton"),
            Content = "+ Categoria",
            Foreground = (Brush)FindResource("TextMutedBrush"),
            ToolTip = "Crear una categoria"
        };

        nueva.Click += (_, _) => CrearCategoria();

        GroupsPanel.Children.Add(nueva);
    }

    private void CrearCategoria()
    {
        var dialog = new TextPromptWindow("Nueva categoria", "Nombre", string.Empty) { Owner = this };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var nombre = dialog.Valor;

        // Si ya existe simplemente se selecciona, no se duplica
        if (!_config.Groups.Contains(nombre))
        {
            _config.Groups.Add(nombre);
        }

        _activeGroup = nombre;

        SaveAndRefresh();
    }

    // Pinta los mosaicos filtrados mas el mosaico de agregar
    private void RenderTiles()
    {
        TilesPanel.Children.Clear();

        var filter = SearchBox.Text.Trim();

        var items = _config.Items
            .Where(x => _activeGroup == "Todos" || x.Group == _activeGroup)
            .Where(x => filter.Length == 0 || x.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name)
            .ToList();

        foreach (var item in items)
        {
            TilesPanel.Children.Add(BuildTile(item));
        }

        var addTile = new Button
        {
            Style = (Style)FindResource("AddTileButton"),
            ToolTip = "Agregar un acceso",
            AllowDrop = true
        };

        addTile.Click += AddTile_Click;

        // Soltar sobre el mosaico de agregar manda el acceso al final
        addTile.DragOver += AddTile_DragOver;
        addTile.DragLeave += AddTile_DragLeave;
        addTile.Drop += AddTile_Drop;

        TilesPanel.Children.Add(addTile);
    }

    private Button BuildTile(ShortcutItem item)
    {
        var icon = new TextBlock
        {
            Text = item.IconGlyph,
            FontFamily = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets"),
            FontSize = 18,
            Foreground = Brushes.White
        };

        var name = new TextBlock
        {
            Text = item.Name,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        var subtitle = new TextBlock
        {
            Text = item.Subtitle,
            FontSize = 11,
            Opacity = 0.85,
            Foreground = Brushes.White,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        var bottom = new StackPanel { VerticalAlignment = VerticalAlignment.Bottom };
        bottom.Children.Add(name);
        bottom.Children.Add(subtitle);

        var layout = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(icon, Dock.Top);
        layout.Children.Add(icon);
        layout.Children.Add(bottom);

        var tile = new Button
        {
            Style = (Style)FindResource("TileButton"),
            Background = ToBrush(item.Color),
            Content = layout,
            Tag = item,
            ToolTip = string.IsNullOrWhiteSpace(item.Path) ? item.Name : item.Path,
            ContextMenu = BuildMenu(item),
            AllowDrop = true,
            BorderThickness = new Thickness(0)
        };

        tile.Click += Tile_Click;

        // Arrastrar el mosaico para reordenarlo
        tile.PreviewMouseLeftButtonDown += Tile_PreviewMouseLeftButtonDown;
        tile.PreviewMouseMove += Tile_PreviewMouseMove;
        tile.DragOver += Tile_DragOver;
        tile.DragLeave += Tile_DragLeave;
        tile.Drop += Tile_Drop;

        return tile;
    }

    // Guarda donde empezo el clic para saber si es clic o arrastre
    private void Tile_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStart = e.GetPosition(this);
        _wasDragging = false;

        if (sender is Button { Tag: ShortcutItem item })
        {
            _dragItem = item;
        }
    }

    // Pasado el umbral del sistema arranca el arrastre
    private void Tile_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _dragItem is null)
        {
            return;
        }

        var current = e.GetPosition(this);

        var moved = Math.Abs(current.X - _dragStart.X) > SystemParameters.MinimumHorizontalDragDistance
                 || Math.Abs(current.Y - _dragStart.Y) > SystemParameters.MinimumVerticalDragDistance;

        if (!moved || sender is not Button tile)
        {
            return;
        }

        _wasDragging = true;
        tile.Opacity = 0.5;

        DragDrop.DoDragDrop(tile, _dragItem, DragDropEffects.Move);

        tile.Opacity = 1;
    }

    // Marca el mosaico sobre el que se va a soltar
    private void Tile_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = _dragItem is null ? DragDropEffects.None : DragDropEffects.Move;
        e.Handled = true;

        if (sender is Button tile && _dragItem is not null)
        {
            tile.BorderBrush = Brushes.White;
            tile.BorderThickness = new Thickness(2);
        }
    }

    private void Tile_DragLeave(object sender, DragEventArgs e)
    {
        if (sender is Button tile)
        {
            tile.BorderThickness = new Thickness(0);
        }
    }

    private void Tile_Drop(object sender, DragEventArgs e)
    {
        if (sender is Button tile)
        {
            tile.BorderThickness = new Thickness(0);
        }

        if (sender is not Button { Tag: ShortcutItem target } || _dragItem is null || _dragItem == target)
        {
            return;
        }

        MoveItem(_dragItem, _config.Items.OrderBy(x => x.Order).ToList().IndexOf(target));
    }

    // El mosaico de agregar lleva borde propio, asi que se resalta con opacidad
    private void AddTile_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = _dragItem is null ? DragDropEffects.None : DragDropEffects.Move;
        e.Handled = true;

        if (sender is Button tile && _dragItem is not null)
        {
            tile.Opacity = 0.6;
        }
    }

    private void AddTile_DragLeave(object sender, DragEventArgs e)
    {
        if (sender is Button tile)
        {
            tile.Opacity = 1;
        }
    }

    private void AddTile_Drop(object sender, DragEventArgs e)
    {
        if (sender is Button tile)
        {
            tile.Opacity = 1;
        }

        if (_dragItem is null)
        {
            return;
        }

        MoveItem(_dragItem, _config.Items.Count - 1);
    }

    // Reordena sobre la lista completa y renumera para que el orden quede firme
    private void MoveItem(ShortcutItem item, int targetIndex)
    {
        var ordered = _config.Items.OrderBy(x => x.Order).ToList();

        ordered.Remove(item);
        ordered.Insert(Math.Clamp(targetIndex, 0, ordered.Count), item);

        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].Order = i;
        }

        _dragItem = null;

        SaveAndRefresh();
    }

    // Menu de una categoria: se puede renombrar o quitar
    private ContextMenu BuildGroupMenu(string group)
    {
        var menu = new ContextMenu();

        menu.Items.Add(BuildMenuItem("Renombrar categoria", () => RenombrarCategoria(group)));
        menu.Items.Add(BuildMenuItem("Quitar categoria", () => QuitarCategoria(group)));
        menu.Items.Add(new Separator());
        menu.Items.Add(BuildMenuItem("Eliminar categoria y sus accesos", () => EliminarCategoriaConAccesos(group)));

        return menu;
    }

    private void RenombrarCategoria(string group)
    {
        var dialog = new TextPromptWindow("Renombrar categoria", "Nuevo nombre", group) { Owner = this };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        var nuevo = dialog.Valor;

        foreach (var item in _config.Items.Where(x => x.Group == group))
        {
            item.Group = nuevo;
        }

        _config.Groups.Remove(group);

        if (!_config.Groups.Contains(nuevo))
        {
            _config.Groups.Add(nuevo);
        }

        if (_activeGroup == group)
        {
            _activeGroup = nuevo;
        }

        SaveAndRefresh();
    }

    // Quita la categoria pero conserva los accesos, que pasan a General
    private void QuitarCategoria(string group)
    {
        var cuantos = _config.Items.Count(x => x.Group == group);

        var confirm = MessageBox.Show(
            $"Quitar la categoria \"{group}\"?\n\nSus {cuantos} accesos pasan a General.",
            "Deckx", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        foreach (var item in _config.Items.Where(x => x.Group == group))
        {
            item.Group = "General";
        }

        _config.Groups.Remove(group);

        // General tiene que existir para recibirlos
        if (cuantos > 0 && !_config.Groups.Contains("General"))
        {
            _config.Groups.Add("General");
        }

        _activeGroup = "Todos";

        SaveAndRefresh();
    }

    // Borra la categoria con todo lo que tiene dentro
    private void EliminarCategoriaConAccesos(string group)
    {
        var cuantos = _config.Items.Count(x => x.Group == group);

        var confirm = MessageBox.Show(
            $"Eliminar la categoria \"{group}\" y sus {cuantos} accesos?\n\nEsto no se puede deshacer.",
            "Deckx", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        _config.Items.RemoveAll(x => x.Group == group);
        _config.Groups.Remove(group);

        _activeGroup = "Todos";

        SaveAndRefresh();
    }

    private ContextMenu BuildMenu(ShortcutItem item)
    {
        var menu = new ContextMenu();

        menu.Items.Add(BuildMenuItem("Abrir", () => LaunchService.Open(item.Path, item.Arguments)));
        menu.Items.Add(BuildMenuItem("Abrir carpeta", () => LaunchService.OpenFolder(item.Path)));
        menu.Items.Add(BuildMenuItem("Abrir en VS Code", () => LaunchService.OpenInVsCode(item.Path)));
        menu.Items.Add(BuildMenuItem("Abrir terminal", () => LaunchService.OpenTerminal(item.Path)));
        menu.Items.Add(new Separator());
        menu.Items.Add(BuildMenuItem("Editar", () => EditItem(item)));
        menu.Items.Add(BuildMenuItem("Eliminar", () => DeleteItem(item)));

        return menu;
    }

    private static MenuItem BuildMenuItem(string header, Action action)
    {
        var menuItem = new MenuItem { Header = header };
        menuItem.Click += (_, _) => action();

        return menuItem;
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

    // Abre el acceso y avisa en vez de reventar si la ruta ya no existe
    private void Tile_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ShortcutItem item })
        {
            return;
        }

        // Si el mosaico se venia arrastrando no se abre nada
        if (_wasDragging)
        {
            _wasDragging = false;
            return;
        }

        try
        {
            LaunchService.Open(item.Path, item.Arguments);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo abrir:\n{item.Path}\n\n{ex.Message}", "Deckx",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AddTile_Click(object sender, RoutedEventArgs e)
    {
        // El acceso nace en la categoria que se este viendo
        var item = new ShortcutItem
        {
            Order = _config.Items.Count,
            Group = _activeGroup == "Todos" ? string.Empty : _activeGroup
        };

        var dialog = new EditItemWindow(item, _config.Groups) { Owner = this };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _config.Items.Add(item);
        SaveAndRefresh();
    }

    private void EditItem(ShortcutItem item)
    {
        var dialog = new EditItemWindow(item, _config.Groups) { Owner = this };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        SaveAndRefresh();
    }

    private void DeleteItem(ShortcutItem item)
    {
        var confirm = MessageBox.Show($"Eliminar \"{item.Name}\"?", "Deckx",
            MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        _config.Items.Remove(item);
        SaveAndRefresh();
    }

    private void SaveAndRefresh()
    {
        // Ningun acceso puede quedar en una categoria que no existe, o no se veria su chip
        foreach (var item in _config.Items)
        {
            if (string.IsNullOrWhiteSpace(item.Group))
            {
                item.Group = "General";
            }

            if (!_config.Groups.Contains(item.Group))
            {
                _config.Groups.Add(item.Group);
            }
        }

        ConfigService.Save(_config);
        RenderGroups();
        RenderTiles();
    }

    private void Chip_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string group })
        {
            return;
        }

        _activeGroup = group;

        RenderGroups();
        RenderTiles();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RenderTiles();
    }

    private void PinButton_Click(object sender, RoutedEventArgs e)
    {
        _config.Topmost = !_config.Topmost;
        Topmost = _config.Topmost;

        PinButton.Foreground = _config.Topmost
            ? (Brush)FindResource("TextBrush")
            : (Brush)FindResource("TextMutedBrush");

        ConfigService.Save(_config);
    }

    private void CollapseButton_Click(object sender, RoutedEventArgs e)
    {
        _config.Collapsed = !_config.Collapsed;

        BodyPanel.Visibility = _config.Collapsed ? Visibility.Collapsed : Visibility.Visible;
        CollapseButton.Content = _config.Collapsed ? "" : "";

        ConfigService.Save(_config);
    }

    // Menu de opciones: columnas, transparencia y carpeta de configuracion
    private void OptionsButton_Click(object sender, RoutedEventArgs e)
    {
        var menu = new ContextMenu();

        var columns = new MenuItem { Header = "Columnas" };

        foreach (var count in new[] { 3, 4, 5, 6 })
        {
            var option = new MenuItem { Header = count.ToString(), IsChecked = _config.Columns == count };
            option.Click += (_, _) => ChangeColumns(count);
            columns.Items.Add(option);
        }

        var opacity = new MenuItem { Header = "Transparencia" };

        foreach (var value in new[] { 1.00, 0.96, 0.88, 0.75 })
        {
            var label = $"{value * 100:0}%";
            var option = new MenuItem { Header = label, IsChecked = Math.Abs(_config.Opacity - value) < 0.01 };
            option.Click += (_, _) => ChangeOpacity(value);
            opacity.Items.Add(option);
        }

        menu.Items.Add(columns);
        menu.Items.Add(opacity);
        menu.Items.Add(new Separator());
        menu.Items.Add(BuildMenuItem("Traer al centro", TraerAlCentro));
        menu.Items.Add(new Separator());

        // Instalar solo tiene sentido cuando el exe corre desde otra carpeta
        if (InstallService.YaEstaInstalado())
        {
            menu.Items.Add(BuildMenuItem("Quitar de este PC", QuitarDeEstePc));
        }
        else
        {
            menu.Items.Add(BuildMenuItem("Instalar en este PC", InstalarEnEstePc));
        }

        menu.Items.Add(BuildMenuItem("Abrir carpeta de configuracion", () => LaunchService.OpenFolder(ConfigService.ConfigFolder)));

        menu.PlacementTarget = (UIElement)sender;
        menu.IsOpen = true;
    }

    // Se copia al arranque de Windows y deja el acceso directo en el escritorio
    private void InstalarEnEstePc()
    {
        var confirm = MessageBox.Show(
            "Deckx quedara en el arranque de Windows, con su acceso directo en el escritorio.\n\n" +
            "Tus accesos no se tocan: viven aparte en tu carpeta de usuario.",
            "Instalar Deckx", MessageBoxButton.OKCancel, MessageBoxImage.Information);

        if (confirm != MessageBoxResult.OK)
        {
            return;
        }

        try
        {
            InstallService.Instalar();

            MessageBox.Show(
                "Listo. Deckx arrancara con Windows.\n\nEsta copia la puedes borrar.",
                "Deckx", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo instalar:\n\n{ex.Message}", "Deckx",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void QuitarDeEstePc()
    {
        var confirm = MessageBox.Show(
            "Deckx dejara de arrancar con Windows y se cerrara.\n\nTus accesos se conservan.",
            "Quitar Deckx", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            // El exe no se puede borrar mientras corre: lo borra un comando que espera a que cierre
            var comando = $"/c timeout /t 2 /nobreak >nul & del \"{InstallService.RutaEnArranque}\"";

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd.exe", comando)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            });

            InstallService.Desinstalar();
        }
        catch
        {
            // Si falla el borrado del acceso directo no vale la pena molestar
        }

        Close();
    }

    private void ChangeColumns(int count)
    {
        _config.Columns = count;
        PanelRoot.Width = count * (TileWidth + TileGap);

        ConfigService.Save(_config);
    }

    // Si la posicion guardada quedo fuera de las pantallas conectadas, vuelve a la principal
    private void AsegurarQueSeVea()
    {
        var dentro = Left >= SystemParameters.VirtualScreenLeft
                  && Top >= SystemParameters.VirtualScreenTop
                  && Left < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 80
                  && Top < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 80;

        if (dentro)
        {
            return;
        }

        TraerAlCentro();
    }

    // Deja el widget centrado en la pantalla donde esta el raton, que es donde el usuario mira
    private void TraerAlCentro()
    {
        var (areaLeft, areaTop, areaAncho, areaAlto) = ScreenService.AreaDondeEstaElRaton();

        // Al arrancar la ventana todavia no se ha medido: se usa el ancho configurado
        var ancho = ActualWidth > 0 ? ActualWidth : PanelRoot.Width + 30;
        var alto = ActualHeight > 0 ? ActualHeight : 400;

        Left = areaLeft + ((areaAncho - ancho) / 2);
        Top = areaTop + ((areaAlto - alto) / 2);

        _config.Left = Left;
        _config.Top = Top;

        ConfigService.Save(_config);
    }

    private void ChangeOpacity(double value)
    {
        _config.Opacity = value;
        Opacity = value;

        ConfigService.Save(_config);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // Arrastrar el widget a cualquier parte de la pantalla
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        try
        {
            DragMove();

            _config.Left = Left;
            _config.Top = Top;

            ConfigService.Save(_config);
        }
        catch
        {
            // DragMove revienta si el boton ya se solto; no pasa nada
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _config.Left = Left;
        _config.Top = Top;

        ConfigService.Save(_config);

        base.OnClosing(e);
    }
}
