using System.Windows;
using System.Windows.Input;

namespace Deckx.Views;

// Pide un texto corto: se usa para renombrar categorias
public partial class TextPromptWindow : Window
{
    public string Valor { get; private set; } = string.Empty;

    public TextPromptWindow(string titulo, string etiqueta, string valorInicial)
    {
        InitializeComponent();

        TitleText.Text = titulo;
        LabelText.Text = etiqueta;
        ValueBox.Text = valorInicial;

        Loaded += (_, _) =>
        {
            ValueBox.Focus();
            ValueBox.SelectAll();
        };
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var texto = ValueBox.Text.Trim();

        // Sin nombre no hay categoria
        if (texto.Length == 0)
        {
            ErrorText.Text = "Escribe un nombre";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        Valor = texto;
        DialogResult = true;
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }

    // Enter guarda, Escape cancela
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter)
        {
            SaveButton_Click(this, new RoutedEventArgs());
        }

        if (e.Key == Key.Escape)
        {
            DialogResult = false;
        }
    }

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
