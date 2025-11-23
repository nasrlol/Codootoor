using System.Runtime.InteropServices;

namespace Odootoor;

using System.Diagnostics;


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
                    string? line = proc.StandardOutput.ReadLine();
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

        // public void Draw(Rectangle bounds)
        // {
        //     const int   font_size = 14;
        //     const float spacing = 0.5f;
        //     const int   line_height = 20;
        //     const int   padding_x = 40;
        //     const int   padding_y = 10;
        //
        //     Font defaultFont = Raylib.GetFontDefault();
        //     int startX = (int)bounds.X + padding_x;
        //     int startY = (int)bounds.Y + padding_y;
        //     int currentY = startY;
        //
        //     lock (OutputBuffer)
        //     {
        //         foreach (var line in OutputBuffer)
        //         {
        //             int currentX = startX;
        //
        //             foreach(var c in line)
        //             {
        //                 Raylib.DrawTextEx(defaultFont, c.ToString(), new Vector2(currentX, currentY), font_size, spacing, Color.White);
        //                 currentX += (int)(font_size * 0.6f); 
        //             }
        //             currentY += line_height; 
        //         }
        //     }
        // }
        //
        public void Stop()
        {
            if (proc != null && !proc.HasExited)
                proc.Kill();
        }

    }
}
