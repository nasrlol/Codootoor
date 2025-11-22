using System.Runtime.InteropServices;

namespace Odootoor;

using System.Diagnostics;
using Raylib_cs;

// Simple code interpreter
public partial class Program
{
    public static string ExecuteCode(List<string> lines)
    {
        var output = new List<string>();
        output.Add("=== Program Output ===");
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            
            // Simple "print" command recognition
            if (line.Trim().StartsWith("print ") && line.Contains("\""))
            {
                try
                {
                    int start = line.IndexOf('"') + 1;
                    int end = line.LastIndexOf('"');
                    if (end > start)
                    {
                        string text = line.Substring(start, end - start);
                        output.Add(text);
                    }
                }
                catch
                {
                    output.Add($"Error in print statement: {line}");
                }
            }
            else
            {
                output.Add($"Executed: {line.Trim()}");
            }
        }
        
        output.Add("=== End of Output ===");
        return string.Join("\n", output);
    }
}