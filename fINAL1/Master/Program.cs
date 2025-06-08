using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

class Program
{
    private static readonly string ScannerAPipeName = "ScannerAPipe";
    private static readonly string ScannerBPipeName = "ScannerBPipe";
    private static Process currentProcess;
    private static List<string> results = new List<string>();

    static async Task Main(string[] args)
    {
        // Set CPU affinity to core 2
        currentProcess = Process.GetCurrentProcess();
        currentProcess.ProcessorAffinity = (IntPtr)(1 << 2); // Use third core

        Console.WriteLine("Master process started. Waiting for scanners...");

        var scannerATask = Task.Run(() => HandleScannerConnection(ScannerAPipeName));
        var scannerBTask = Task.Run(() => HandleScannerConnection(ScannerBPipeName));

        await Task.WhenAll(scannerATask, scannerBTask);

        // Display final results
        Console.WriteLine("\nFinal Results:");
        foreach (var result in results.OrderBy(x => x))
        {
            Console.WriteLine(result);
        }
    }

    private static async Task HandleScannerConnection(string pipeName)
    {
        using (var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.In))
        {
            Console.WriteLine($"Waiting for connection on {pipeName}...");
            await pipeServer.WaitForConnectionAsync();
            Console.WriteLine($"Connected to {pipeName}");

            using (var reader = new StreamReader(pipeServer))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lock (results)
                    {
                        results.Add(line);
                    }
                }
            }
        }
    }
}
