using System.Diagnostics;
using System.Runtime.Versioning;

namespace BCR.Library;
public class FileWatcher
{
    [SupportedOSPlatform("windows")]
    public static void Watch()
    {
        //using var watcher = new FileSystemWatcher("\\\\ARCH-FRIGATE\\Users\\tgioa\\Desktop\\Separation");
        //using var watcher = new FileSystemWatcher("../../../../BCR.Library/Data");
        using var watcher = new FileSystemWatcher("\\\\ARCH-FRIGATE\\Scans\\BCR Test"); 

        watcher.NotifyFilter = NotifyFilters.Attributes
                             | NotifyFilters.CreationTime
                             | NotifyFilters.DirectoryName
                             | NotifyFilters.FileName
                             | NotifyFilters.LastAccess
                             | NotifyFilters.LastWrite
                             | NotifyFilters.Security
                             | NotifyFilters.Size;

        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.Renamed += OnRenamed;
        watcher.Error += OnError;

        watcher.Filter = "*.pdf";
        //watcher.IncludeSubdirectories = true;
        watcher.EnableRaisingEvents = true;
        watcher.InternalBufferSize = 65536;

        Console.WriteLine("Press enter to exit.");
        Console.ReadLine();
    }
    [SupportedOSPlatform("windows")]
    private static void OnCreated(object sender, FileSystemEventArgs e)
    {
        string value = $"Created: {e.FullPath}";
        //Console.WriteLine(value);
        Trace.WriteLine("\n"+value);
        InitialSweep.ProcessPDFBetter(e.FullPath);
        //InitialSweep.ProcessTiff(e.FullPath);
        //File.Delete(e.FullPath);
    }

    private static void OnDeleted(object sender, FileSystemEventArgs e) =>
        Console.WriteLine($"Deleted: {e.FullPath}");

    private static void OnRenamed(object sender, RenamedEventArgs e)
    {
        Console.WriteLine($"Renamed:");
        Console.WriteLine($"    Old: {e.OldFullPath}");
        Console.WriteLine($"    New: {e.FullPath}");
    }

    private static void OnError(object sender, ErrorEventArgs e) =>
        PrintException(e.GetException());

    private static void PrintException(Exception? ex)
    {
        if (ex != null)
        {
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine("Stacktrace:");
            Console.WriteLine(ex.StackTrace);
            Console.WriteLine();
            PrintException(ex.InnerException);
        }
    }
}
