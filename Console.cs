using System.Runtime.InteropServices;

namespace Odootoor;

using System.Diagnostics;

using Raylib_cs;


// Simple code interpreter
public partial class Program
{
    public class Piper 
    {
        private Process? proc;
        public List<string> OutputBuffer = new();

        public void Run(string codeToExecute)
        {
            proc = new Process();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                proc.StartInfo.FileName = "python";
            }
            else
            {
                proc.StartInfo.FileName = "python3";
            }

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.CreateNoWindow = true;

            proc.Start();

            proc.StandardInput.WriteLine(codeToExecute);
            proc.StandardInput.WriteLine(""); 
            proc.StandardInput.Close();

            Task.Run(() =>
                    {
                    while (!proc.StandardOutput.EndOfStream)
                    {
                    string line = proc.StandardOutput.ReadLine();
                    lock (OutputBuffer)
                    {
                    OutputBuffer.Add(line);
                    }
                    }

                    while (!proc.StandardError.EndOfStream)
                    {
                    var line = proc.StandardError.ReadLine();
                    lock (OutputBuffer)
                    {
                    OutputBuffer.Add("Error: " + line);
                    }
                    }
                    });
        }

        public void Draw(Rectangle bounds)
        {
            int y = (int)bounds.Y + 40;
            lock (OutputBuffer)
            {
                foreach (var line in OutputBuffer)
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
