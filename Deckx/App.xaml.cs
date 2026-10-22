using System.Threading;
using System.Windows;

namespace Deckx;

public partial class App : Application
{
    public const string SenalMostrar = "Deckx.Mostrar";

    private const string NombreInstancia = "Deckx.InstanciaUnica";

    private Mutex? _instancia;

    protected override void OnStartup(StartupEventArgs e)
    {
        _instancia = new Mutex(true, NombreInstancia, out var esLaPrimera);

        // Si ya hay un Deckx abierto, se le avisa para que se muestre y esta copia se va
        if (!esLaPrimera)
        {
            if (EventWaitHandle.TryOpenExisting(SenalMostrar, out var senal))
            {
                senal.Set();
                senal.Dispose();
            }

            Shutdown();
            return;
        }

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _instancia?.ReleaseMutex();
        _instancia?.Dispose();

        base.OnExit(e);
    }
}
