using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.IO;

enum GameState { Editing, Delivering, Returning, Success, QuickDelivery, Falling }



class Achievement
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsUnlocked { get; set; }
    public Func<bool> CheckCondition { get; set; }
    public float DisplayTime { get; set; }

    public Achievement(string name, string description, Func<bool> condition)
    {
        Name = name;
        Description = description;
        CheckCondition = condition;
        IsUnlocked = false;
        DisplayTime = 0;
    }
}

class Stickman
{
    public Vector2 Position { get; set; }
    public Vector2 OriginalPosition { get; set; }
    public string CurrentWord { get; set; }
    public float Speed { get; set; } = 3.0f;
    public bool IsFalling { get; set; }
    public float FallTimer { get; set; }
    public Vector2 FallStartPosition { get; set; }
    public string FallenLetter { get; set; }
    public Vector2 FallTargetPosition { get; set; }
    
    public Stickman(Vector2 startPosition)
    {
        Position = startPosition;
        OriginalPosition = startPosition;
        CurrentWord = "";
        IsFalling = false;
        FallTimer = 0;
        FallenLetter = "";
        FallTargetPosition = Vector2.Zero;
    }
    
    public void Update(GameState state, Vector2 targetPosition)
    {
        if (state == GameState.Delivering)
        {
            if (Position.X > targetPosition.X)
            {
                Position = new Vector2(Position.X - Speed, Position.Y);
            }
        }
        else if (state == GameState.Returning)
        {
            if (Position.X < OriginalPosition.X)
            {
                Position = new Vector2(Position.X + Speed, Position.Y);
            }
        }
        else if (state == GameState.QuickDelivery)
        {
            float quickSpeed = 15.0f;
            if (Position.X > targetPosition.X)
            {
                Position = new Vector2(Position.X - quickSpeed, Position.Y);
            }
        }
        else if (state == GameState.Falling)
        {
            FallTimer -= Raylib.GetFrameTime();
            if (FallTimer > 0)
            {
                float progress = 1.0f - (FallTimer / 1.0f);
                Position = Vector2.Lerp(FallStartPosition, FallTargetPosition, progress);
            }
        }
        else if (state == GameState.Editing || state == GameState.Success)
        {
            Position = OriginalPosition;
            IsFalling = false;
            FallTimer = 0;
        }
    }
    
    public void StartFall(Vector2 currentPos, Vector2 targetPos, string letter)
    {
        IsFalling = true;
        FallTimer = 1.0f;
        FallStartPosition = currentPos;
        FallTargetPosition = targetPos;
        FallenLetter = letter;
        Position = currentPos;
    }
    
    public void Draw()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        
        if (IsFalling)
        {
            Raylib.DrawCircle(x, y - 20, 10, new Color(255, 218, 185, 255));
            Raylib.DrawLine(x, y - 10, x + 20, y + 10, Color.Blue);
            Raylib.DrawLine(x, y, x - 10, y + 5, Color.Blue);
            Raylib.DrawLine(x, y, x + 15, y - 10, Color.Blue);
            Raylib.DrawLine(x + 20, y + 10, x + 5, y + 30, Color.DarkBlue);
            Raylib.DrawLine(x + 20, y + 10, x + 35, y + 25, Color.DarkBlue);
            
            if (!string.IsNullOrEmpty(FallenLetter))
            {
                Raylib.DrawRectangle(x - 20, y - 40, 40, 20, Color.White);
                Raylib.DrawRectangleLines(x - 20, y - 40, 40, 20, Color.Black);
                Raylib.DrawText(FallenLetter, x - 15, y - 35, 12, Color.Black);
            }
        }
        else
        {
            Raylib.DrawCircle(x, y - 20, 10, new Color(255, 218, 185, 255));
            Raylib.DrawLine(x, y - 10, x, y + 20, Color.Blue);
            Raylib.DrawLine(x, y, x - 15, y - 5, Color.Blue);
            Raylib.DrawLine(x, y, x + 15, y - 5, Color.Blue);
            
            float walkOffset = (float)Math.Sin(Raylib.GetTime() * 8) * 5;
            Raylib.DrawLine(x, y + 20, x - 15, y + 40 + (int)walkOffset, Color.DarkBlue);
            Raylib.DrawLine(x, y + 20, x + 15, y + 40 - (int)walkOffset, Color.DarkBlue);

            if (!string.IsNullOrEmpty(CurrentWord))
            {
                Raylib.DrawRectangle(x - 40, y - 60, 80, 25, Color.White);
                Raylib.DrawRectangleLines(x - 40, y - 60, 80, 25, Color.Black);
                Raylib.DrawText(CurrentWord, x - 35, y - 55, 12, Color.Black);
            }
        }
    }
    
    public void Reset()
    {
        Position = OriginalPosition;
        CurrentWord = "";
        IsFalling = false;
        FallTimer = 0;
        FallenLetter = "";
    }
}

class CodeEditor
{
    public Rectangle Bounds { get; set; }
    public List<string> Lines { get; set; } = new List<string>();
    public string CurrentInput { get; set; } = "";
    public float ScrollOffset { get; set; }
    public int CurrentLine { get; set; }
    public Vector2 Position { get; set; }


    
    private const int LINE_HEIGHT = 25;
    
    public CodeEditor(Rectangle bounds, Vector2 position)
    {
        Bounds = bounds;
        Position = position;
    }

    public void HandleInput()
    {
        // Handle backspace
        if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && CurrentInput.Length > 0)
        {
            CurrentInput = CurrentInput.Substring(0, CurrentInput.Length - 1);
        }

        // Handle enter (both regular and keypad enter)
        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            if (!string.IsNullOrEmpty(CurrentInput))
            {
                Lines.Add(CurrentInput);
                CurrentInput = "";
                CurrentLine = Lines.Count - 1;
                ScrollOffset = Math.Max(0, (Lines.Count - 1) * LINE_HEIGHT - Bounds.Height + LINE_HEIGHT);
            }
        }

        // Handle character input
        int key = Raylib.GetCharPressed();
        if (key > 0)
        {
            char c = (char)key;
            if (char.IsLetterOrDigit(c) || c == ' ' || c == '.' || c == ',' || c == ';' ||
                c == '(' || c == ')' || c == '{' || c == '}' || c == '=' ||
                c == '+' || c == '-' || c == '*' || c == '/')
            {
                CurrentInput += c;
            }
        }
    }



    public void HandleScroll(Vector2 mousePos)
    {
        if (Raylib.CheckCollisionPointRec(mousePos, Bounds))
        {
            float mouseWheel = Raylib.GetMouseWheelMove();
            ScrollOffset -= mouseWheel * 20;
            float maxScroll = Math.Max(0, Lines.Count * LINE_HEIGHT - Bounds.Height + 50);
            ScrollOffset = Math.Clamp(ScrollOffset, 0, maxScroll);
        }
    }
    
    public void Draw()
    {
        // Editor background with solid color
        Raylib.DrawRectangleRec(Bounds, new Color(25, 25, 35, 255));
        
        // Editor border
        Raylib.DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, new Color(60, 60, 80, 255));
        Raylib.DrawRectangleLines((int)Bounds.X - 1, (int)Bounds.Y - 1, (int)Bounds.Width + 2, (int)Bounds.Height + 2, new Color(20, 20, 30, 255));
        
        DrawLineNumbers();
        DrawCodeLines();
        DrawCurrentInput();
        DrawScrollBar();
    }
    
    private void DrawLineNumbers()
    {
        Raylib.DrawRectangle((int)Bounds.X, (int)Bounds.Y, 40, (int)Bounds.Height, new Color(35, 35, 45, 255));
        Raylib.DrawLine((int)Bounds.X + 40, (int)Bounds.Y, (int)Bounds.X + 40, (int)Bounds.Y + (int)Bounds.Height, new Color(60, 60, 80, 255));
    }
    
    private void DrawCodeLines()
    {
        int visibleLines = (int)(Bounds.Height / LINE_HEIGHT);
        int startLine = (int)(ScrollOffset / LINE_HEIGHT);
        int endLine = Math.Min(startLine + visibleLines + 1, Lines.Count);
        
        for (int i = startLine; i < endLine; i++)
        {
            float yPos = Bounds.Y + 20 + (i - startLine) * LINE_HEIGHT - (ScrollOffset % LINE_HEIGHT);
            
            if (yPos >= Bounds.Y && yPos <= Bounds.Y + Bounds.Height - LINE_HEIGHT)
            {
                Color lineColor = new Color(220, 220, 220, 255);
                
                Raylib.DrawText($"{i + 1}", (int)Bounds.X + 10, (int)yPos, 18, new Color(150, 150, 170, 255));
                Raylib.DrawText(Lines[i], (int)Bounds.X + 45, (int)yPos, 18, lineColor);
            }
        }
    }
    
    private void DrawCurrentInput()
    {
        int startLine = (int)(ScrollOffset / LINE_HEIGHT);
        float currentInputY = Bounds.Y + 20 + (Lines.Count - startLine) * LINE_HEIGHT - (ScrollOffset % LINE_HEIGHT);
        
        if (currentInputY >= Bounds.Y && currentInputY <= Bounds.Y + Bounds.Height - LINE_HEIGHT)
        {
            Raylib.DrawText($"{Lines.Count + 1}:", (int)Bounds.X + 10, (int)currentInputY, 18, new Color(150, 150, 170, 255));
            
            // Draw cursor with blinking effect
            string displayText = CurrentInput;
            if ((int)(Raylib.GetTime() * 2) % 2 == 0)
            {
                displayText += "_";
            }
            
            Raylib.DrawText(displayText, (int)Bounds.X + 45, (int)currentInputY, 18, Color.White);
        }
    }
    
    private void DrawScrollBar()
    {
        if (Lines.Count * LINE_HEIGHT > Bounds.Height)
        {
            float scrollbarHeight = Bounds.Height * (Bounds.Height / (Lines.Count * LINE_HEIGHT));
            float scrollbarY = Bounds.Y + (ScrollOffset / (Lines.Count * LINE_HEIGHT)) * (Bounds.Height - scrollbarHeight);
            
            Raylib.DrawRectangle((int)Bounds.X + (int)Bounds.Width - 12, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(80, 80, 100, 255));
            Raylib.DrawRectangleLines((int)Bounds.X + (int)Bounds.Width - 12, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(120, 120, 140, 255));
        }
    }
    
    public void Clear()
    {
        Lines.Clear();
        CurrentInput = "";
        ScrollOffset = 0;
        CurrentLine = 0;
    }


    
}

class AchievementManager
{
    public List<Achievement> Achievements { get; set; } = new List<Achievement>();
    public bool ShowAchievementsPanel { get; set; }
    public float AchievementsScrollOffset { get; set; }
    
    private int totalLinesWritten = 0;
    private bool hasTypedFirstLetter = false;
    private int programsExecuted = 0;
    private int quickDeliveries = 0;
    
    public AchievementManager()
    {
        InitializeAchievements();
    }
    
    private void InitializeAchievements()
    {
        Achievements.Add(new Achievement("First Letter", "Type your first letter", () => hasTypedFirstLetter));
        Achievements.Add(new Achievement("Code Novice", "Write 50 lines of code", () => totalLinesWritten >= 50));
        Achievements.Add(new Achievement("Code Apprentice", "Write 100 lines of code", () => totalLinesWritten >= 100));
        Achievements.Add(new Achievement("Code Journeyman", "Write 250 lines of code", () => totalLinesWritten >= 250));
        Achievements.Add(new Achievement("Code Master", "Write 500 lines of code", () => totalLinesWritten >= 500));
        Achievements.Add(new Achievement("Code Legend", "Write 1000 lines of code", () => totalLinesWritten >= 1000));
        Achievements.Add(new Achievement("First Program", "Execute your first program", () => programsExecuted >= 1));
        Achievements.Add(new Achievement("Productive Programmer", "Execute 5 programs", () => programsExecuted >= 5));
        Achievements.Add(new Achievement("Quick Fingers", "Make 10 quick deliveries", () => quickDeliveries >= 10));
        Achievements.Add(new Achievement("Code Marathon", "Write 2000 lines of code", () => totalLinesWritten >= 2000));
    }
    
    public void CheckAchievements(string inputText, int linesWritten)
    {
        totalLinesWritten = linesWritten;
        
        if (!hasTypedFirstLetter && !string.IsNullOrEmpty(inputText) && inputText.Any(char.IsLetter))
        {
            hasTypedFirstLetter = true;
        }
        
        foreach (var achievement in Achievements)
        {
            if (!achievement.IsUnlocked && achievement.CheckCondition())
            {
                achievement.IsUnlocked = true;
                achievement.DisplayTime = 3.0f;
            }
        }
    }
    
    public void MarkProgramExecuted()
    {
        programsExecuted++;
        CheckAchievements("", totalLinesWritten);
    }
    
    public void MarkQuickDelivery()
    {
        quickDeliveries++;
        CheckAchievements("", totalLinesWritten);
    }
    
    public void UpdateAchievementDisplays()
    {
        foreach (var achievement in Achievements)
        {
            if (achievement.DisplayTime > 0)
            {
                achievement.DisplayTime -= Raylib.GetFrameTime();
            }
        }
    }
    
    public void HandleAchievementsScroll(Vector2 mousePos, Rectangle panelBounds)
    {
        if (Raylib.CheckCollisionPointRec(mousePos, panelBounds))
        {
            float mouseWheel = Raylib.GetMouseWheelMove();
            AchievementsScrollOffset -= mouseWheel * 20;
            float maxScroll = Math.Max(0, Achievements.Count * 65 - panelBounds.Height + 120);
            AchievementsScrollOffset = Math.Clamp(AchievementsScrollOffset, 0, maxScroll);
        }
    }
    
    public void DrawAchievementsPanel(int screenWidth, int screenHeight)
    {
        if (!ShowAchievementsPanel) return;
        
        int panelWidth = 500;
        int panelHeight = 600;
        int panelX = (screenWidth - panelWidth) / 2;
        int panelY = (screenHeight - panelHeight) / 2;

        Rectangle panelBounds = new Rectangle(panelX, panelY, panelWidth, panelHeight);
        
        // Handle scrolling
        Vector2 mousePos = Raylib.GetMousePosition();
        HandleAchievementsScroll(mousePos, panelBounds);
        
        // Panel background
        Raylib.DrawRectangle(panelX - 2, panelY - 2, panelWidth + 4, panelHeight + 4, new Color(0, 0, 0, 100));
        Raylib.DrawRectangle(panelX, panelY, panelWidth, panelHeight, new Color(30, 30, 40, 255));
        Raylib.DrawRectangleLines(panelX, panelY, panelWidth, panelHeight, new Color(80, 60, 120, 255));
        Raylib.DrawRectangleLines(panelX - 1, panelY - 1, panelWidth + 2, panelHeight + 2, new Color(120, 100, 160, 255));
        
        // Title and progress
        int unlockedCount = Achievements.Count(a => a.IsUnlocked);
        Raylib.DrawText("ACHIEVEMENTS", panelX + 150, panelY + 25, 32, Color.Gold);
        Raylib.DrawText($"{unlockedCount}/{Achievements.Count} Unlocked", panelX + 180, panelY + 60, 20, Color.LightGray);
        Raylib.DrawLine(panelX + 50, panelY + 85, panelX + panelWidth - 50, panelY + 85, new Color(80, 60, 120, 255));
        
        // Achievements list with scrolling
        int yOffset = 100;
        int startIndex = (int)(AchievementsScrollOffset / 65);
        int visibleCount = (int)((panelHeight - 120) / 65);
        
        for (int i = startIndex; i < Math.Min(startIndex + visibleCount + 1, Achievements.Count); i++)
        {
            var achievement = Achievements[i];
            float itemY = panelY + yOffset + (i - startIndex) * 65 - (AchievementsScrollOffset % 65);
            
            if (itemY >= panelY + 100 && itemY <= panelY + panelHeight - 50)
            {
                Color bgColor = achievement.IsUnlocked ? new Color(60, 100, 60, 100) : new Color(60, 60, 60, 100);
                Color borderColor = achievement.IsUnlocked ? new Color(100, 200, 100, 255) : new Color(100, 100, 100, 255);
                Color textColor = achievement.IsUnlocked ? new Color(144, 238, 144, 255) : Color.LightGray;
                Color descColor = achievement.IsUnlocked ? new Color(200, 255, 200, 255) : new Color(180, 180, 180, 255);
                string status = achievement.IsUnlocked ? "UNLOCKED" : "LOCKED";
                Color statusColor = achievement.IsUnlocked ? Color.Gold : Color.Gray;
                
                // Achievement item background
                Raylib.DrawRectangle(panelX + 20, (int)itemY, panelWidth - 40, 50, bgColor);
                Raylib.DrawRectangleLines(panelX + 20, (int)itemY, panelWidth - 40, 50, borderColor);
                
                Raylib.DrawText($"{achievement.Name}", panelX + 35, (int)itemY + 5, 20, textColor);
                Raylib.DrawText(achievement.Description, panelX + 35, (int)itemY + 28, 14, descColor);
                Raylib.DrawText(status, panelX + panelWidth - 120, (int)itemY + 15, 16, statusColor);
            }
        }
        
        // Scroll bar for achievements
        if (Achievements.Count * 65 > panelHeight - 120)
        {
            float scrollbarHeight = (panelHeight - 120) * ((panelHeight - 120) / (Achievements.Count * 65));
            float scrollbarY = panelY + 100 + (AchievementsScrollOffset / (Achievements.Count * 65)) * (panelHeight - 120 - scrollbarHeight);
            
            Raylib.DrawRectangle(panelX + panelWidth - 20, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(80, 80, 100, 255));
            Raylib.DrawRectangleLines(panelX + panelWidth - 20, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(120, 120, 140, 255));
        }
        
        // Close hint
        Raylib.DrawText("Press ESC or click outside to close", panelX + 120, panelY + panelHeight - 30, 16, Color.Gray);
    }
    
    public void DrawAchievementNotifications(int screenWidth, int screenHeight)
    {
        foreach (var achievement in Achievements)
        {
            if (achievement.DisplayTime > 0)
            {
                float alpha = Math.Clamp(achievement.DisplayTime / 1.0f, 0f, 1f);
                Color bgColor = new Color(40, 80, 40, (int)(220 * alpha));
                Color borderColor = new Color(120, 200, 120, (int)(255 * alpha));
                Color textColor = new Color(255, 255, 255, (int)(255 * alpha));
                Color goldColor = new Color(255, 215, 0, (int)(255 * alpha));
                
                int centerX = screenWidth / 2;
                int centerY = screenHeight / 3;
                
                // Notification background with shadow
                Raylib.DrawRectangle(centerX - 210, centerY - 70, 420, 140, new Color(0, 0, 0, (int)(100 * alpha)));
                Raylib.DrawRectangle(centerX - 200, centerY - 60, 400, 120, bgColor);
                Raylib.DrawRectangleLines(centerX - 200, centerY - 60, 400, 120, borderColor);
                
                Raylib.DrawText("ACHIEVEMENT UNLOCKED!", centerX - 120, centerY - 35, 22, goldColor);
                Raylib.DrawText(achievement.Name, centerX - 120, centerY - 5, 28, textColor);
                Raylib.DrawText(achievement.Description, centerX - 120, centerY + 25, 18, textColor);
            }
        }
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
    
    public bool IsMouseOver()
    {
        return Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), Bounds);
    }
    
    public void Draw()
    {
        Color color = IsMouseOver() ? HoverColor : NormalColor;
        
        if (HasShadow)
        {
            Raylib.DrawRectangle((int)Bounds.X + 3, (int)Bounds.Y + 3, (int)Bounds.Width, (int)Bounds.Height, new Color(0, 0, 0, 100));
        }
        
        Raylib.DrawRectangleRec(Bounds, color);
        Raylib.DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, BorderColor);
        
        int textWidth = Raylib.MeasureText(Text, 20);
        int textX = (int)Bounds.X + ((int)Bounds.Width - textWidth) / 2;
        Raylib.DrawText(Text, textX, (int)Bounds.Y + 12, 20, TextColor);
        
        if (IsMouseOver())
        {
            Raylib.DrawRectangleLines((int)Bounds.X - 1, (int)Bounds.Y - 1, (int)Bounds.Width + 2, (int)Bounds.Height + 2, Color.White);
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
        }
    }
    
    public void Draw()
    {
        // Background
        Raylib.DrawRectangleRec(VisualBounds, new Color(50, 50, 70, 255));
        Raylib.DrawRectangleLines((int)VisualBounds.X, (int)VisualBounds.Y, 
                                (int)VisualBounds.Width, (int)VisualBounds.Height, new Color(100, 100, 120, 255));
        
        // Fill
        float fillHeight = VisualBounds.Height * Volume;
        Color fillColor = new Color(50, 200, 50, 255);
        
        Raylib.DrawRectangle((int)VisualBounds.X, (int)(VisualBounds.Y + VisualBounds.Height - fillHeight), 
                           (int)VisualBounds.Width, (int)fillHeight, fillColor);
        
        // Marker
        float markerY = VisualBounds.Y + VisualBounds.Height - fillHeight;
        Raylib.DrawRectangle((int)VisualBounds.X - 5, (int)markerY - 2, (int)VisualBounds.Width + 10, 4, Color.White);
        
        // Text
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
    
    public OutputWindow()
    {
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
        
        // Window background with border
        Raylib.DrawRectangleRec(Bounds, new Color(20, 20, 30, 255));
        Raylib.DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, new Color(80, 80, 120, 255));
        Raylib.DrawRectangleLines((int)Bounds.X - 1, (int)Bounds.Y - 1, (int)Bounds.Width + 2, (int)Bounds.Height + 2, new Color(120, 120, 160, 255));
        
        // Title bar
        Raylib.DrawRectangle((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, 30, new Color(40, 40, 60, 255));
        Raylib.DrawText("PROGRAM OUTPUT", (int)Bounds.X + 10, (int)Bounds.Y + 5, 20, Color.Gold);
        
        // Close button
        Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 5, 20, 20);
        Color closeColor = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), closeButton) ? Color.Red : new Color(200, 100, 100, 255);
        Raylib.DrawRectangleRec(closeButton, closeColor);
        Raylib.DrawText("X", (int)closeButton.X + 6, (int)closeButton.Y + 2, 16, Color.White);
        
        // Output content
        string[] lines = OutputText.Split('\n');
        int visibleLines = (int)((Bounds.Height - 40) / 20);
        int startLine = (int)(ScrollOffset / 20);
        
        for (int i = startLine; i < Math.Min(startLine + visibleLines + 1, lines.Length); i++)
        {
            float yPos = Bounds.Y + 40 + (i - startLine) * 20 - (ScrollOffset % 20);
            Raylib.DrawText(lines[i], (int)Bounds.X + 10, (int)yPos, 16, Color.White);
        }
        
        // Scroll bar
        if (CountLines() * 20 > Bounds.Height - 40)
        {
            float scrollbarHeight = (Bounds.Height - 40) * ((Bounds.Height - 40) / (CountLines() * 20));
            float scrollbarY = Bounds.Y + 40 + (ScrollOffset / (CountLines() * 20)) * (Bounds.Height - 40 - scrollbarHeight);
            
            Raylib.DrawRectangle((int)Bounds.X + (int)Bounds.Width - 12, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(80, 80, 100, 255));
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
        Raylib.DrawRectangleRec(Bounds, new Color(30, 30, 45, 255));
        Raylib.DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, new Color(80, 80, 120, 255));
        
        // Title
        Raylib.DrawText("CODING TIPS", (int)Bounds.X + 220, (int)Bounds.Y + 20, 28, Color.Gold);
        Raylib.DrawLine((int)Bounds.X + 50, (int)Bounds.Y + 60, (int)Bounds.X + 550, (int)Bounds.Y + 60, new Color(80, 80, 120, 255));
        
        // Tips
        for (int i = 0; i < tips.Count; i++)
        {
            Raylib.DrawText(tips[i], (int)Bounds.X + 50, (int)Bounds.Y + 80 + i * 40, 18, Color.White);
        }
        
        // Close button
        Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 15, 20, 20);
        Color closeColor = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), closeButton) ? Color.Red : new Color(200, 100, 100, 255);
        Raylib.DrawRectangleRec(closeButton, closeColor);
        Raylib.DrawText("X", (int)closeButton.X + 6, (int)closeButton.Y + 2, 16, Color.White);
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
        
        // Shadow
        Raylib.DrawRectangle(x - 55, y + 5, 110, 80, new Color(0, 0, 0, 100));
        
        // Main house - different color from door
        Raylib.DrawRectangle(x - 60, y, 120, 80, new Color(120, 80, 40, 255)); // Lighter brown for house
        Raylib.DrawTriangle(new Vector2(x - 70, y), new Vector2(x + 70, y), new Vector2(x, y - 60), new Color(140, 40, 40, 255)); // Darker red roof
        
        // Door - different color from house
        Raylib.DrawRectangle(x - 15, y + 20, 30, 60, new Color(80, 50, 20, 255)); // Darker brown for door
        Raylib.DrawCircle(x, y + 50, 3, Color.Gold);
        
        // Windows
        DrawWindow(x - 45, y + 15);
        DrawWindow(x + 20, y + 15);
        
        Raylib.DrawText("Stickman\n   Home", x - 40, y + 90, 14, Color.White);
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
        // Move water higher up - start closer to editor bottom
        int startY = (int)editor.Y + (int)editor.Height - 10; // Moved up by 20 pixels
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

// Simple code interpreter
class CodeInterpreter
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
}

class Program
{
    static int screenWidth = 1400;
    static int screenHeight = 900;
    const int CODE_EDITOR_WIDTH_PERCENT = 70;
    const int CODE_EDITOR_HEIGHT_PERCENT = 85;
    
    static AchievementManager achievementManager = new AchievementManager();
    static CodeEditor codeEditor;
    static Stickman stickman;
    static UIButton executeButton;
    static UIButton achievementsButton;
    static UIButton clearButton;
    static UIButton tipsButton;
    static UIButton saveButton;
    static VolumeSlider volumeSlider;
    static OutputWindow outputWindow = new OutputWindow();
    static TipsWindow tipsWindow = new TipsWindow();
    
    static Random rand = new Random();
    static bool quickDeliveryActive = false;
    static string quickDeliveryLetter = "";
    static float quickDeliveryTimer = 0;
    static Vector2 quickDeliveryTargetPos;
    static Vector2 letterDropPosition;
    
    static GameState currentState = GameState.Editing;
    static string statusMessage = "Welcome to Stickman IDE! Type code to begin...";
    static int lettersDelivered = 0;

    static void Main()
    {
        Raylib.InitWindow(screenWidth, screenHeight, "Stickman IDE - Code Delivery Adventure");
        Raylib.SetWindowState(ConfigFlags.ResizableWindow);
        Raylib.SetTargetFPS(60);
        Raylib.SetExitKey(KeyboardKey.Null);

        InitializeComponents();
        
        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsWindowResized())
            {
                screenWidth = Raylib.GetScreenWidth();
                screenHeight = Raylib.GetScreenHeight();
                UpdateComponentPositions();
            }

            Update();
            Draw();
        }

        Raylib.CloseWindow();
    }

    static void InitializeComponents()
    {
        codeEditor = new CodeEditor(CalculateCodeEditor(), CalculateCodeEditorPosition());
        stickman = new Stickman(CalculateStickmanStartPosition());
        executeButton = new UIButton(CalculateExecuteButton(), "Execute Code");
        achievementsButton = new UIButton(CalculateAchievementsButton(), "Achievements");
        clearButton = new UIButton(CalculateClearButton(), "Clear Code");
        tipsButton = new UIButton(CalculateTipsButton(), "Tips");
        saveButton = new UIButton(CalculateSaveButton(), "Save Code");
        volumeSlider = new VolumeSlider(CalculateVolumeSlider(), CalculateVolumeSliderActual());
    }

    static void UpdateComponentPositions()
    {
        codeEditor.Bounds = CalculateCodeEditor();
        codeEditor.Position = CalculateCodeEditorPosition();
        stickman.OriginalPosition = CalculateStickmanStartPosition();
        if (currentState == GameState.Editing || currentState == GameState.Success)
        {
            stickman.Reset();
        }
        executeButton.Bounds = CalculateExecuteButton();
        achievementsButton.Bounds = CalculateAchievementsButton();
        clearButton.Bounds = CalculateClearButton();
        tipsButton.Bounds = CalculateTipsButton();
        saveButton.Bounds = CalculateSaveButton();
        volumeSlider.VisualBounds = CalculateVolumeSlider();
        volumeSlider.ActualBounds = CalculateVolumeSliderActual();
        outputWindow.Bounds = new Rectangle(screenWidth / 2 - 400, screenHeight / 2 - 250, 800, 500);
        tipsWindow.Bounds = new Rectangle(screenWidth / 2 - 300, screenHeight / 2 - 200, 600, 400);
    }

    static void Update()
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        
        // Handle ESC for panels
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            achievementManager.ShowAchievementsPanel = false;
            outputWindow.IsVisible = false;
            tipsWindow.IsVisible = false;
        }
        
        // Handle F1 for tips
        if (Raylib.IsKeyPressed(KeyboardKey.F1))
        {
            tipsWindow.IsVisible = !tipsWindow.IsVisible;
        }
        
        volumeSlider.Update();
        codeEditor.HandleScroll(mousePos);
        outputWindow.HandleScroll(mousePos);
        
        // FIXED: Achievements button - simplified click detection
        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            if (achievementsButton.IsMouseOver())
            {
                achievementManager.ShowAchievementsPanel = !achievementManager.ShowAchievementsPanel;
                Console.WriteLine("Achievements button clicked!"); // Debug line
            }
            else if (clearButton.IsMouseOver())
            {
                codeEditor.Clear();
                statusMessage = "Code editor cleared!";
            }
            else if (tipsButton.IsMouseOver())
            {
                tipsWindow.IsVisible = !tipsWindow.IsVisible;
            }
            else if (executeButton.IsMouseOver())
            {
                ExecuteCode();
            }
            else if (saveButton.IsMouseOver())
            {
                SaveCode();
            }
        }

        // Close buttons for windows
        if (outputWindow.CloseButtonClicked())
        {
            outputWindow.IsVisible = false;
        }

        if (tipsWindow.CloseButtonClicked())
        {
            tipsWindow.IsVisible = false;
        }

        // Close achievements panel when clicking outside
        if (achievementManager.ShowAchievementsPanel && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Rectangle achievementsPanel = new Rectangle(
                (screenWidth - 500) / 2,
                (screenHeight - 600) / 2,
                500,
                600
            );
            
            if (!Raylib.CheckCollisionPointRec(mousePos, achievementsPanel) && 
                !Raylib.CheckCollisionPointRec(mousePos, achievementsButton.Bounds))
            {
                achievementManager.ShowAchievementsPanel = false;
            }
        }

        if (quickDeliveryActive)
        {
            UpdateQuickDelivery();
        }

        if (currentState == GameState.Editing)
        {
            UpdateEditingState(mousePos);
        }

        UpdateStickman();
        achievementManager.UpdateAchievementDisplays();
    }

    static void UpdateQuickDelivery()
    {
        quickDeliveryTimer -= Raylib.GetFrameTime();
        
        if (stickman.Position.X <= quickDeliveryTargetPos.X + 5f)
        {
            quickDeliveryActive = false;
            currentState = GameState.Editing;
            stickman.Reset();
            statusMessage = "Quick delivery successful!";
            lettersDelivered++;
            achievementManager.MarkQuickDelivery(); // Track quick deliveries for achievements
        }
        else if (quickDeliveryTimer <= 0)
        {
            quickDeliveryActive = false;
            currentState = GameState.Editing;
            stickman.Reset();
            statusMessage = "Quick delivery timed out!";
        }
        
        // Fall chance - 5% chance to fall
        float distanceToTarget = Vector2.Distance(stickman.Position, quickDeliveryTargetPos);
        float totalDistance = Vector2.Distance(stickman.OriginalPosition, quickDeliveryTargetPos);
        
        if (distanceToTarget < totalDistance * 0.2f && rand.Next(0, 20) == 0 && !stickman.IsFalling)
        {
            currentState = GameState.Falling;
            letterDropPosition = new Vector2(stickman.Position.X, codeEditor.Bounds.Y + codeEditor.Bounds.Height + 30);
            stickman.StartFall(stickman.Position, letterDropPosition, quickDeliveryLetter);
            statusMessage = "Oh no! Stickman dropped the letter in the water!";
            quickDeliveryActive = false;
        }
    }

    static void UpdateEditingState(Vector2 mousePos)
    {
        string previousInput = codeEditor.CurrentInput;
        
        codeEditor.HandleInput();
        achievementManager.CheckAchievements(codeEditor.CurrentInput, codeEditor.Lines.Count);

        if (codeEditor.CurrentInput.Length > previousInput.Length && 
            char.IsLetter(codeEditor.CurrentInput[^1]) && 
            !quickDeliveryActive && 
            currentState == GameState.Editing)
        {
            StartQuickDeliveryForLetters();
        }

        //StartQuickDeliveryForLetters();
    }

    // function to write code
    static void ExecuteCode()
    {
        string fullCode = string.Join("\n", codeEditor.Lines) +
                                 (string.IsNullOrEmpty(codeEditor.CurrentInput) ? "" : "\n" + codeEditor.CurrentInput);

        if (!string.IsNullOrWhiteSpace(codeEditor.CurrentInput))
        {
            codeEditor.Lines.Add(codeEditor.CurrentInput);
            codeEditor.CurrentInput = "";
        }
        
        if (fullCode.Length > 0)
        {
            outputWindow.OutputText = CodeInterpreter.ExecuteCode(codeEditor.Lines);
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

    static void SaveCode()
    {
        if (!string.IsNullOrWhiteSpace(codeEditor.CurrentInput))
        {
            codeEditor.Lines.Add(codeEditor.CurrentInput);
            codeEditor.CurrentInput = "";
        }

        if (codeEditor.Lines.Count > 0)
        {
            bool success = FileManager.SaveCodeToFile(codeEditor.Lines);
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

    static void StartQuickDeliveryForLetters()
    {
        if (!string.IsNullOrEmpty(codeEditor.CurrentInput) && 
            char.IsLetter(codeEditor.CurrentInput[^1]) && 
            !quickDeliveryActive && 
            currentState == GameState.Editing)
        {
            quickDeliveryActive = true;
            quickDeliveryLetter = codeEditor.CurrentInput[^1].ToString();
            quickDeliveryTimer = 1.5f;
            
            quickDeliveryTargetPos = new Vector2(
                codeEditor.Bounds.X + 100f,
                codeEditor.Bounds.Y + 50f
            );
            
            currentState = GameState.QuickDelivery;
            statusMessage = "Quick delivery! Stickman is running...";
            
            stickman.Position = stickman.OriginalPosition;
            stickman.CurrentWord = quickDeliveryLetter;
        }
    }

    static void UpdateStickman()
    {
        if (currentState == GameState.QuickDelivery)
        {
            stickman.Update(currentState, quickDeliveryTargetPos);
        }
        else if (currentState == GameState.Falling)
        {
            stickman.Update(currentState, Vector2.Zero);
            if (stickman.FallTimer <= 0)
            {
                currentState = GameState.Editing;
                stickman.Reset();
                statusMessage = "The letter sank in the water!";
            }
        }
        else
        {
            stickman.Update(currentState, Vector2.Zero);
        }
    }

    static void Draw()
    {
        Raylib.BeginDrawing();
        
        // Background
        Raylib.ClearBackground(new Color(20, 20, 30, 255));
        
        // Draw header
        DrawHeader();
        
        codeEditor.Draw();
        EnvironmentRenderer.DrawWaterWaves(codeEditor.Bounds);
        EnvironmentRenderer.DrawHouse(CalculateHousePosition());
        stickman.Draw();

        if (currentState == GameState.Falling)
        {
            EnvironmentRenderer.DrawSplashEffect(letterDropPosition, 1.0f - stickman.FallTimer);
        }

        // Draw UI elements
        executeButton.Draw();
        achievementsButton.Draw();
        clearButton.Draw();
        tipsButton.Draw();
        saveButton.Draw();
        volumeSlider.Draw();

        DrawStatusMessage();

        // Draw windows
        outputWindow.Draw();
        tipsWindow.Draw();
        achievementManager.DrawAchievementsPanel(screenWidth, screenHeight);
        achievementManager.DrawAchievementNotifications(screenWidth, screenHeight);

        Raylib.EndDrawing();
    }

    static void DrawHeader()
    {
        // Header background
        Raylib.DrawRectangle(0, 0, screenWidth, 60, new Color(40, 40, 60, 255));
        Raylib.DrawRectangle(0, 60, screenWidth, 2, new Color(80, 60, 120, 255));
        
        // Title
        Raylib.DrawText("STICKMAN IDE", screenWidth / 2 - 150, 10, 36, Color.White);
        Raylib.DrawText("Code Delivery Adventure", screenWidth / 2 - 120, 45, 18, new Color(200, 180, 255, 255));
    }

    static void DrawStatusMessage()
    {
        Color statusColor = currentState switch
        {
            GameState.Success => Color.Green,
            GameState.Falling => Color.Red,
            GameState.QuickDelivery => Color.Yellow,
            _ => new Color(100, 200, 255, 255)
        };
        
        Raylib.DrawText("Status: " + statusMessage, 20, 70, 20, statusColor);
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