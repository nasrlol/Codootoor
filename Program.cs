using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq; // Deze was missing!

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
        Raylib.DrawRectangleRec(Bounds, new Color(25, 25, 35, 255));
        Raylib.DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, new Color(60, 60, 80, 255));
        
        DrawLineNumbers();
        DrawCodeLines();
        DrawCurrentInput();
        DrawScrollBar();
    }
    
    private void DrawLineNumbers()
    {
        Raylib.DrawRectangle((int)Bounds.X, (int)Bounds.Y, 40, (int)Bounds.Height, new Color(35, 35, 45, 255));
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
                Color lineColor = i == CurrentLine ? Color.Green : new Color(200, 200, 200, 255);
                
                Raylib.DrawText($"{i + 1}", (int)Bounds.X + 10, (int)yPos, 18, new Color(100, 100, 120, 255));
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
            Raylib.DrawText($"{Lines.Count + 1}:", (int)Bounds.X + 10, (int)currentInputY, 18, new Color(100, 100, 120, 255));
            Raylib.DrawText($"{CurrentInput}_", (int)Bounds.X + 45, (int)currentInputY, 18, Color.White);
        }
    }
    
    private void DrawScrollBar()
    {
        if (Lines.Count * LINE_HEIGHT > Bounds.Height)
        {
            float scrollbarHeight = Bounds.Height * (Bounds.Height / (Lines.Count * LINE_HEIGHT));
            float scrollbarY = Bounds.Y + (ScrollOffset / (Lines.Count * LINE_HEIGHT)) * (Bounds.Height - scrollbarHeight);
            Raylib.DrawRectangle((int)Bounds.X + (int)Bounds.Width - 10, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(100, 100, 120, 255));
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
    
    private int totalLinesWritten = 0;
    private bool hasTypedFirstLetter = false;
    
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
    
    public void DrawAchievementsPanel(int screenWidth, int screenHeight)
    {
        if (!ShowAchievementsPanel) return;
        
        int panelWidth = 400;
        int panelHeight = 500;
        int panelX = (screenWidth - panelWidth) / 2;
        int panelY = (screenHeight - panelHeight) / 2;
        
        Raylib.DrawRectangle(panelX, panelY, panelWidth, panelHeight, new Color(25, 25, 35, 240));
        Raylib.DrawRectangleLines(panelX, panelY, panelWidth, panelHeight, Color.Gold);
        
        Raylib.DrawText("ACHIEVEMENTS", panelX + 100, panelY + 20, 28, Color.Gold);
        
        int yOffset = 70;
        foreach (var achievement in Achievements)
        {
            Color color = achievement.IsUnlocked ? Color.Green : Color.Gray;
            string status = achievement.IsUnlocked ? "UNLOCKED" : "LOCKED";
            
            Raylib.DrawText($"{achievement.Name}", panelX + 20, panelY + yOffset, 20, color);
            Raylib.DrawText(achievement.Description, panelX + 20, panelY + yOffset + 25, 16, color);
            Raylib.DrawText(status, panelX + panelWidth - 100, panelY + yOffset, 18, color);
            
            yOffset += 60;
        }
        
        Raylib.DrawText("Click outside to close", panelX + 100, panelY + panelHeight - 30, 16, Color.Gray);
    }
    
    public void DrawAchievementNotifications(int screenWidth, int screenHeight)
    {
        foreach (var achievement in Achievements)
        {
            if (achievement.DisplayTime > 0)
            {
                float alpha = Math.Clamp(achievement.DisplayTime / 1.0f, 0f, 1f);
                Color bgColor = new Color(0, 100, 0, (int)(200 * alpha));
                Color textColor = new Color(255, 255, 255, (int)(255 * alpha));
                
                int centerX = screenWidth / 2;
                int centerY = screenHeight / 2;
                
                Raylib.DrawRectangle(centerX - 200, centerY - 60, 400, 120, bgColor);
                Raylib.DrawRectangleLines(centerX - 200, centerY - 60, 400, 120, Color.Gold);
                
                Raylib.DrawText("ACHIEVEMENT UNLOCKED!", centerX - 180, centerY - 40, 24, Color.Gold);
                Raylib.DrawText(achievement.Name, centerX - 180, centerY - 10, 32, textColor);
                Raylib.DrawText(achievement.Description, centerX - 180, centerY + 30, 20, textColor);
            }
        }
    }
}

class UIButton
{
    public Rectangle Bounds { get; set; }
    public string Text { get; set; }
    public Color NormalColor { get; set; } = Color.LightGray;
    public Color HoverColor { get; set; } = Color.Gray;
    public Color TextColor { get; set; } = Color.Black;
    
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
        Raylib.DrawRectangleRec(Bounds, color);
        Raylib.DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, Color.DarkGray);
        
        int textWidth = Raylib.MeasureText(Text, 20);
        int textX = (int)Bounds.X + ((int)Bounds.Width - textWidth) / 2;
        Raylib.DrawText(Text, textX, (int)Bounds.Y + 10, 20, TextColor);
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
        Raylib.DrawText("Volume", (int)VisualBounds.X, (int)VisualBounds.Y - 25, 20, Color.White);
        Raylib.DrawRectangleRec(VisualBounds, new Color(60, 60, 80, 255));
        
        float fillHeight = VisualBounds.Height * Volume;
        Raylib.DrawRectangle((int)VisualBounds.X, (int)(VisualBounds.Y + VisualBounds.Height - fillHeight), 
                           (int)VisualBounds.Width, (int)fillHeight, Color.Green);
        
        Raylib.DrawRectangleLines((int)VisualBounds.X, (int)VisualBounds.Y, 
                                (int)VisualBounds.Width, (int)VisualBounds.Height, Color.White);
        
        Raylib.DrawText($"{(int)(Volume * 100)}%", (int)VisualBounds.X + (int)VisualBounds.Width + 10, 
                      (int)VisualBounds.Y, 20, Color.White);
    }
}

class EnvironmentRenderer
{
    public static void DrawHouse(Vector2 position)
    {
        int x = (int)position.X;
        int y = (int)position.Y;
        
        Raylib.DrawRectangle(x - 60, y, 120, 80, Color.Brown);
        Raylib.DrawTriangle(new Vector2(x - 70, y), new Vector2(x + 70, y), new Vector2(x, y - 60), Color.Red);
        Raylib.DrawRectangle(x - 15, y + 20, 30, 60, new Color(101, 67, 33, 255));
        Raylib.DrawCircle(x, y + 50, 3, Color.Gold);
        Raylib.DrawRectangle(x - 45, y + 15, 25, 25, new Color(135, 206, 235, 255));
        Raylib.DrawRectangle(x + 20, y + 15, 25, 25, new Color(135, 206, 235, 255));
        Raylib.DrawRectangleLines(x - 45, y + 15, 25, 25, Color.Black);
        Raylib.DrawRectangleLines(x + 20, y + 15, 25, 25, Color.Black);
        Raylib.DrawLine(x - 32, y + 15, x - 32, y + 40, Color.Black);
        Raylib.DrawLine(x - 45, y + 27, x - 20, y + 27, Color.Black);
        Raylib.DrawLine(x + 33, y + 15, x + 33, y + 40, Color.Black);
        Raylib.DrawLine(x + 20, y + 27, x + 45, y + 27, Color.Black);
        Raylib.DrawText("Stickman\n   Home", x - 40, y + 90, 14, Color.White);
    }
    
    public static void DrawWaterWaves(Rectangle editor)
    {
        int startY = (int)editor.Y + (int)editor.Height + 10;
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

class Program
{
    static int screenWidth = 1200;
    static int screenHeight = 800;
    const int CODE_EDITOR_WIDTH_PERCENT = 58;
    const int CODE_EDITOR_HEIGHT_PERCENT = 75;
    
    static AchievementManager achievementManager;
    static CodeEditor codeEditor;
    static Stickman stickman;
    static UIButton executeButton;
    static UIButton achievementsButton;
    static VolumeSlider volumeSlider;
    
    static Random rand = new Random();
    static bool quickDeliveryActive = false;
    static string quickDeliveryLetter = "";
    static float quickDeliveryTimer = 0;
    static Vector2 quickDeliveryTargetPos;
    static Vector2 letterDropPosition;
    
    static GameState currentState = GameState.Editing;
    static string statusMessage = "Type code in the editor...";
    
    static List<string> currentLineWords = new List<string>();
    static int currentWordIndex = 0;
    static int currentLine = 0;

    static void Main()
    {
        achievementManager = new AchievementManager();
        
        Raylib.InitWindow(screenWidth, screenHeight, "Stickman IDE");
        Raylib.SetWindowState(ConfigFlags.ResizableWindow);
        Raylib.SetTargetFPS(60);

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
        executeButton = new UIButton(CalculateExecuteButton(), "Execute");
        achievementsButton = new UIButton(CalculateAchievementsButton(), "Achievements");
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
        volumeSlider.VisualBounds = CalculateVolumeSlider();
        volumeSlider.ActualBounds = CalculateVolumeSliderActual();
    }

    static void Update()
    {
        Vector2 mousePos = Raylib.GetMousePosition();
        
        volumeSlider.Update();
        codeEditor.HandleScroll(mousePos);
        
        if (achievementsButton.IsMouseOver() && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            achievementManager.ShowAchievementsPanel = !achievementManager.ShowAchievementsPanel;
        }


        if (achievementManager.ShowAchievementsPanel && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            Rectangle achievementsPanel = new Rectangle(
                (screenWidth - 400) / 2,
                (screenHeight - 500) / 2,
                400,
                500
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
        
        float distanceToTarget = Vector2.Distance(stickman.Position, quickDeliveryTargetPos);
        float totalDistance = Vector2.Distance(stickman.OriginalPosition, quickDeliveryTargetPos);
        
        if (distanceToTarget < totalDistance * 0.2f && rand.Next(0, 20) == 0 && !stickman.IsFalling)
        {
            currentState = GameState.Falling;
            letterDropPosition = new Vector2(stickman.Position.X, codeEditor.Bounds.Y + codeEditor.Bounds.Height + 50);
            stickman.StartFall(stickman.Position, letterDropPosition, quickDeliveryLetter);
            statusMessage = "Oh no! Stickman dropped the letter in the water!";
            quickDeliveryActive = false;
        }
        else if (quickDeliveryTimer <= 0 && !stickman.IsFalling)
        {
            quickDeliveryActive = false;
            currentState = GameState.Editing;
            stickman.Reset();
            statusMessage = "Letter delivered!";
        }
    }

    static void UpdateEditingState(Vector2 mousePos)
    {
        codeEditor.HandleInput();
        achievementManager.CheckAchievements(codeEditor.CurrentInput, codeEditor.Lines.Count);

        if (executeButton.IsMouseOver() && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            ExecuteCode();
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
            currentState = GameState.Delivering;
            statusMessage = "Stickman is delivering your code...";
            currentLine = 0;
            currentWordIndex = 0;
            currentLineWords = new List<string>(codeEditor.Lines[0].Split(' '));
            stickman.CurrentWord = currentLineWords[0];
            Console.WriteLine(fullCode);
        }
        else
        {
            statusMessage = "Write some code first!";
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
                codeEditor.Bounds.X + codeEditor.Bounds.Width * 0.3f,
                codeEditor.Bounds.Y + 20 + codeEditor.Lines.Count * 25
            );
            currentState = GameState.QuickDelivery;
            statusMessage = "Quick delivery!";
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
            stickman.Update(currentState, new Vector2(codeEditor.Position.X + codeEditor.Bounds.Width * 0.3f, stickman.Position.Y));
        }

        if (currentState == GameState.Delivering)
        {
            if (stickman.Position.X <= codeEditor.Position.X + codeEditor.Bounds.Width * 0.3f)
            {
                currentState = GameState.Returning;
                statusMessage = $"Delivered: {stickman.CurrentWord}";
                
                currentWordIndex++;
                if (currentWordIndex < currentLineWords.Count)
                {
                    stickman.CurrentWord = currentLineWords[currentWordIndex];
                }
                else
                {
                    currentLine++;
                    if (currentLine < codeEditor.Lines.Count)
                    {
                        currentLineWords = new List<string>(codeEditor.Lines[currentLine].Split(' '));
                        currentWordIndex = 0;
                        stickman.CurrentWord = currentLineWords[0];
                    }
                    else
                    {
                        currentState = GameState.Success;
                        statusMessage = "All code delivered successfully!";
                    }
                }
            }
        }
        else if (currentState == GameState.Returning)
        {
            if (stickman.Position.X >= stickman.OriginalPosition.X)
            {
                if (currentState != GameState.Success)
                {
                    currentState = GameState.Delivering;
                    statusMessage = $"Getting next word: {stickman.CurrentWord}";
                }
            }
        }
        else if (currentState == GameState.Success)
        {
            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                currentState = GameState.Editing;
                stickman.Reset();
                codeEditor.Clear();
                statusMessage = "Type code in the editor...";
            }
        }
    }

    static void Draw()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(new Color(30, 30, 40, 255));
        Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(40, 44, 52, 255));

        codeEditor.Draw();
        EnvironmentRenderer.DrawWaterWaves(codeEditor.Bounds);
        EnvironmentRenderer.DrawHouse(CalculateHousePosition());
        stickman.Draw();

        if (currentState == GameState.Falling)
        {
            EnvironmentRenderer.DrawSplashEffect(letterDropPosition, 1.0f - stickman.FallTimer);
        }

        executeButton.Draw();
        achievementsButton.Draw();
        volumeSlider.Draw();

        DrawStatusMessage();
        DrawCarryingWord();

        achievementManager.DrawAchievementsPanel(screenWidth, screenHeight);
        achievementManager.DrawAchievementNotifications(screenWidth, screenHeight);

        if (currentState == GameState.Success)
        {
            Raylib.DrawText("Press SPACE to write new code", screenWidth / 2 - 200, screenHeight - 100, 22, Color.Green);
        }

        Raylib.EndDrawing();
    }

    static void DrawStatusMessage()
    {
        Color statusColor = currentState switch
        {
            GameState.Success => Color.Green,
            GameState.Falling => Color.Red,
            GameState.QuickDelivery => Color.Yellow,
            _ => new Color(100, 150, 255, 255)
        };
        Raylib.DrawText(statusMessage, 50, 30, 25, statusColor);
    }

    static void DrawCarryingWord()
    {
        if (!string.IsNullOrEmpty(stickman.CurrentWord) && currentState != GameState.Editing && currentState != GameState.Success)
        {
            Raylib.DrawText($"Carrying: {stickman.CurrentWord}", (int)(screenWidth * 0.7f), 30, 20, Color.Yellow);
        }
    }

    static Rectangle CalculateCodeEditor()
    {
        return new Rectangle(
            screenWidth * 0.04f,
            screenHeight * 0.125f,
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
            screenWidth * 0.83f,
            screenHeight * 0.037f,
            120,
            40
        );
    }

    static Rectangle CalculateAchievementsButton()
    {
        return new Rectangle(
            screenWidth * 0.83f,
            screenHeight * 0.10f,
            120,
            40
        );
    }

    static Rectangle CalculateVolumeSlider()
    {
        return new Rectangle(
            screenWidth * 0.83f,
            screenHeight * 0.18f,
            150,
            20
        );
    }

    static Rectangle CalculateVolumeSliderActual()
    {
        return new Rectangle(
            screenWidth * 0.83f,
            screenHeight * 0.173f,
            150,
            30
        );
    }

    static Vector2 CalculateHousePosition()
    {
        return new Vector2(screenWidth * 0.75f, screenHeight * 0.625f);
    }

    static Vector2 CalculateStickmanStartPosition()
    {
        return new Vector2(screenWidth * 0.79f, screenHeight * 0.687f);
    }
}