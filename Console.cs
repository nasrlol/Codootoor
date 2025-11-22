namespace Odootoor;
using System.Diagnostics;
using Raylib_cs;

// Simple code interpreter
public partial class Program
{
    public static string ExecuteCode(List<string> lines)
    {
        return "";
    }

    static void ExecuteCode()
    {
        string fullCode = string.Join("\n", editor.Lines) +
                          (string.IsNullOrEmpty(editor.CurrentInput) ? "" : "\n" + editor.CurrentInput);

        if (!string.IsNullOrWhiteSpace(editor.CurrentInput))
        {
            editor.Lines.Add(editor.CurrentInput);
            editor.CurrentInput = "";
        }

        if (fullCode.Length > 0)
        {
            outputWindow.OutputText = ExecuteCode(editor.Lines);
            outputWindow.IsVisible = true;
            achievementManager.MarkProgramExecuted();
            statusMessage = "Code executed successfully! Check output window.";

            currentState = GameState.Editing;
            stickman.Reset();
        }
        else
        {
            statusMessage = "Write some code first!";
        }
    }
    public class Output
    {
        public Process proc;
        public List<string> buffer = new List<string>();
        public const int MaxLines = 200;

        public void Init()
        {
            proc = new Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \"cat\"";
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            proc.Start();

            Task.Run(() =>
            {
                var streamWriter = proc.StandardInput;
                streamWriter.WriteLine("1");
                while (!proc.StandardOutput.EndOfStream)
                {
                    string line = proc.StandardOutput.ReadLine();
                    lock (buffer)
                    {
                        buffer.Add(line);
                        if (buffer.Count > MaxLines)
                            buffer.RemoveAt(0);
                    }
                }
            });
        }

        public void Draw(Rectangle bounds)
        {
            int y = (int)bounds.Y + 40;
            lock (buffer)
            {
                foreach (var line in buffer)
                {
                    Raylib.DrawText(line, (int)bounds.X + 10, y, 14, Color.White);
                    y += 20;
                }
            }
        }

        public void Stop()
        {
            if (proc != null && !proc.HasExited)
                proc.Kill();
        }
    }
}