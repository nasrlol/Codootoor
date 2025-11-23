namespace Odootoor;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.IO;


public partial class Program
{

    class ThemeToggle
    {
        public Rectangle Bounds { get; set; }

        public ThemeToggle(Rectangle bounds)
        {
            Bounds = bounds;
        }

        public void Update()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mousePos, Bounds))
            {
                if (!ThemeManager.IsLightMode)
                {
                    ThemeManager.StartThemeSwitch();
                }
            }
        }

        public void Draw()
        {
            // Toggle background
            Color bgColor = ThemeManager.IsLightMode ? new Color(200, 200, 220, 255) : new Color(50, 50, 70, 255);
            Raylib.DrawRectangleRec(Bounds, bgColor);
            Raylib.DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, ThemeManager.GetBorderColor());

            // Toggle knob
            int knobSize = (int)Bounds.Height - 8;
            int knobX = ThemeManager.IsLightMode ? (int)(Bounds.X + Bounds.Width - knobSize - 4) : (int)(Bounds.X + 4);

            Raylib.DrawRectangle(knobX, (int)Bounds.Y + 4, knobSize, knobSize, ThemeManager.GetAccentColor());

            // Labels
            DrawTextEx(regular_font, "THEME", new Vector2((int)Bounds.X, (int)Bounds.Y - 25), 20, 0, ThemeManager.GetTextColor());
            DrawTextEx(regular_font, "D", new Vector2((int)Bounds.X + 5, (int)Bounds.Y + 7), 18, 0, Color.White);
            DrawTextEx(regular_font, "L", new Vector2((int)Bounds.X + (int)Bounds.Width - 25, (int)Bounds.Y + 7), 18, 0, Color.Black);

        }

    }
    class UIButton
    {
        public Rectangle Bounds { get; set; }
        public string Text { get; set; }
        public Color NormalColor { get; set; } = new Color(80, 100, 150, 255);
        public Color HoverColor { get; set; } = new Color(100, 130, 190, 255);
        public Color TextColor { get; set; } = Color.White;
        public Color BorderColor { get; set; } = new Color(120, 150, 200, 255);
        public bool HasShadow { get; set; } = true;

        public UIButton(Rectangle bounds, string text)
        {
            Bounds = bounds;
            Text = text;
        }

        public void Draw(bool Hovered)
        {
            Color color = Hovered ? HoverColor : NormalColor;

            if (HasShadow)
            {
                DrawRectangle((int)Bounds.X + 3, (int)Bounds.Y + 3, (int)Bounds.Width, (int)Bounds.Height, new Color(0, 0, 0, 100));
            }

            DrawRectangleRec(Bounds, color);
            DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, BorderColor);

            int textWidth = MeasureText(Text, 20);
            int textX = (int)Bounds.X + ((int)Bounds.Width - textWidth) / 2;
            DrawTextEx(regular_font, Text, new Vector2(textX - 15, (int)Bounds.Y + 12), font_size - 12, spacing, TextColor);


            if (Hovered)
            {
                DrawRectangleLines((int)Bounds.X - 1, (int)Bounds.Y - 1, (int)Bounds.Width + 2, (int)Bounds.Height + 2, Color.White);
            }
        }
    }
    class VolumeSlider
    {
        public Rectangle VisualBounds { get; set; }
        public Rectangle ActualBounds { get; set; }
        public float Volume { get; set; } = 0.5f;

        public VolumeSlider(Rectangle visualBounds, Rectangle actualBounds)
        {
            VisualBounds = visualBounds;
            ActualBounds = actualBounds;
        }

        public void Update()
        {
            Vector2 mousePos = Raylib.GetMousePosition();
            if (Raylib.IsMouseButtonDown(MouseButton.Left) && Raylib.CheckCollisionPointRec(mousePos, ActualBounds))
            {
                float relativeY = mousePos.Y - ActualBounds.Y;
                Volume = Math.Clamp(1.0f - (relativeY / ActualBounds.Height), 0f, 1f);
                Raylib.SetMasterVolume(Volume);

                // Update muziek volume als het geladen is
                if (MusicManager.isLoaded)
                {
                    Raylib.SetMusicVolume(MusicManager.BackgroundMusic, Volume);
                }
            }
        }

    public void Draw()
    {
        Raylib.DrawRectangleRec(VisualBounds, new Color(50, 50, 70, 255));
        Raylib.DrawRectangleLines((int)VisualBounds.X, (int)VisualBounds.Y, 
                (int)VisualBounds.Width, (int)VisualBounds.Height, new Color(100, 100, 120, 255));

            float fillHeight = VisualBounds.Height * Volume;
            Color fillColor = new Color(50, 200, 50, 255);

        Raylib.DrawRectangle((int)VisualBounds.X, (int)(VisualBounds.Y + VisualBounds.Height - fillHeight), 
                (int)VisualBounds.Width, (int)fillHeight, fillColor);

            float markerY = VisualBounds.Y + VisualBounds.Height - fillHeight;
            Raylib.DrawRectangle((int)VisualBounds.X - 5, (int)markerY - 2, (int)VisualBounds.Width + 10, 4, Color.White);

        Raylib.DrawText("VOLUME", (int)VisualBounds.X, (int)VisualBounds.Y - 30, 20, Color.White);
        Raylib.DrawText($"{(int)(Volume * 100)}%", (int)VisualBounds.X + (int)VisualBounds.Width + 15, 
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
        public bool IsVisible { get; set; }
        public string OutputText { get; set; } = "";
        public Rectangle Bounds { get; set; }
        public float ScrollOffset { get; set; }
        public Program.Piper piper;

        public OutputWindow()
        {
            piper = new();
            IsVisible = false;
            Bounds = new Rectangle(200, 100, 800, 500);
        }

        public void HandleScroll(Vector2 mousePos)
        {
            if (IsVisible && Raylib.CheckCollisionPointRec(mousePos, Bounds))
            {
                float mouseWheel = Raylib.GetMouseWheelMove();
                ScrollOffset -= mouseWheel * 20;
                ScrollOffset = Math.Clamp(ScrollOffset, 0, Math.Max(0, CountLines() * 20 - Bounds.Height + 40));
            }
        }

        private int CountLines()
        {
            return OutputText.Split('\n').Length;
        }

        public void Draw()
        {
            if (!IsVisible) return;

            DrawRectangleRec(Bounds, ThemeManager.GetPanelBackground());
            DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, ThemeManager.GetAccentColor());
            DrawRectangleLines((int)Bounds.X - 1, (int)Bounds.Y - 1, (int)Bounds.Width + 2, (int)Bounds.Height + 2, ThemeManager.GetLightAccentColor());

            DrawRectangle((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, 30, ThemeManager.GetHeaderColor());
            DrawTextEx(regular_font, "PROGRAM OUTPUT", new Vector2((int)Bounds.X + 10, (int)Bounds.Y + 5), 20, spacing, Color.Gold);

            Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 5, 20, 20);
            Color closeColor = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), closeButton) ? Color.Red : new Color(200, 100, 100, 255);
            DrawRectangleRec(closeButton, closeColor);
            DrawTextEx(regular_font, "X", new Vector2((int)closeButton.X, (int)closeButton.Y + 2), font_size, spacing, Color.White);

            string[] lines = OutputText.Split('\n');
            int visibleLines = (int)((Bounds.Height - 40) / 20);
            int startLine = (int)(ScrollOffset / 20);

            for (int i = startLine; i < Math.Min(startLine + visibleLines + 1, lines.Length); i++)
            {
                float yPos = Bounds.Y + 40 + (i - startLine) * 20 - (ScrollOffset % 20);
                DrawTextEx(regular_font, lines[i], new Vector2((int)Bounds.X + 10, (int)yPos), 16, spacing, ThemeManager.GetTextColor());
            }

            if (CountLines() * 20 > Bounds.Height - 40)
            {
                float scrollbarHeight = (Bounds.Height - 40) * ((Bounds.Height - 40) / (CountLines() * 20));
                float scrollbarY = Bounds.Y + 40 + (ScrollOffset / (CountLines() * 20)) * (Bounds.Height - 40 - scrollbarHeight);

                DrawRectangle((int)Bounds.X + (int)Bounds.Width - 12, (int)scrollbarY, 8, (int)scrollbarHeight, ThemeManager.GetScrollbarColor());
            }


        lock (piper.OutputBuffer)
        {
            OutputText = string.Join("\n", piper.OutputBuffer);
        }
    }

    public bool CloseButtonClicked()
    {
        if (!IsVisible) return false;

        Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 5, 20, 20);
        return Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), closeButton) && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }
}

    class TipsWindow
    {
        public bool IsVisible { get; set; }
        public Rectangle Bounds { get; set; }

        private List<string> tips = new List<string>
        {
            "Type letters to trigger quick deliveries",
            "Use 'print \"text\"' to output messages",
            "Stickman can fall! Be careful with timing",
            "Execute code to see program output",
            "Clear the editor to start fresh",
            "Check achievements for your progress",
            "More lines = more coding experience",
            "Quick deliveries help practice typing"
        };

        public TipsWindow()
        {
            IsVisible = false;
            Bounds = new Rectangle(300, 150, 600, 400);
        }

        public void Draw()
        {
            if (!IsVisible) return;

            DrawRectangleRec(Bounds, ThemeManager.GetPanelBackground());
            DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, ThemeManager.GetAccentColor());

            DrawTextEx(regular_font, "CODING TIPS", new Vector2((int)Bounds.X + 220, (int)Bounds.Y + 20), 28, spacing, Color.Gold);
            DrawLine((int)Bounds.X + 50, (int)Bounds.Y + 60, (int)Bounds.X + 550, (int)Bounds.Y + 60, ThemeManager.GetAccentColor());

            for (int i = 0; i < tips.Count; i++)
            {
                DrawTextEx(regular_font, tips[i], new Vector2((int)Bounds.X + 50, (int)Bounds.Y + 80 + i * 40), 18, spacing - 3, ThemeManager.GetTextColor());
            }

            Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 15, 20, 20);
            Color closeColor = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), closeButton) ? Color.Red : new Color(200, 100, 100, 255);
            DrawRectangleRec(closeButton, closeColor);
            DrawTextEx(regular_font, "X", new Vector2((int)closeButton.X, (int)closeButton.Y + 2), font_size, 0, Color.White);
        }

        public bool CloseButtonClicked()
        {
            if (!IsVisible) return false;

            Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 15, 20, 20);
            return Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), closeButton) && Raylib.IsMouseButtonPressed(MouseButton.Left);
        }
    }

    class EnvironmentRenderer
    {
        public static void DrawHouse(Vector2 position)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            Raylib.DrawRectangle(x - 55, y + 5, 110, 80, new Color(0, 0, 0, 100));

            Raylib.DrawRectangle(x - 60, y, 120, 80, new Color(120, 80, 40, 255));
            Raylib.DrawTriangle(new Vector2(x - 70, y), new Vector2(x + 70, y), new Vector2(x, y - 60), new Color(140, 40, 40, 255));

            Raylib.DrawRectangle(x - 15, y + 20, 30, 60, new Color(80, 50, 20, 255));
            Raylib.DrawCircle(x, y + 50, 3, Color.Gold);

            DrawWindow(x - 45, y + 15);
            DrawWindow(x + 20, y + 15);
        }

        private static void DrawWindow(int x, int y)
        {
            Raylib.DrawRectangle(x, y, 25, 25, new Color(135, 206, 235, 200));
            Raylib.DrawRectangleLines(x, y, 25, 25, Color.Black);
            Raylib.DrawLine(x + 12, y, x + 12, y + 25, Color.Black);
            Raylib.DrawLine(x, y + 12, x + 25, y + 12, Color.Black);
        }

        public static void DrawWaterWaves(Rectangle editor)
        {
            int startY = (int)editor.Y + (int)editor.Height - 10;
            for (int i = 0; i < 5; i++)
            {
                int y = startY + i * 8;
                Color waveColor = new Color(30, 144, 255, 100 - i * 15);
                for (int x = (int)editor.X; x < editor.X + editor.Width; x += 20)
                {
                    float waveOffset = (float)Math.Sin(Raylib.GetTime() * 3 + x * 0.1) * 3;
                    Raylib.DrawCircle(x, y + (int)waveOffset, 8, waveColor);
                }
            }
        }

        public static void DrawSplashEffect(Vector2 position, float progress)
        {
            int splashSize = (int)(20 * progress);
            Color splashColor = new Color(255, 255, 255, (int)(150 * (1.0f - progress)));

            Raylib.DrawCircle((int)position.X, (int)position.Y, splashSize, splashColor);
            Raylib.DrawCircle((int)position.X - 10, (int)position.Y, splashSize - 5, splashColor);
            Raylib.DrawCircle((int)position.X + 10, (int)position.Y, splashSize - 5, splashColor);
        }
    }

    class ThemeManager
    {
        public static bool IsLightMode { get; private set; } = false;
        public static int ConfirmationLevel { get; private set; } = 0;
        public static bool ShowThemePopup { get; set; } = false;

        private static float transitionProgress = 0f;
        private static bool isTransitioning = false;

        public static void Update()
        {
            if (isTransitioning)
            {
                transitionProgress += Raylib.GetFrameTime() * 2f;
                if (transitionProgress >= 1f)
                {
                    transitionProgress = 1f;
                    isTransitioning = false;
                }
            }
        }

        public static void StartThemeSwitch()
        {
            if (!IsLightMode && !isTransitioning)
            {
                ConfirmationLevel++;
                ShowThemePopup = true;

                if (ConfirmationLevel >= 3)
                {
                    isTransitioning = true;
                    transitionProgress = 0f;
                    IsLightMode = true;
                    ShowThemePopup = false;
                    ConfirmationLevel = 0;
                }
            }
        }

        public static void CancelThemeSwitch()
        {
            ShowThemePopup = false;
            ConfirmationLevel = 0;
        }

        // Kleur methodes
        public static Color GetBackgroundColor()
        {
            if (!IsLightMode) return new Color(20, 20, 30, 255);
            return Color.White;
        }

        public static Color GetEditorBackground()
        {
            if (!IsLightMode) return new Color(25, 25, 35, 255);
            return new Color(250, 250, 255, 255);
        }

        public static Color GetTextColor()
        {
            if (!IsLightMode) return Color.White;
            return Color.Black;
        }

        public static Color GetLineNumberColor()
        {
            if (!IsLightMode) return new Color(150, 150, 170, 255);
            return new Color(100, 100, 120, 255);
        }

        public static Color GetSidebarColor()
        {
            if (!IsLightMode) return new Color(35, 35, 45, 255);
            return new Color(240, 240, 245, 255);
        }

        public static Color GetBorderColor()
        {
            if (!IsLightMode) return new Color(60, 60, 80, 255);
            return new Color(180, 180, 200, 255);
        }

        public static Color GetDarkBorderColor()
        {
            if (!IsLightMode) return new Color(20, 20, 30, 255);
            return new Color(150, 150, 170, 255);
        }

        public static Color GetScrollbarColor()
        {
            if (!IsLightMode) return new Color(80, 80, 100, 255);
            return new Color(200, 200, 220, 255);
        }

        public static Color GetAccentColor()
        {
            if (!IsLightMode) return new Color(80, 60, 120, 255);
            return new Color(255, 200, 50, 255);
        }

        public static Color GetLightAccentColor()
        {
            if (!IsLightMode) return new Color(120, 100, 160, 255);
            return new Color(255, 220, 100, 255);
        }

        public static Color GetPanelBackground()
        {
            if (!IsLightMode) return new Color(30, 30, 40, 255);
            return new Color(245, 245, 250, 255);
        }

        public static Color GetHeaderColor()
        {
            if (!IsLightMode) return new Color(40, 40, 60, 255);
            return new Color(230, 230, 240, 255);
        }

        public static void DrawThemePopup(int screenWidth, int screenHeight)
        {
            if (!ShowThemePopup) return;

            string[] messages = {
                "Are you SURE you want to use Light Mode?",
                "Are you VERY sure? This is irreversible!"
            };

            string[] buttonTexts = {
                "Yes, I'm brave!",
                "I accept!"
            };

            int currentMessage = ConfirmationLevel - 1;
            if (currentMessage < 0 || currentMessage >= messages.Length) return;

            // Donker overlay
            Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, 180));

            // Popup background
            int popupWidth = 600;
            int popupHeight = 300;
            int popupX = (screenWidth - popupWidth) / 2;
            int popupY = (screenHeight - popupHeight) / 2;

            DrawRectangle(popupX, popupY, popupWidth, popupHeight, new Color(40, 40, 60, 255));
            DrawRectangleLines(popupX, popupY, popupWidth, popupHeight, new Color(120, 100, 160, 255));
            DrawRectangleLines(popupX - 2, popupY - 2, popupWidth + 4, popupHeight + 4, new Color(160, 140, 200, 255));

            // Warning icon
            DrawTextEx(regular_font, "!", new Vector2(popupX + 275, popupY + 40), 60, 0, Color.Yellow);

            // Message
            DrawTextEx(regular_font, messages[currentMessage], new Vector2((int)popupX + 50, (int)popupY + 120), font_size, spacing, Color.White);

            // Yes button
            Rectangle yesButton = new Rectangle(popupX + 150, popupY + 200, 150, 50);
            bool yesHover = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), yesButton);

            DrawRectangleRec(yesButton, yesHover ? new Color(100, 200, 100, 255) : new Color(60, 160, 60, 255));
            DrawRectangleLines((int)yesButton.X, (int)yesButton.Y, (int)yesButton.Width, (int)yesButton.Height, new Color(120, 220, 120, 255));
            DrawTextEx(regular_font, messages[currentMessage], new Vector2((int)popupX + 50, (int)popupY + 120), font_size, spacing, Color.White);

            // No button
            Rectangle noButton = new Rectangle(popupX + 320, popupY + 200, 150, 50);
            bool noHover = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), noButton);

            DrawRectangleRec(noButton, noHover ? new Color(200, 100, 100, 255) : new Color(160, 60, 60, 255));
            DrawRectangleLines((int)noButton.X, (int)noButton.Y, (int)noButton.Width, (int)noButton.Height, new Color(220, 120, 120, 255));
            DrawTextEx(regular_font, "NO, SAVE ME!", new Vector2((int)noButton.X + 25, (int)noButton.Y), font_size, spacing, Color.White);

            // Handle clicks
            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                if (yesHover)
                {
                    StartThemeSwitch();
                }
                else if (noHover)
                {
                    CancelThemeSwitch();
                }
            }
        }
    }

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
                screenWidth * 0.75f + 70,
                screenHeight * 0.57f,
                30,
                200
                );
    }

    static Rectangle CalculateVolumeSliderActual()
    {
        return new Rectangle(
                screenWidth * 0.75f + 70,
                screenHeight * 0.56f,
                30,
                200
                );
    }

    static Rectangle CalculateThemeToggle()
    {
        return new Rectangle(
                screenWidth * 0.75f,
                screenHeight * 0.65f,
                80,
                30
                );
    }

    static Vector2 CalculateHousePosition()
    {
        return new Vector2(screenWidth * 0.78f, screenHeight * 0.88f);
    }

    static Vector2 CalculateStickmanStartPosition()
    {
        return new Vector2(screenWidth * 0.84f, screenHeight * 0.92f);
    }
}
