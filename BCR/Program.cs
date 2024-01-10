using BCR.Library;
using System.Diagnostics;

Trace.Listeners.Clear();

TextWriterTraceListener twtl = new("../../../../BCR.Library/Data/log.txt")
{
    Name = "TextLogger",
    TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime
};

ConsoleTraceListener ctl = new(false)
{
    TraceOutputOptions = TraceOptions.DateTime
};

Trace.Listeners.Add(twtl);
Trace.Listeners.Add(ctl);
Trace.AutoFlush = true;

//Stopwatch sw = Stopwatch.StartNew();
InitialSweep.CheckFolderForPDF("\\\\ARCH-FRIGATE\\Scans\\BCR Test");
//sw.Stop();
//Console.WriteLine($"Initial sweep is over after {sw.ElapsedMilliseconds} milliseconds");
FileWatcher.Watch();

//string fileName = "";
//string splitPage = "";

//while (fileName != "q" || splitPage != "q")
//{
//    Console.WriteLine("Enter 'q' at any time ypu wish to exit");
//    Console.WriteLine("Enter file name in FailedScans folder to be cut");
//    fileName = Console.ReadLine();
//    Console.WriteLine("Enter split page");
//    splitPage = Console.ReadLine();
//    Console.WriteLine("Creating new file...");
//    FileManipulator.SplitPdf($"..\\..\\..\\..\\BCR.Library\\Data\\FailedScans\\{fileName}.pdf", Int32.Parse(splitPage) - 1);
//}