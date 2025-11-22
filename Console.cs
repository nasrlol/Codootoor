namespace Odootoor;

// Simple code interpreter
public partial class Program
{
    public static string ExecuteCode(List<string> codeLines)
    {
        var output = new List<string>();
        output.Add("=== Program Output ===");

        foreach (var line in codeLines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            string trimmedLine = line.Trim();

            // Simple print statement
            if (trimmedLine.StartsWith("print") || trimmedLine.StartsWith("echo"))
            {
                string content = trimmedLine.Substring(trimmedLine.IndexOf(' ') + 1).Trim();
                if (content.StartsWith("\"") && content.EndsWith("\""))
                {
                    output.Add(content.Substring(1, content.Length - 2));
                }
                else
                {
                    output.Add($"[Printed: {content}]");
                }
            }
            // Simple calculation
            else if (trimmedLine.Contains("+") || trimmedLine.Contains("-") || trimmedLine.Contains("*") || trimmedLine.Contains("/"))
            {
                try
                {
                    // Very basic math evaluation
                    var dataTable = new System.Data.DataTable();
                    var result = dataTable.Compute(trimmedLine, "");
                    output.Add($"{trimmedLine} = {result}");
                }
                catch
                {
                    output.Add($"[Calculation: {trimmedLine}]");
                }
            }
            // Variable assignment
            else if (trimmedLine.Contains("="))
            {
                output.Add($"[Variable set: {trimmedLine}]");
            }
            // Comment
            else if (trimmedLine.StartsWith("//") || trimmedLine.StartsWith("#"))
            {
                output.Add($"[Comment: {trimmedLine}]");
            }
            else
            {
                output.Add($"[Executed: {trimmedLine}]");
            }
        }

        output.Add("=== End of Output ===");
        return string.Join("\n", output);
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
}
