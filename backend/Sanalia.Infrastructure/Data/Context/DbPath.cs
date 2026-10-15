using System;

namespace Sanalia.Infrastructure.Data.Context;

public class DbPath
{
    //  Decide dónde vive el archivo SQLite cuando no se configura una cadena de conexión.
    public const string FileName = "sanalia.db";

    public static string DefaultConnectionString() => $"Data Source={DefaultDatabaseFile()}";

    public static string DefaultDatabaseFile()
    {
        var baseDir = FindSolutionDirectory() ?? AppContext.BaseDirectory;
        var dataDir = Path.Combine(baseDir, "data");
        Directory.CreateDirectory(dataDir);
        return Path.Combine(dataDir, FileName);
    }

    private static string? FindSolutionDirectory()
    {
        // Se usa la ubicación de este ensamblado (no AppContext.BaseDirectory) porque cuando
        // corre "dotnet ef" el directorio base es el de la herramienta, no el del proyecto.
        var location = typeof(DbPath).Assembly.Location;
        if (string.IsNullOrEmpty(location))
            return null;

        for (var dir = new DirectoryInfo(Path.GetDirectoryName(location)!); dir != null; dir = dir.Parent)
        {
            if (dir.EnumerateFiles("*.sln").Any() || dir.EnumerateFiles("*.slnx").Any())
                return dir.FullName;
        }

        return null;
    }
}
