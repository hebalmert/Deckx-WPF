using System.IO;

namespace Deckx.Services;

// Deckx se instala a si mismo: se copia al arranque de Windows y deja su acceso directo
public static class InstallService
{
    public static string RutaEnArranque
    {
        get
        {
            var arranque = Environment.GetFolderPath(Environment.SpecialFolder.Startup);

            return Path.Combine(arranque, "Deckx.exe");
        }
    }

    // Verdadero cuando el exe que corre ES el que esta en la carpeta de arranque
    public static bool YaEstaInstalado()
    {
        var actual = Environment.ProcessPath;

        if (string.IsNullOrEmpty(actual))
        {
            return false;
        }

        return string.Equals(actual, RutaEnArranque, StringComparison.OrdinalIgnoreCase);
    }

    public static void Instalar()
    {
        var actual = Environment.ProcessPath
            ?? throw new InvalidOperationException("No se pudo ubicar el ejecutable.");

        File.Copy(actual, RutaEnArranque, true);

        CrearAccesoDirecto();
    }

    // Cada borrado va por su cuenta: el exe puede estar en uso y aun asi hay que
    // quitar el acceso directo
    public static void Desinstalar()
    {
        Borrar(RutaEnArranque);
        Borrar(RutaAccesoDirecto());
    }

    private static void Borrar(string ruta)
    {
        try
        {
            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }
        catch
        {
            // En uso o sin permisos: lo remata el comando diferido
        }
    }

    private static string RutaAccesoDirecto()
    {
        var escritorio = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        return Path.Combine(escritorio, "Deckx.lnk");
    }

    // El acceso directo se arma con el shell de Windows, sin librerias de por medio
    private static void CrearAccesoDirecto()
    {
        try
        {
            var tipo = Type.GetTypeFromProgID("WScript.Shell");

            if (tipo is null)
            {
                return;
            }

            dynamic? shell = Activator.CreateInstance(tipo);

            if (shell is null)
            {
                return;
            }

            dynamic acceso = shell.CreateShortcut(RutaAccesoDirecto());

            acceso.TargetPath = RutaEnArranque;
            acceso.WorkingDirectory = Path.GetDirectoryName(RutaEnArranque);
            acceso.IconLocation = RutaEnArranque;
            acceso.Description = "Deckx, todos tus accesos a un solo clic";
            acceso.Save();
        }
        catch
        {
            // Sin acceso directo tampoco pasa nada: ya quedo en el arranque
        }
    }
}
