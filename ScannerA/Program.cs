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
    private static readonly string PipeName = "ScannerAPipe";
    private static Process currentProcess;

    static async Task Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Please provide a directory path as an argument.");
            return;
        }

        string directoryPath = args[0];
        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine("The specified directory does not exist.");
            return;
        }

        // Set CPU affinity to core 0
        currentProcess = Process.GetCurrentProcess();
        currentProcess.ProcessorAffinity = (IntPtr)(1 << 0); // Use first core

        try
        {
            await ScanAndSendFiles(directoryPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static async Task ScanAndSendFiles(string directoryPath)
    {
        var fileScanTask = Task.Run(() => ScanFiles(directoryPath));
        var results = await fileScanTask;

        using (var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
        {
            Console.WriteLine("Connecting to master...");
            await pipeClient.ConnectAsync(5000);

            using (var writer = new StreamWriter(pipeClient))
            {
                foreach (var result in results)
                {
                    await writer.WriteLineAsync(result);
                    await writer.FlushAsync();
                }
            }
        }
    }

    private static List<string> ScanFiles(string directoryPath)
    {
        var results = new List<string>();
        var files = Directory.GetFiles(directoryPath, "*.txt");

        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);
            string content = File.ReadAllText(file);
            string[] words = content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var wordCount = new Dictionary<string, int>();

            foreach (var word in words)
            {
                string cleanWord = word.ToLower().Trim(new[] { '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}' });
                if (!string.IsNullOrEmpty(cleanWord))
                {
                    if (!wordCount.ContainsKey(cleanWord))
                    {
                        wordCount[cleanWord] = 0;
                    }
                    wordCount[cleanWord]++;
                }
            }

            foreach (var count in wordCount)
            {
                results.Add($"{fileName}:{count.Key}:{count.Value}");
            }
        }

        return results;
    }
}
