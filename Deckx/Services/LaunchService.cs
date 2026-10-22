using System.Diagnostics;
using System.IO;

namespace Deckx.Services;

// Todo lo que el widget sabe abrir en el PC
public static class LaunchService
{
    public static void Open(string path, string arguments = "")
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        var info = new ProcessStartInfo
        {
            FileName = Environment.ExpandEnvironmentVariables(path),
            Arguments = arguments,
            UseShellExecute = true
        };

        Process.Start(info);
    }

    public static void OpenFolder(string path)
    {
        var folder = ResolveFolder(path);

        if (folder is null)
        {
            return;
        }

        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{folder}\"") { UseShellExecute = true });
    }

    public static void OpenInVsCode(string path)
    {
        var folder = ResolveFolder(path);

        if (folder is null)
        {
            return;
        }

        Process.Start(new ProcessStartInfo("cmd.exe", $"/c code \"{folder}\"")
        {
            UseShellExecute = false,
            CreateNoWindow = true
        });
    }

    public static void OpenTerminal(string path)
    {
        var folder = ResolveFolder(path);

        if (folder is null)
        {
            return;
        }

        // Windows Terminal si existe; si no, PowerShell clasico
        try
        {
            Process.Start(new ProcessStartInfo("wt.exe", $"-d \"{folder}\"") { UseShellExecute = true });
        }
        catch
        {
            Process.Start(new ProcessStartInfo("powershell.exe")
            {
                WorkingDirectory = folder,
                UseShellExecute = true
            });
        }
    }

    // De una ruta cualquiera saca la carpeta que se puede abrir
    private static string? ResolveFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var full = Environment.ExpandEnvironmentVariables(path);

        if (Directory.Exists(full))
        {
            return full;
        }

        if (File.Exists(full))
        {
            return Path.GetDirectoryName(full);
        }

        return null;
    }
}
