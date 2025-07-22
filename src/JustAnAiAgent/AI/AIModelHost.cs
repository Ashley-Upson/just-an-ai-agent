using System.Diagnostics;

namespace JustAnAiAgent.AI;

public class AIModelHost
{
    Process ollamaProcess;
    readonly ProcessStartInfo ollamaArgs;

    public AIModelHost(string ollamaExePath, string model)
    {
        //Options: 1.5b,7b,8b,14b,32b,70b,671b
        EnsureModelIsDownloaded(ollamaExePath, model);

        ollamaArgs = new ProcessStartInfo
        {
            FileName = ollamaExePath, // Assumes ollama is in PATH or project directory
            Arguments = "serve --gpu",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
    }

    public void EnsureModelIsDownloaded(string ollamaExePath, string model)
    {
        Console.WriteLine($"[Model Prep Output] Downloading model {model} ...");
        var pullProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = ollamaExePath,
                Arguments = "pull " + model,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        pullProcess.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine($"[Model Prep Output] {e.Data}");
        };

        pullProcess.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine($"[Model Prep Error] {e.Data}");
        };

        pullProcess.Start();
        pullProcess.WaitForExit(); // Wait for the model to download
    }

    public void Start()
    {
        Console.WriteLine($"[Model Output] Starting up model host ...");

        if (ollamaProcess is not null)
            throw new InvalidOperationException("AI Host Process is already running!");

        ollamaProcess = new Process { StartInfo = ollamaArgs };

        ollamaProcess.OutputDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine($"[Model Output] {e.Data}");
        };

        ollamaProcess.ErrorDataReceived += (sender, e) =>
        {
            if (e.Data != null)
                Console.WriteLine($"[Model Error] {e.Data}");
        };

        ollamaProcess.Start();

        Console.WriteLine("AI server started.");
    }

    public void Stop()
    {
        if (ollamaProcess != null && !ollamaProcess.HasExited)
        {
            ollamaProcess.Kill();
            ollamaProcess.Dispose();

            Console.WriteLine("AI server stopped.");
        }
    }
}