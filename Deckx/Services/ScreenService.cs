using System.Runtime.InteropServices;

namespace Deckx.Services;

// Donde estan las pantallas de verdad. Con varios monitores las coordenadas pueden ser
// negativas, asi que no sirve suponer que la principal empieza en 0,0
public static class ScreenService
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Punto
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rectangulo
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct InfoMonitor
    {
        public int Size;
        public Rectangulo Monitor;
        public Rectangulo Trabajo;
        public uint Flags;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Punto punto);

    [DllImport("user32.dll")]
    private static extern nint MonitorFromPoint(Punto punto, uint opcion);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool GetMonitorInfoW(nint monitor, ref InfoMonitor info);

    private const uint MasCercano = 2;

    // Area util (sin barra de tareas) del monitor donde esta el raton
    public static (double Left, double Top, double Width, double Height) AreaDondeEstaElRaton()
    {
        if (!GetCursorPos(out var cursor))
        {
            return Pantalla();
        }

        var monitor = MonitorFromPoint(cursor, MasCercano);

        var info = new InfoMonitor();
        info.Size = Marshal.SizeOf<InfoMonitor>();

        if (!GetMonitorInfoW(monitor, ref info))
        {
            return Pantalla();
        }

        var area = info.Trabajo;

        return (area.Left, area.Top, area.Right - area.Left, area.Bottom - area.Top);
    }

    // Respaldo: el escritorio completo, por si las llamadas al sistema fallan
    private static (double, double, double, double) Pantalla()
    {
        return (System.Windows.SystemParameters.VirtualScreenLeft,
                System.Windows.SystemParameters.VirtualScreenTop,
                System.Windows.SystemParameters.VirtualScreenWidth,
                System.Windows.SystemParameters.VirtualScreenHeight);
    }
}
