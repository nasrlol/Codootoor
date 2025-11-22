namespace Odootoor;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System;
using static System.Console;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
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

    public struct Editor
    {
        public Rectangle Bounds;
        public List<string> Lines = new List<string>();
        public string CurrentInput = "";
        public float ScrollOffset;
        public int CurrentLine;
        public Vector2 Position;

        public Editor(Rectangle bounds, Vector2 position)
        {
            Bounds = bounds;
            Position = position;
        }
    }

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
    }

    if (IsKeyDown(KeyboardKey.Backspace) && lastHeldKey == KeyboardKey.Backspace)
    {
        keyHoldTimer += GetFrameTime();
        if (ShouldRepeatKey())
        {
            HandleBackspace();
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
        editor.CurrentInput = editor.CurrentInput.Insert(cursorPosition, "    ");
        cursorPosition += 4;
    }
}

private static void HandleDelete()
{
    if (cursorPosition < editor.CurrentInput.Length)
    {
        // Verwijder character na cursor
        editor.CurrentInput = editor.CurrentInput.Remove(cursorPosition, 1);
    }
    else if (editor.CurrentLine < editor.Lines.Count - 1)
    {
        // Delete aan einde van regel - voeg volgende regel samen met huidige
        string nextLine = editor.Lines[editor.CurrentLine + 1];
        editor.CurrentInput += nextLine;
        editor.Lines.RemoveAt(editor.CurrentLine + 1);
    }
}

    private static void ProcessCharacterInput()
    {
        // Simpele aanpak - gebruik GetCharPressed maar verwerk maar 1 character per frame
        int key = GetCharPressed();
        if (key > 0)
        {
            char c = (char)key;
            editor.CurrentInput = editor.CurrentInput.Insert(cursorPosition, c.ToString());
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
        // Normale backspace - verwijder character voor cursor
        editor.CurrentInput = editor.CurrentInput.Remove(cursorPosition - 1, 1);
        cursorPosition--;
    }
    else if (editor.CurrentLine > 0 && string.IsNullOrEmpty(editor.CurrentInput))
    {
        // Backspace op lege regel - ga naar vorige regel
        editor.CurrentLine--;
        editor.CurrentInput = editor.Lines[editor.CurrentLine];
        cursorPosition = editor.CurrentInput.Length;
        
        // Verwijder de lege regel uit de lijst
        editor.Lines.RemoveAt(editor.CurrentLine);
        
        // Pas scroll aan
        if (editor.CurrentLine * LINE_HEIGHT < editor.ScrollOffset)
        {
            editor.ScrollOffset = Math.Max(0, editor.CurrentLine * LINE_HEIGHT);
        }
    }
}

    private static void HandleSpace()
    {
        editor.CurrentInput = editor.CurrentInput.Insert(cursorPosition, " ");
        cursorPosition++;
    }

    private static void HandleArrowNavigation()
    {
        // Left/Right arrow handling
        if (IsKeyPressed(KeyboardKey.Left))
        {
            if (cursorPosition > 0) cursorPosition--;
        }

        if (IsKeyPressed(KeyboardKey.Right))
        {
            if (cursorPosition < editor.CurrentInput.Length) cursorPosition++;
        }

        // Up arrow - go to previous line
        if (IsKeyPressed(KeyboardKey.Up))
        {
            if (editor.CurrentLine > 0)
            {
                // Save current input to the current line before moving up
                if (editor.CurrentLine < editor.Lines.Count)
                {
                    editor.Lines[editor.CurrentLine] = editor.CurrentInput;
                }
                
                editor.CurrentLine--;
                editor.CurrentInput = editor.Lines[editor.CurrentLine];
                cursorPosition = Math.Min(cursorPosition, editor.CurrentInput.Length);
                
                // Adjust scroll if needed
                if (editor.CurrentLine * LINE_HEIGHT < editor.ScrollOffset)
                {
                    editor.ScrollOffset = Math.Max(0, editor.CurrentLine * LINE_HEIGHT);
                }
            }
        }

        // Down arrow - go to next line
        if (IsKeyPressed(KeyboardKey.Down))
        {
            if (editor.CurrentLine < editor.Lines.Count)
            {
                // Save current line before moving down
                if (editor.CurrentLine < editor.Lines.Count)
                {
                    editor.Lines[editor.CurrentLine] = editor.CurrentInput;
                }

                editor.CurrentLine++;
                if (editor.CurrentLine < editor.Lines.Count)
                {
                    editor.CurrentInput = editor.Lines[editor.CurrentLine];
                }
                else
                {
                    editor.CurrentInput = "";  // New line at the end
                }
                cursorPosition = Math.Min(cursorPosition, editor.CurrentInput.Length);
                
                // Adjust scroll if needed
                float bottomScroll = editor.ScrollOffset + editor.Bounds.Height - LINE_HEIGHT;
                if (editor.CurrentLine * LINE_HEIGHT > bottomScroll)
                {
                    editor.ScrollOffset = editor.CurrentLine * LINE_HEIGHT - editor.Bounds.Height + LINE_HEIGHT;
                }
            }
        }

        // Home key - move to start of line
        if (IsKeyPressed(KeyboardKey.Home))
        {
            cursorPosition = 0;
        }

        // End key - move to end of line
        if (IsKeyPressed(KeyboardKey.End))
        {
            cursorPosition = editor.CurrentInput.Length;
        }
    }

    public static void HandleScroll(Vector2 mousePos)
    {
        if (CheckCollisionPointRec(mousePos, editor.Bounds))
        {
            float mouseWheel = GetMouseWheelMove();
            editor.ScrollOffset -= mouseWheel * 20;
            float maxScroll = Math.Max(0, editor.Lines.Count * LINE_HEIGHT - editor.Bounds.Height + 50);
            editor.ScrollOffset = Math.Clamp(editor.ScrollOffset, 0, maxScroll);
        }
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
        DrawCurrentInput();
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
        int endLine = Math.Min(startLine + visibleLines + 1, editor.Lines.Count);

        for (int i = startLine; i < endLine; i++)
        {
            float yPos = editor.Bounds.Y + 20 + (i - startLine) * LINE_HEIGHT - (editor.ScrollOffset % LINE_HEIGHT);

            if (yPos >= editor.Bounds.Y && yPos <= editor.Bounds.Y + editor.Bounds.Height - LINE_HEIGHT)
            {
                Color lineColor = new Color(220, 220, 220, 255);

                DrawText($"{i + 1}", (int)editor.Bounds.X + 10, (int)yPos, 18, new Color(150, 150, 170, 255));
                DrawText(editor.Lines[i], (int)editor.Bounds.X + 45, (int)yPos, 18, lineColor);
            }
        }
    }

private static void DrawCurrentInput()
{
    int startLine = (int)(editor.ScrollOffset / LINE_HEIGHT);
    float currentInputY = editor.Bounds.Y + 20 + (editor.Lines.Count - startLine) * LINE_HEIGHT - (editor.ScrollOffset % LINE_HEIGHT);

    if (currentInputY >= editor.Bounds.Y && currentInputY <= editor.Bounds.Y + editor.Bounds.Height - LINE_HEIGHT)
    {
        // Show the correct line number for the current input
        DrawText($"{editor.Lines.Count + 1}:", (int)editor.Bounds.X + 10, (int)currentInputY, 18, new Color(150, 150, 170, 255));

        // SIMPELE CURSOR: gewoon de hele tekst tekenen en cursor apart
        string textBeforeCursor = editor.CurrentInput.Substring(0, cursorPosition);
        string textAfterCursor = editor.CurrentInput.Substring(cursorPosition);

        // Teken tekst voor cursor
        DrawText(textBeforeCursor, (int)editor.Bounds.X + 45, (int)currentInputY, 18, Color.White);
        
        // Bereken cursor positie
        int cursorX = (int)editor.Bounds.X + 45 + MeasureText(textBeforeCursor, 18);
        
        // Teken cursor
        if ((int)(GetTime() * 2) % 2 == 0)
        {
            DrawRectangle(cursorX, (int)currentInputY, 2, 18, Color.Yellow);
        }
        
        // Teken tekst na cursor
        DrawText(textAfterCursor, cursorX, (int)currentInputY, 18, Color.White);
    }
}

    private static void DrawScrollBar()
    {
        if (editor.Lines.Count * LINE_HEIGHT > editor.Bounds.Height)
        {
            float scrollbarHeight = editor.Bounds.Height * (editor.Bounds.Height / (editor.Lines.Count * LINE_HEIGHT));
            float scrollbarY = editor.Bounds.Y + (editor.ScrollOffset / (editor.Lines.Count * LINE_HEIGHT)) * (editor.Bounds.Height - scrollbarHeight);

            DrawRectangle((int)editor.Bounds.X + (int)editor.Bounds.Width - 12, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(80, 80, 100, 255));
            DrawRectangleLines((int)editor.Bounds.X + (int)editor.Bounds.Width - 12, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(120, 120, 140, 255));
        }
    }

    public static void ClearEditor()
    {
        editor.Lines.Clear();
        editor.CurrentInput = "";
        editor.ScrollOffset = 0;
        editor.CurrentLine = 0;
        cursorPosition = 0;
    }

private static void HandleEnter()
{
    if (cursorPosition == editor.CurrentInput.Length)
    {
        // Cursor staat aan het einde - voeg gewoon nieuwe regel toe
        editor.Lines.Add(editor.CurrentInput);
        editor.CurrentInput = "";
    }
    else
    {
        // Cursor staat ergens in het midden - splits de tekst
        string textBeforeCursor = editor.CurrentInput.Substring(0, cursorPosition);
        string textAfterCursor = editor.CurrentInput.Substring(cursorPosition);
        
        // Huidige regel wordt tekst voor cursor
        editor.Lines.Add(textBeforeCursor);
        // Nieuwe regel wordt tekst na cursor
        editor.CurrentInput = textAfterCursor;
    }
    
    cursorPosition = 0;
    editor.CurrentLine = editor.Lines.Count;
    editor.ScrollOffset = Math.Max(0, (editor.Lines.Count) * LINE_HEIGHT - editor.Bounds.Height + LINE_HEIGHT);
}

    public static void SaveCode()
    {
        if (!string.IsNullOrWhiteSpace(editor.CurrentInput))
        {
            editor.Lines.Add(editor.CurrentInput);
            editor.CurrentInput = "";
        }

        if (editor.Lines.Count > 0)
        {
            bool success = FileManager.SaveCodeToFile(editor.Lines);
            if (success)
            {
                statusMessage = "Code saved successfully to saves/code.txt!";
            }
            else
            {
                statusMessage = "Error saving code!";
            }
        }
        else
        {
            statusMessage = "No code to save!";
        }
    }
}
