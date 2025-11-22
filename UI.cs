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
    static Rectangle CalculateCodeEditor()
    {
        return new Rectangle(
            screenWidth * 0.02f,
            screenHeight * 0.12f,
            screenWidth * (CODE_EDITOR_WIDTH_PERCENT / 100f),
            screenHeight * (CODE_EDITOR_HEIGHT_PERCENT / 100f)
        );
    }

    static Vector2 CalculateCodeEditorPosition()
    {
        return new Vector2(screenWidth * 0.08f, screenHeight * 0.187f);
    }

    static Rectangle CalculateExecuteButton()
    {
        return new Rectangle(
            screenWidth * 0.75f,
            screenHeight * 0.15f,
            180,
            40
        );
    }

    static Rectangle CalculateAchievementsButton()
    {
        return new Rectangle(
            screenWidth * 0.75f,
            screenHeight * 0.22f,
            180,
            40
        );
    }

    static Rectangle CalculateClearButton()
    {
        return new Rectangle(
            screenWidth * 0.75f,
            screenHeight * 0.29f,
            180,
            40
        );
    }

    static Rectangle CalculateTipsButton()
    {
        return new Rectangle(
            screenWidth * 0.75f,
            screenHeight * 0.36f,
            180,
            40
        );
    }

    static Rectangle CalculateSaveButton()
    {
        return new Rectangle(
            screenWidth * 0.75f,
            screenHeight * 0.43f,
            180,
            40
        );
    }

    static Rectangle CalculateVolumeSlider()
    {
        return new Rectangle(
            screenWidth * 0.75f,
            screenHeight * 0.57f,
            200,
            20
        );
    }

    static Rectangle CalculateVolumeSliderActual()
    {
        return new Rectangle(
            screenWidth * 0.75f,
            screenHeight * 0.56f,
            200,
            30
        );
    }

    static Vector2 CalculateHousePosition()
    {
        return new Vector2(screenWidth * 0.85f, screenHeight * 0.65f);
    }

    static Vector2 CalculateStickmanStartPosition()
    {
        return new Vector2(screenWidth * 0.89f, screenHeight * 0.71f);
    }
}

class UIButton
{
    public Rectangle Bounds;
    public string Text;
    public Color NormalColor = new Color(80, 100, 150, 255);
    public Color HoverColor = new Color(100, 130, 190, 255);
    public Color TextColor = Color.White;
    public Color BorderColor = new Color(120, 150, 200, 255);
    public bool HasShadow = true;

    public UIButton(Rectangle bounds, string text)
    {
        Bounds = bounds;
        Text = text;
    }

    public bool IsMouseOver()
    {
        return CheckCollisionPointRec(GetMousePosition(), Bounds);
    }

    public void Draw()
    {
        Color color = IsMouseOver() ? HoverColor : NormalColor;

        if (HasShadow)
        {
            DrawRectangle((int)Bounds.X + 3, (int)Bounds.Y + 3, (int)Bounds.Width, (int)Bounds.Height, new Color(0, 0, 0, 100));
        }

        DrawRectangleRec(Bounds, color);
        DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, BorderColor);

        int textWidth = MeasureText(Text, 20);
        int textX = (int)Bounds.X + ((int)Bounds.Width - textWidth) / 2;
        DrawText(Text, textX, (int)Bounds.Y + 12, 20, TextColor);

        if (IsMouseOver())
        {
            DrawRectangleLines((int)Bounds.X - 1, (int)Bounds.Y - 1, (int)Bounds.Width + 2, (int)Bounds.Height + 2, Color.White);
        }
    }
}

class VolumeSlider
{
    public Rectangle VisualBounds;
    public Rectangle ActualBounds;
    public float Volume = 0.5f;

    public VolumeSlider(Rectangle visualBounds, Rectangle actualBounds)
    {
        VisualBounds = visualBounds;
        ActualBounds = actualBounds;
    }

    public void Update()
    {
        Vector2 mousePos = GetMousePosition();
        if (IsMouseButtonDown(MouseButton.Left) && CheckCollisionPointRec(mousePos, ActualBounds))
        {
            float relativeY = mousePos.Y - ActualBounds.Y;
            Volume = Math.Clamp(1.0f - (relativeY / ActualBounds.Height), 0f, 1f);
            SetMasterVolume(Volume);
        }
    }

    public void Draw()
    {
        // Background
        DrawRectangleRec(VisualBounds, new Color(50, 50, 70, 255));
        DrawRectangleLines((int)VisualBounds.X, (int)VisualBounds.Y,
                                (int)VisualBounds.Width, (int)VisualBounds.Height, new Color(100, 100, 120, 255));

        // Fill
        float fillHeight = VisualBounds.Height * Volume;
        Color fillColor = new Color(50, 200, 50, 255);

        DrawRectangle((int)VisualBounds.X, (int)(VisualBounds.Y + VisualBounds.Height - fillHeight),
                           (int)VisualBounds.Width, (int)fillHeight, fillColor);

        // Marker
        float markerY = VisualBounds.Y + VisualBounds.Height - fillHeight;
        DrawRectangle((int)VisualBounds.X - 5, (int)markerY - 2, (int)VisualBounds.Width + 10, 4, Color.White);

        // Text
        DrawText("VOLUME", (int)VisualBounds.X, (int)VisualBounds.Y - 30, 20, Color.White);
        DrawText($"{(int)(Volume * 100)}%", (int)VisualBounds.X + (int)VisualBounds.Width + 15,
                      (int)VisualBounds.Y + (int)VisualBounds.Height / 2 - 10, 20, Color.White);
    }
}

class FileManager
{
    public static bool SaveCodeToFile(List<string> lines, string filename = "code.txt")
    {
        try
        {
            string directory = "saves";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string filePath = Path.Combine(directory, filename);
            File.WriteAllLines(filePath, lines);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving file: {ex.Message}");
            return false;
        }
    }

    public static List<string> LoadCodeFromFile(string filename = "code.txt")
    {
        try
        {
            string filePath = Path.Combine("saves", filename);
            if (File.Exists(filePath))
            {
                return File.ReadAllLines(filePath).ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading file: {ex.Message}");
        }
        return new List<string>();
    }
}

class OutputWindow
{
    public bool IsVisible;
    public string OutputText = "";
    public Rectangle Bounds;
    public float ScrollOffset;
    public Program.Output output;

    public OutputWindow()
    {
        output = new Program.Output();
        output.Init();

        IsVisible = false;
        Bounds = new Rectangle(200, 100, 800, 500);
    }

    public void HandleScroll(Vector2 mousePos)
    {
        if (IsVisible && CheckCollisionPointRec(mousePos, Bounds))
        {
            float mouseWheel = GetMouseWheelMove();
            ScrollOffset -= mouseWheel * 20;
            ScrollOffset = Math.Clamp(ScrollOffset, 0, Math.Max(0, CountLines() * 20 - Bounds.Height + 40));
        }
    }

    public int CountLines()
    {
        return OutputText.Split('\n').Length;
    }

    public void Draw()
    {
        output.Draw(Bounds);

        if (!IsVisible) return;

        // Window background with border
        DrawRectangleRec(Bounds, new Color(20, 20, 30, 255));
        DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, new Color(80, 80, 120, 255));
        DrawRectangleLines((int)Bounds.X - 1, (int)Bounds.Y - 1, (int)Bounds.Width + 2, (int)Bounds.Height + 2, new Color(120, 120, 160, 255));

        // Title bar
        DrawRectangle((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, 30, new Color(40, 40, 60, 255));
        DrawText("PROGRAM OUTPUT", (int)Bounds.X + 10, (int)Bounds.Y + 5, 20, Color.Gold);

        // Close button
        Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 5, 20, 20);
        Color closeColor = CheckCollisionPointRec(GetMousePosition(), closeButton) ? Color.Red : new Color(200, 100, 100, 255);
        DrawRectangleRec(closeButton, closeColor);
        DrawText("X", (int)closeButton.X + 6, (int)closeButton.Y + 2, 16, Color.White);

        // Output content
        string[] lines = OutputText.Split('\n');
        int visibleLines = (int)((Bounds.Height - 40) / 20);
        int startLine = (int)(ScrollOffset / 20);

        for (int i = startLine; i < Math.Min(startLine + visibleLines + 1, lines.Length); i++)
        {
            float yPos = Bounds.Y + 40 + (i - startLine) * 20 - (ScrollOffset % 20);
            DrawText(lines[i], (int)Bounds.X + 10, (int)yPos, 16, Color.White);
        }

        // Scroll bar
        if (CountLines() * 20 > Bounds.Height - 40)
        {
            float scrollbarHeight = (Bounds.Height - 40) * ((Bounds.Height - 40) / (CountLines() * 20));
            float scrollbarY = Bounds.Y + 40 + (ScrollOffset / (CountLines() * 20)) * (Bounds.Height - 40 - scrollbarHeight);

            DrawRectangle((int)Bounds.X + (int)Bounds.Width - 12, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(80, 80, 100, 255));
        }
        output.Draw(Bounds);
    }

    public bool CloseButtonClicked()
    {
        output.Stop();
        if (!IsVisible) return false;

        Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 5, 20, 20);
        return CheckCollisionPointRec(GetMousePosition(), closeButton) && IsMouseButtonPressed(MouseButton.Left);
    }
}

class TipsWindow
{
    public bool IsVisible;
    public Rectangle Bounds;

    public List<string> tips = new List<string>
    {
        "💡 Type letters to trigger quick deliveries",
        "💡 Use 'print \"text\"' to output messages",
        "💡 Stickman can fall! Be careful with timing",
        "💡 Execute code to see program output",
        "💡 Clear the editor to start fresh",
        "💡 Check achievements for your progress",
        "💡 More lines = more coding experience",
        "💡 Quick deliveries help practice typing"
    };

    public TipsWindow()
    {
        IsVisible = false;
        Bounds = new Rectangle(300, 150, 600, 400);
    }

    public void Draw()
    {
        if (!IsVisible) return;

        // Window background
        DrawRectangleRec(Bounds, new Color(30, 30, 45, 255));
        DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, new Color(80, 80, 120, 255));

        // Title
        DrawText("CODING TIPS", (int)Bounds.X + 220, (int)Bounds.Y + 20, 28, Color.Gold);
        DrawLine((int)Bounds.X + 50, (int)Bounds.Y + 60, (int)Bounds.X + 550, (int)Bounds.Y + 60, new Color(80, 80, 120, 255));

        // Tips
        for (int i = 0; i < tips.Count; i++)
        {
            DrawText(tips[i], (int)Bounds.X + 50, (int)Bounds.Y + 80 + i * 40, 18, Color.White);
        }

        // Close button
        Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 15, 20, 20);
        Color closeColor = CheckCollisionPointRec(GetMousePosition(), closeButton) ? Color.Red : new Color(200, 100, 100, 255);
        DrawRectangleRec(closeButton, closeColor);
        DrawText("X", (int)closeButton.X + 6, (int)closeButton.Y + 2, 16, Color.White);
    }

    public bool CloseButtonClicked()
    {
        if (!IsVisible) return false;

        Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 15, 20, 20);
        return CheckCollisionPointRec(GetMousePosition(), closeButton) && IsMouseButtonPressed(MouseButton.Left);
    }
}

class EnvironmentRenderer
{
    public static void DrawHouse(Vector2 position)
    {
        int x = (int)position.X;
        int y = (int)position.Y;

        // Shadow
        DrawRectangle(x - 55, y + 5, 110, 80, new Color(0, 0, 0, 100));

        // Main house - different color from door
        DrawRectangle(x - 60, y, 120, 80, new Color(120, 80, 40, 255)); // Lighter brown for house
        DrawTriangle(new Vector2(x - 70, y), new Vector2(x + 70, y), new Vector2(x, y - 60), new Color(140, 40, 40, 255)); // Darker red roof

        // Door - different color from house
        DrawRectangle(x - 15, y + 20, 30, 60, new Color(80, 50, 20, 255)); // Darker brown for door
        DrawCircle(x, y + 50, 3, Color.Gold);

        // Windows
        DrawWindow(x - 45, y + 15);
        DrawWindow(x + 20, y + 15);

        DrawText("Stickman\n   Home", x - 40, y + 90, 14, Color.White);
    }

    public static void DrawWindow(int x, int y)
    {
        DrawRectangle(x, y, 25, 25, new Color(135, 206, 235, 200));
        DrawRectangleLines(x, y, 25, 25, Color.Black);
        DrawLine(x + 12, y, x + 12, y + 25, Color.Black);
        DrawLine(x, y + 12, x + 25, y + 12, Color.Black);
    }

    public static void DrawWaterWaves(Rectangle editor)
    {
        // Move water higher up - start closer to editor bottom
        int startY = (int)editor.Y + (int)editor.Height - 10; // Moved up by 20 pixels
        for (int i = 0; i < 5; i++)
        {
            int y = startY + i * 8;
            Color waveColor = new Color(30, 144, 255, 100 - i * 15);
            for (int x = (int)editor.X; x < editor.X + editor.Width; x += 20)
            {
                float waveOffset = (float)Math.Sin(GetTime() * 3 + x * 0.1) * 3;
                DrawCircle(x, y + (int)waveOffset, 8, waveColor);
            }
        }
    }

    public static void DrawSplashEffect(Vector2 position, float progress)
    {
        int splashSize = (int)(20 * progress);
        Color splashColor = new Color(255, 255, 255, (int)(150 * (1.0f - progress)));

        DrawCircle((int)position.X, (int)position.Y, splashSize, splashColor);
        DrawCircle((int)position.X - 10, (int)position.Y, splashSize - 5, splashColor);
        DrawCircle((int)position.X + 10, (int)position.Y, splashSize - 5, splashColor);
    }
}
