namespace Odootoor;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.IO;

public partial class Program
{
    // Key repeating system
    private static float initialDelay = 0.3f;
    private static float repeatRate = 0.05f;
    private static float keyHoldTimer = 0f;
    private static KeyboardKey lastHeldKey = KeyboardKey.Null;
    private static bool isRepeating = false;
    private static int cursorPosition = 0;

    static Vector2 lastCharPos;
    static string? lastCharString;

    public struct Editor
    {
        public Rectangle Bounds;
        public string Text = "";
        public float ScrollOffset;
        public int CurrentLine;
        public Vector2 Position;

        public Editor(Rectangle bounds, Vector2 position)
        {
            Bounds = bounds;
            Position = position;
        }
    }

    static Editor editor;

    public const int LINE_HEIGHT = 25;

    public static void HandleInput()
    {
        HandleArrowNavigation();
        ProcessControlKeys();
        ProcessCharacterInput();
        UpdateKeyRepeatTiming();
    }

    private static void ProcessControlKeys()
    {
        // Handle backspace
        if (IsKeyPressed(KeyboardKey.Backspace))
        {
            HandleBackspace();
            lastHeldKey = KeyboardKey.Backspace;
            keyHoldTimer = 0f;
            isRepeating = false;
            pressedChar = true;
        }

        if (IsKeyDown(KeyboardKey.Backspace) && lastHeldKey == KeyboardKey.Backspace)
        {
            keyHoldTimer += GetFrameTime();
            if (ShouldRepeatKey())
            {
                HandleBackspace();
                pressedChar = true;
            }
        }

        // Handle Delete key
        if (IsKeyPressed(KeyboardKey.Delete))
        {
            HandleDelete();
            lastHeldKey = KeyboardKey.Delete;
            keyHoldTimer = 0f;
            isRepeating = false;
        }

        if (IsKeyDown(KeyboardKey.Delete) && lastHeldKey == KeyboardKey.Delete)
        {
            keyHoldTimer += GetFrameTime();
            if (ShouldRepeatKey())
            {
                HandleDelete();
            }
        }

        // Handle enter
        if (IsKeyPressed(KeyboardKey.Enter))
        {
            HandleEnter();
            lastHeldKey = KeyboardKey.Enter;
            keyHoldTimer = 0f;
            isRepeating = false;
        }

        // Handle space (with proper repeating)
        if (IsKeyPressed(KeyboardKey.Space))
        {
            HandleSpace();
            lastHeldKey = KeyboardKey.Space;
            keyHoldTimer = 0f;
            isRepeating = false;
        }

        if (IsKeyDown(KeyboardKey.Space) && lastHeldKey == KeyboardKey.Space)
        {
            keyHoldTimer += GetFrameTime();
            if (ShouldRepeatKey())
            {
                HandleSpace();
            }
        }

        // Handle Tab key for indentation
        if (IsKeyPressed(KeyboardKey.Tab))
        {
            editor.Text = editor.Text.Insert(cursorPosition, "    ");
            cursorPosition += 4;
        }
    }

    private static void HandleDelete()
    {
        //if (cursorPosition < editor.CurrentInput.Length)
        //{
        //    // Verwijder character na cursor
        //    editor.CurrentInput = editor.CurrentInput.Remove(cursorPosition, 1);
        //}
        //else if (editor.CurrentLine < editor.Lines.Count - 1)
        //{
        //    // Delete aan einde van regel - voeg volgende regel samen met huidige
        //    string nextLine = editor.Lines[editor.CurrentLine + 1];
        //    editor.CurrentInput += nextLine;
        //    editor.Lines.RemoveAt(editor.CurrentLine + 1);
        //}
    }

    static bool pressedChar;

    private static void ProcessCharacterInput()
    {
        // Simple approach - use GetCharPressed but process only 1 character per frame

        int key = GetCharPressed();
        if (key > 0)
        {
            pressedChar = true;

            string charString = ((char)key).ToString();
            lastCharString = charString;

            editor.Text = editor.Text.Insert(cursorPosition, charString);
            cursorPosition++;
        }
    }

    private static bool ShouldRepeatKey()
    {
        if (!isRepeating)
        {
            if (keyHoldTimer >= initialDelay)
            {
                isRepeating = true;
                keyHoldTimer = 0f;
                return true;
            }
            return false;
        }
        else
        {
            if (keyHoldTimer >= repeatRate)
            {
                keyHoldTimer = 0f;
                return true;
            }
            return false;
        }
    }

    private static void UpdateKeyRepeatTiming()
    {
        // Check if the last held key is no longer being pressed
        if (lastHeldKey != KeyboardKey.Null && !IsKeyDown(lastHeldKey))
        {
            lastHeldKey = KeyboardKey.Null;
            keyHoldTimer = 0f;
            isRepeating = false;
        }
    }

    private static void HandleBackspace()
    {
        if (cursorPosition > 0)
        {
            editor.Text = editor.Text.Remove(cursorPosition - 1, 1);
            cursorPosition--;
        }
    }

    private static void HandleSpace()
    {
        editor.Text = editor.Text.Insert(cursorPosition, " ");
        cursorPosition++;
    }

    private static void HandleArrowNavigation()
    {
        // Left arrow handling with repeating
        if (IsKeyPressed(KeyboardKey.Left))
        {
            if (cursorPosition > 0) cursorPosition--;
            lastHeldKey = KeyboardKey.Left;
            keyHoldTimer = 0f;
            isRepeating = false;
        }

        if (IsKeyDown(KeyboardKey.Left) && lastHeldKey == KeyboardKey.Left)
        {
            keyHoldTimer += GetFrameTime();
            if (ShouldRepeatKey() && cursorPosition > 0)
            {
                cursorPosition--;
            }
        }

        // Right arrow handling with repeating
        if (IsKeyPressed(KeyboardKey.Right))
        {
            if (cursorPosition < editor.Text.Length) cursorPosition++;
            lastHeldKey = KeyboardKey.Right;
            keyHoldTimer = 0f;
            isRepeating = false;
        }

        if (IsKeyDown(KeyboardKey.Right) && lastHeldKey == KeyboardKey.Right)
        {
            keyHoldTimer += GetFrameTime();
            if (ShouldRepeatKey() && cursorPosition < editor.Text.Length)
            {
                cursorPosition++;
            }
        }

        // Up arrow - go to previous line (without repeating for now)
        if (IsKeyPressed(KeyboardKey.Up))
        {
            int currentLineStart = GetCurrentLineStart();
            if (currentLineStart > 0)
            {
                // Find the start of previous line
                int prevLineEnd = currentLineStart - 1;
                int prevLineStart = editor.Text.LastIndexOf('\n', prevLineEnd - 1) + 1;

                // Calculate cursor position in previous line
                int cursorOffset = cursorPosition - currentLineStart;
                int prevLineLength = prevLineEnd - prevLineStart;
                int newCursorPos = prevLineStart + Math.Min(cursorOffset, prevLineLength);

                cursorPosition = newCursorPos;

                // Adjust scroll if needed
                int lineNumber = GetLineNumberFromPosition(cursorPosition);
                if (lineNumber * LINE_HEIGHT < editor.ScrollOffset)
                {
                    editor.ScrollOffset = Math.Max(0, lineNumber * LINE_HEIGHT);
                }
            }
        }

        // Down arrow - go to next line (without repeating for now)
        if (IsKeyPressed(KeyboardKey.Down))
        {
            int currentLineStart = GetCurrentLineStart();
            int currentLineEnd = GetCurrentLineEnd();

            if (currentLineEnd < editor.Text.Length)
            {
                // Find the start of next line
                int nextLineStart = currentLineEnd + 1;
                int nextLineEnd = editor.Text.IndexOf('\n', nextLineStart);
                if (nextLineEnd == -1) nextLineEnd = editor.Text.Length;

                // Calculate cursor position in next line
                int cursorOffset = cursorPosition - currentLineStart;
                int nextLineLength = nextLineEnd - nextLineStart;
                int newCursorPos = nextLineStart + Math.Min(cursorOffset, nextLineLength);

                cursorPosition = newCursorPos;

                // Adjust scroll if needed
                int lineNumber = GetLineNumberFromPosition(cursorPosition);
                float bottomScroll = editor.ScrollOffset + editor.Bounds.Height - LINE_HEIGHT;
                if (lineNumber * LINE_HEIGHT > bottomScroll)
                {
                    editor.ScrollOffset = lineNumber * LINE_HEIGHT - editor.Bounds.Height + LINE_HEIGHT;
                }
            }
        }

        // Home key - move to start of line
        if (IsKeyPressed(KeyboardKey.Home))
        {
            int lineStart = GetCurrentLineStart();
            cursorPosition = lineStart;
        }

        // End key - move to end of line
        if (IsKeyPressed(KeyboardKey.End))
        {
            int lineEnd = GetCurrentLineEnd();
            cursorPosition = lineEnd;
        }
    }

    // Helper methods for line navigation
    private static int GetCurrentLineStart()
    {
        if (cursorPosition == 0) return 0;
        int lineStart = editor.Text.LastIndexOf('\n', cursorPosition - 1) + 1;
        return lineStart;
    }

    private static int GetCurrentLineEnd()
    {
        int lineEnd = editor.Text.IndexOf('\n', cursorPosition);
        if (lineEnd == -1) lineEnd = editor.Text.Length;
        return lineEnd;
    }

    private static int GetLineNumberFromPosition(int position)
    {
        if (position == 0) return 0;

        int lineNumber = 0;
        for (int i = 0; i < position; i++)
        {
            if (editor.Text[i] == '\n')
                lineNumber++;
        }
        return lineNumber;
    }

    public static void HandleScroll(Vector2 mousePos)
    {
        if (CheckCollisionPointRec(mousePos, editor.Bounds))
        {
            float mouseWheel = GetMouseWheelMove();
            editor.ScrollOffset -= mouseWheel * 20;
            int totalLines = GetTotalLines();
            float maxScroll = Math.Max(0, totalLines * LINE_HEIGHT - editor.Bounds.Height + 50);
            editor.ScrollOffset = Math.Clamp(editor.ScrollOffset, 0, maxScroll);



        }
    }

    private static int GetTotalLines()
    {
        if (string.IsNullOrEmpty(editor.Text)) return 1;
        int count = 1;
        foreach (char c in editor.Text)
        {
            if (c == '\n') count++;
        }
        return count;
    }

    public static void DrawEditor()
    {
        // Editor background with solid color
        DrawRectangleRec(editor.Bounds, new Color(25, 25, 35, 255));

        // Editor border
        DrawRectangleLines((int)editor.Bounds.X, (int)editor.Bounds.Y, (int)editor.Bounds.Width, (int)editor.Bounds.Height, new Color(60, 60, 80, 255));
        DrawRectangleLines((int)editor.Bounds.X - 1, (int)editor.Bounds.Y - 1, (int)editor.Bounds.Width + 2, (int)editor.Bounds.Height + 2, new Color(20, 20, 30, 255));

        DrawLineNumbers();
        DrawCodeLines();
        DrawCursor();
        DrawScrollBar();
    }

    private static void DrawLineNumbers()
    {
        DrawRectangle((int)editor.Bounds.X, (int)editor.Bounds.Y, 40, (int)editor.Bounds.Height, new Color(35, 35, 45, 255));
        DrawLine((int)editor.Bounds.X + 40, (int)editor.Bounds.Y, (int)editor.Bounds.X + 40, (int)editor.Bounds.Y + (int)editor.Bounds.Height, new Color(60, 60, 80, 255));
    }

    private static void DrawCodeLines()
    {


        int visibleLines = (int)(editor.Bounds.Height / LINE_HEIGHT);
        int startLine = (int)(editor.ScrollOffset / LINE_HEIGHT);
        int totalLines = GetTotalLines();
        int endLine = Math.Min(startLine + visibleLines + 1, totalLines);

        string[] lines = editor.Text.Split('\n');

        for (int i = startLine; i < endLine; i++)
        {
            float yPos = editor.Bounds.Y + 20 + (i - startLine) * LINE_HEIGHT - (editor.ScrollOffset % LINE_HEIGHT);

            if (yPos >= editor.Bounds.Y && yPos <= editor.Bounds.Y + editor.Bounds.Height - LINE_HEIGHT)
            {
                Color lineColor = new Color(220, 220, 220, 255);
                string lineText = i < lines.Length ? lines[i] : "";

                DrawText($"{i + 1}", (int)editor.Bounds.X + 10, (int)yPos, 18, new Color(150, 150, 170, 255));
                DrawText(lineText, (int)editor.Bounds.X + 45, (int)yPos, 18, lineColor);
            }
        }
    }

    private static void DrawCursor()
    {
        int currentLine = GetLineNumberFromPosition(cursorPosition);
        int startLine = (int)(editor.ScrollOffset / LINE_HEIGHT);
        int totalLines = GetTotalLines();

        float currentInputY = editor.Bounds.Y + 20 + (currentLine - startLine) * LINE_HEIGHT - (editor.ScrollOffset % LINE_HEIGHT);

        if (currentInputY >= editor.Bounds.Y && currentInputY <= editor.Bounds.Y + editor.Bounds.Height - LINE_HEIGHT)
        {
            // Show the correct line number
            DrawText($"{currentLine + 1}:", (int)editor.Bounds.X + 10, (int)currentInputY, 18, new Color(150, 150, 170, 255));

            // Get the current line text
            int lineStart = GetCurrentLineStart();
            int lineEnd = GetCurrentLineEnd();
            string currentLineText = editor.Text.Substring(lineStart, lineEnd - lineStart);
            string textBeforeCursor = currentLineText.Substring(0, cursorPosition - lineStart);

            // Draw text before cursor
            DrawText(currentLineText, (int)editor.Bounds.X + 45, (int)currentInputY, 18, Color.White);

            // Calculate cursor position
            int cursorX = (int)editor.Bounds.X + 45 + MeasureText(textBeforeCursor, 18);

            lastCharPos = new Vector2(cursorX, currentInputY);

            // Animate cursor
            if ((int)(GetTime() * 2) % 2 == 0)
            {
                DrawRectangle(cursorX, (int)currentInputY, 2, 18, Color.Yellow);
            }

        }
    }

    private static void DrawScrollBar()
    {
        int totalLines = GetTotalLines();
        if (totalLines * LINE_HEIGHT > editor.Bounds.Height)
        {
            float scrollbarHeight = editor.Bounds.Height * (editor.Bounds.Height / (totalLines * LINE_HEIGHT));
            float scrollbarY = editor.Bounds.Y + (editor.ScrollOffset / (totalLines * LINE_HEIGHT)) * (editor.Bounds.Height - scrollbarHeight);

            DrawRectangle((int)editor.Bounds.X + (int)editor.Bounds.Width - 12, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(80, 80, 100, 255));
            DrawRectangleLines((int)editor.Bounds.X + (int)editor.Bounds.Width - 12, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(120, 120, 140, 255));
        }
    }

    public static void ClearEditor()
    {
        editor.Text = "";
        editor.ScrollOffset = 0;
        editor.CurrentLine = 0;
        cursorPosition = 0;
    }

    private static void HandleEnter()
    {
        editor.Text = editor.Text.Insert(cursorPosition, "\n");
        cursorPosition++;

        // Adjust scroll
        int totalLines = GetTotalLines();
        editor.ScrollOffset = Math.Max(0, totalLines * LINE_HEIGHT - editor.Bounds.Height + LINE_HEIGHT);
    }

    public static bool SaveCodeToFile(string code)
    {
        try
        {
            string directoryPath = "saves";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, "code.txt");
            File.WriteAllText(filePath, code);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving file: {ex.Message}");
            return false;
        }
    }

    public static string ToBuffer(List<String> lines)
    {
        if (lines == null) return string.Empty;
        return string.Join(Environment.NewLine, lines);
    }

    public void RandomDeletion()
    {
        Random num = new();
        if (num.Next(1, 10) < 6)
        {
            statusMessage = "LOL YOU'RE CODE IS GONE";
            ClearEditor();
        }
        else return;
    }
}
