using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

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
            // Supersnelle beweging voor letter delivery
            float quickSpeed = 15.0f;
            if (Position.X > targetPosition.X)
            {
                Position = new Vector2(Position.X - quickSpeed, Position.Y);
            }
        }
        else if (state == GameState.Falling)
        {
            // Val animatie - beweeg naar beneden op dezelfde X positie
            FallTimer -= Raylib.GetFrameTime();
            if (FallTimer > 0)
            {
                // Lineaire interpolatie van start naar target positie
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
        FallTimer = 1.0f; // 1 seconde valtijd
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
            // Vallende stickman - horizontaal
            Raylib.DrawCircle(x, y - 20, 10, new Color(255, 218, 185, 255));
            Raylib.DrawLine(x, y - 10, x + 20, y + 10, Color.Blue); // Schuin lichaam
            Raylib.DrawLine(x, y, x - 10, y + 5, Color.Blue);
            Raylib.DrawLine(x, y, x + 15, y - 10, Color.Blue);
            Raylib.DrawLine(x + 20, y + 10, x + 5, y + 30, Color.DarkBlue);
            Raylib.DrawLine(x + 20, y + 10, x + 35, y + 25, Color.DarkBlue);
            
            // Vallende letter
            if (!string.IsNullOrEmpty(FallenLetter))
            {
                Raylib.DrawRectangle(x - 20, y - 40, 40, 20, Color.White);
                Raylib.DrawRectangleLines(x - 20, y - 40, 40, 20, Color.Black);
                Raylib.DrawText(FallenLetter, x - 15, y - 35, 12, Color.Black);
            }
        }
        else
        {
            // Normale stickman
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

class Program
{
    static int screenWidth = 1200;
    static int screenHeight = 800;
    const int CODE_EDITOR_WIDTH_PERCENT = 58;
    const int CODE_EDITOR_HEIGHT_PERCENT = 75;
    const int LINE_HEIGHT = 25;
    
    static List<Achievement> achievements = new List<Achievement>();
    static int totalLinesWritten = 0;
    static bool hasTypedFirstLetter = false;
    static bool showAchievements = false;
    static Random rand = new Random();

    // Quick delivery variabelen
    static bool quickDeliveryActive = false;
    static string quickDeliveryLetter = "";
    static float quickDeliveryTimer = 0;
    static Vector2 quickDeliveryTargetPos;
    static Vector2 letterDropPosition; // Positie waar de letter in het water valt

    static void Main()
    {
        InitializeAchievements();
        
        Raylib.InitWindow(screenWidth, screenHeight, "Stickman IDE");
        Raylib.SetWindowState(ConfigFlags.ResizableWindow);
        Raylib.SetTargetFPS(60);

        Stickman stickman = new Stickman(CalculateStickmanStartPosition());
        Rectangle codeEditor = CalculateCodeEditor();
        Rectangle executeButton = CalculateExecuteButton();
        Rectangle achievementsButton = CalculateAchievementsButton();
        Rectangle volumeSliderVisual = CalculateVolumeSlider();
        Rectangle volumeSliderActual = CalculateVolumeSliderActual();
        Vector2 housePos = CalculateHousePosition();
        Vector2 codeEditorPos = CalculateCodeEditorPosition();
        
        List<string> currentLineWords = new List<string>();
        int currentWordIndex = 0;

        string inputText = "";
        string statusMessage = "Type code in the editor...";
        
        Color executeButtonColor = Color.LightGray;
        Color achievementsButtonColor = Color.LightGray;
        
        GameState currentState = GameState.Editing;
        List<string> codeLines = new List<string>();
        int currentLine = 0;
        float scrollOffset = 0;
        float volume = 0.5f;

        while (!Raylib.WindowShouldClose())
        {
            if (Raylib.IsWindowResized())
            {
                screenWidth = Raylib.GetScreenWidth();
                screenHeight = Raylib.GetScreenHeight();
                
                codeEditor = CalculateCodeEditor();
                executeButton = CalculateExecuteButton();
                achievementsButton = CalculateAchievementsButton();
                volumeSliderVisual = CalculateVolumeSlider();
                volumeSliderActual = CalculateVolumeSliderActual();
                housePos = CalculateHousePosition();
                codeEditorPos = CalculateCodeEditorPosition();
                
                stickman.OriginalPosition = CalculateStickmanStartPosition();
                if (currentState == GameState.Editing || currentState == GameState.Success)
                {
                    stickman.Reset();
                }
            }

            Vector2 mousePos = Raylib.GetMousePosition();
            bool mouseOverExecute = Raylib.CheckCollisionPointRec(mousePos, executeButton);
            bool mouseOverAchievements = Raylib.CheckCollisionPointRec(mousePos, achievementsButton);
            bool mouseOverVolume = Raylib.CheckCollisionPointRec(mousePos, volumeSliderActual);
            
            executeButtonColor = mouseOverExecute ? Color.Gray : Color.LightGray;
            achievementsButtonColor = mouseOverAchievements ? Color.Gray : Color.LightGray;

            if (Raylib.IsMouseButtonDown(MouseButton.Left) && mouseOverVolume)
            {
                float relativeY = mousePos.Y - volumeSliderActual.Y;
                volume = Math.Clamp(1.0f - (relativeY / volumeSliderActual.Height), 0f, 1f);
                Raylib.SetMasterVolume(volume);
            }

            // Achievements button
            if (mouseOverAchievements && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                showAchievements = !showAchievements;
            }

            // Close achievements when clicking outside
            if (showAchievements && Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                Rectangle achievementsPanel = new Rectangle(
                    (screenWidth - 400) / 2,
                    (screenHeight - 500) / 2,
                    400,
                    500
                );
                
                if (!Raylib.CheckCollisionPointRec(mousePos, achievementsPanel) && 
                    !Raylib.CheckCollisionPointRec(mousePos, achievementsButton))
                {
                    showAchievements = false;
                }
            }

            float mouseWheel = Raylib.GetMouseWheelMove();
            if (Raylib.CheckCollisionPointRec(mousePos, codeEditor))
            {
                scrollOffset -= mouseWheel * 20;
                float maxScroll = Math.Max(0, codeLines.Count * LINE_HEIGHT - codeEditor.Height + 50);
                scrollOffset = Math.Clamp(scrollOffset, 0, maxScroll);
            }

            // Quick delivery update
            if (quickDeliveryActive)
            {
                quickDeliveryTimer -= Raylib.GetFrameTime();
                
                // Check of stickman bijna bij de target is (80% van de weg)
                float distanceToTarget = Vector2.Distance(stickman.Position, quickDeliveryTargetPos);
                float totalDistance = Vector2.Distance(stickman.OriginalPosition, quickDeliveryTargetPos);
                
                if (distanceToTarget < totalDistance * 0.2f && rand.Next(0, 20) == 0 && !stickman.IsFalling)
                {
                    // Start val - 5% kans
                    currentState = GameState.Falling;
                    letterDropPosition = new Vector2(stickman.Position.X, codeEditor.Y + codeEditor.Height + 50);
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

            if (currentState == GameState.Editing)
            {
                int key = Raylib.GetCharPressed();
                while (key > 0)
                {
                    char c = (char)key;
                    if (char.IsLetterOrDigit(c) || c == ' ' || c == '.' || c == ',' || c == ';' || 
                        c == '(' || c == ')' || c == '{' || c == '}' || c == '=' || 
                        c == '+' || c == '-' || c == '*' || c == '/')
                    {
                        inputText += c;
                        
                        // Check first letter achievement
                        if (!hasTypedFirstLetter && char.IsLetter(c))
                        {
                            hasTypedFirstLetter = true;
                            CheckAchievements();
                        }
                        
                        // Start quick delivery voor letters
                        if (char.IsLetter(c) && !quickDeliveryActive && currentState == GameState.Editing)
                        {
                            quickDeliveryActive = true;
                            quickDeliveryLetter = c.ToString();
                            quickDeliveryTimer = 1.5f; // Iets langer voor betere animatie
                            quickDeliveryTargetPos = new Vector2(
                                codeEditor.X + codeEditor.Width * 0.3f,
                                codeEditor.Y + 20 + codeLines.Count * LINE_HEIGHT
                            );
                            currentState = GameState.QuickDelivery;
                            statusMessage = "Quick delivery!";
                        }
                    }
                    key = Raylib.GetCharPressed();
                }

                if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && inputText.Length > 0)
                    inputText = inputText.Substring(0, inputText.Length - 1);

                if (Raylib.IsKeyPressed(KeyboardKey.Enter))
                {
                    if (!string.IsNullOrWhiteSpace(inputText))
                    {
                        codeLines.Add(inputText);
                        totalLinesWritten++;
                        CheckAchievements();
                        inputText = "";
                    }
                }

                if (mouseOverExecute && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    if (!string.IsNullOrWhiteSpace(inputText))
                    {
                        codeLines.Add(inputText);
                        totalLinesWritten++;
                        CheckAchievements();
                        inputText = "";
                    }
                    
                    if (codeLines.Count > 0)
                    {
                        currentState = GameState.Delivering;
                        statusMessage = "Stickman is delivering your code...";
                        currentLine = 0;
                        currentWordIndex = 0;
                        currentLineWords = new List<string>(codeLines[0].Split(' '));
                        stickman.CurrentWord = currentLineWords[0];
                    }
                    else
                    {
                        statusMessage = "Write some code first!";
                    }
                }
            }

            // Update stickman based on state
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
                    
                    // Splash effect in het water
                    CreateSplashEffect(letterDropPosition);
                }
            }
            else
            {
                stickman.Update(currentState, new Vector2(codeEditorPos.X + codeEditor.Width * 0.3f, stickman.Position.Y));
            }

            // Normale delivery logic
            if (currentState == GameState.Delivering)
            {
                if (stickman.Position.X <= codeEditorPos.X + codeEditor.Width * 0.3f)
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
                        if (currentLine < codeLines.Count)
                        {
                            currentLineWords = new List<string>(codeLines[currentLine].Split(' '));
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
                    codeLines.Clear();
                    inputText = "";
                    statusMessage = "Type code in the editor...";
                    scrollOffset = 0;
                }
            }

            // Update achievement display times
            UpdateAchievementDisplays();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(30, 30, 40, 255));

            Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(40, 44, 52, 255));

            Raylib.DrawRectangleRec(codeEditor, new Color(25, 25, 35, 255));
            Raylib.DrawRectangleLines((int)codeEditor.X, (int)codeEditor.Y, (int)codeEditor.Width, (int)codeEditor.Height, new Color(60, 60, 80, 255));
            
            DrawWaterWaves(codeEditor);
            
            DrawHouse(housePos);
            
            Raylib.DrawRectangle((int)codeEditor.X, (int)codeEditor.Y, 40, (int)codeEditor.Height, new Color(35, 35, 45, 255));
            
            int visibleLines = (int)(codeEditor.Height / LINE_HEIGHT);
            int startLine = (int)(scrollOffset / LINE_HEIGHT);
            int endLine = Math.Min(startLine + visibleLines + 1, codeLines.Count);
            
            for (int i = startLine; i < endLine; i++)
            {
                float yPos = codeEditor.Y + 20 + (i - startLine) * LINE_HEIGHT - (scrollOffset % LINE_HEIGHT);
                
                if (yPos >= codeEditor.Y && yPos <= codeEditor.Y + codeEditor.Height - LINE_HEIGHT)
                {
                    Color lineColor = i == currentLine && currentState == GameState.Delivering ? Color.Green : new Color(200, 200, 200, 255);
                    
                    Raylib.DrawText($"{i + 1}", (int)codeEditor.X + 10, (int)yPos, 18, new Color(100, 100, 120, 255));
                    Raylib.DrawText(codeLines[i], (int)codeEditor.X + 45, (int)yPos, 18, lineColor);
                }
            }

            float currentInputY = codeEditor.Y + 20 + (codeLines.Count - startLine) * LINE_HEIGHT - (scrollOffset % LINE_HEIGHT);
            if (currentInputY >= codeEditor.Y && currentInputY <= codeEditor.Y + codeEditor.Height - LINE_HEIGHT)
            {
                Raylib.DrawText($"{codeLines.Count + 1}:", (int)codeEditor.X + 10, (int)currentInputY, 18, new Color(100, 100, 120, 255));
                Raylib.DrawText($"{inputText}_", (int)codeEditor.X + 45, (int)currentInputY, 18, Color.White);
            }

            if (codeLines.Count * LINE_HEIGHT > codeEditor.Height)
            {
                float scrollbarHeight = codeEditor.Height * (codeEditor.Height / (codeLines.Count * LINE_HEIGHT));
                float scrollbarY = codeEditor.Y + (scrollOffset / (codeLines.Count * LINE_HEIGHT)) * (codeEditor.Height - scrollbarHeight);
                Raylib.DrawRectangle((int)codeEditor.X + (int)codeEditor.Width - 10, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(100, 100, 120, 255));
            }

            stickman.Draw();

            // Draw splash effect if letter fell in water
            if (currentState == GameState.Falling)
            {
                DrawSplashEffect(letterDropPosition, 1.0f - stickman.FallTimer);
            }

            // Execute button
            Raylib.DrawRectangleRec(executeButton, executeButtonColor);
            Raylib.DrawRectangleLines((int)executeButton.X, (int)executeButton.Y, (int)executeButton.Width, (int)executeButton.Height, Color.DarkGray);
            Raylib.DrawText("Execute", (int)executeButton.X + 15, (int)executeButton.Y + 15, 20, Color.Black);

            // Achievements button
            Raylib.DrawRectangleRec(achievementsButton, achievementsButtonColor);
            Raylib.DrawRectangleLines((int)achievementsButton.X, (int)achievementsButton.Y, (int)achievementsButton.Width, (int)achievementsButton.Height, Color.DarkGray);
            Raylib.DrawText("Achievements", (int)achievementsButton.X + 10, (int)achievementsButton.Y + 15, 16, Color.Black);

            // Volume slider
            Raylib.DrawText("Volume", (int)volumeSliderVisual.X, (int)volumeSliderVisual.Y - 25, 20, Color.White);
            Raylib.DrawRectangleRec(volumeSliderVisual, new Color(60, 60, 80, 255));
            float fillHeight = volumeSliderVisual.Height * volume;
            Raylib.DrawRectangle((int)volumeSliderVisual.X, (int)(volumeSliderVisual.Y + volumeSliderVisual.Height - fillHeight), 
                               (int)volumeSliderVisual.Width, (int)fillHeight, Color.Green);
            Raylib.DrawRectangleLines((int)volumeSliderVisual.X, (int)volumeSliderVisual.Y, 
                                    (int)volumeSliderVisual.Width, (int)volumeSliderVisual.Height, Color.White);
            Raylib.DrawText($"{(int)(volume * 100)}%", (int)volumeSliderVisual.X + (int)volumeSliderVisual.Width + 10, 
                          (int)volumeSliderVisual.Y, 20, Color.White);

            // Status message
            Color statusColor = currentState switch
            {
                GameState.Success => Color.Green,
                GameState.Falling => Color.Red,
                GameState.QuickDelivery => Color.Yellow,
                _ => new Color(100, 150, 255, 255)
            };
            Raylib.DrawText(statusMessage, 50, 30, 25, statusColor);

            if (!string.IsNullOrEmpty(stickman.CurrentWord) && currentState != GameState.Editing && currentState != GameState.Success)
            {
                Raylib.DrawText($"Carrying: {stickman.CurrentWord}", (int)(screenWidth * 0.7f), 30, 20, Color.Yellow);
            }

            // Draw achievements panel if open
            if (showAchievements)
            {
                DrawAchievementsPanel();
            }

            // Draw achievement notifications
            DrawAchievementNotifications();

            if (currentState == GameState.Success)
            {
                Raylib.DrawText("Press SPACE to write new code", screenWidth / 2 - 200, screenHeight - 100, 22, Color.Green);
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static void CreateSplashEffect(Vector2 position)
    {
        // Hier kun je later een splash particle effect toevoegen
    }

    static void DrawSplashEffect(Vector2 position, float progress)
    {
        // Eenvoudige splash animatie
        int splashSize = (int)(20 * progress);
        Color splashColor = new Color(255, 255, 255, (int)(150 * (1.0f - progress)));
        
        Raylib.DrawCircle((int)position.X, (int)position.Y, splashSize, splashColor);
        Raylib.DrawCircle((int)position.X - 10, (int)position.Y, splashSize - 5, splashColor);
        Raylib.DrawCircle((int)position.X + 10, (int)position.Y, splashSize - 5, splashColor);
    }

    static void InitializeAchievements()
    {
        achievements.Add(new Achievement("First Letter", "Type your first letter", () => hasTypedFirstLetter));
        achievements.Add(new Achievement("Code Novice", "Write 50 lines of code", () => totalLinesWritten >= 50));
        achievements.Add(new Achievement("Code Apprentice", "Write 100 lines of code", () => totalLinesWritten >= 100));
        achievements.Add(new Achievement("Code Journeyman", "Write 250 lines of code", () => totalLinesWritten >= 250));
        achievements.Add(new Achievement("Code Master", "Write 500 lines of code", () => totalLinesWritten >= 500));
        achievements.Add(new Achievement("Code Legend", "Write 1000 lines of code", () => totalLinesWritten >= 1000));
    }

    static void CheckAchievements()
    {
        foreach (var achievement in achievements)
        {
            if (!achievement.IsUnlocked && achievement.CheckCondition())
            {
                achievement.IsUnlocked = true;
                achievement.DisplayTime = 3.0f;
            }
        }
    }

    static void UpdateAchievementDisplays()
    {
        foreach (var achievement in achievements)
        {
            if (achievement.DisplayTime > 0)
            {
                achievement.DisplayTime -= Raylib.GetFrameTime();
            }
        }
    }

    static void DrawAchievementNotifications()
    {
        foreach (var achievement in achievements)
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

    static void DrawAchievementsPanel()
    {
        int panelWidth = 400;
        int panelHeight = 500;
        int panelX = (screenWidth - panelWidth) / 2;
        int panelY = (screenHeight - panelHeight) / 2;
        
        // Background
        Raylib.DrawRectangle(panelX, panelY, panelWidth, panelHeight, new Color(25, 25, 35, 240));
        Raylib.DrawRectangleLines(panelX, panelY, panelWidth, panelHeight, Color.Gold);
        
        // Title
        Raylib.DrawText("ACHIEVEMENTS", panelX + 100, panelY + 20, 28, Color.Gold);
        
        // Achievements list
        int yOffset = 70;
        foreach (var achievement in achievements)
        {
            Color color = achievement.IsUnlocked ? Color.Green : Color.Gray;
            string status = achievement.IsUnlocked ? "UNLOCKED" : "LOCKED";
            
            Raylib.DrawText($"{achievement.Name}", panelX + 20, panelY + yOffset, 20, color);
            Raylib.DrawText(achievement.Description, panelX + 20, panelY + yOffset + 25, 16, color);
            Raylib.DrawText(status, panelX + panelWidth - 100, panelY + yOffset, 18, color);
            
            yOffset += 60;
        }
        
        // Close hint
        Raylib.DrawText("Click outside to close", panelX + 100, panelY + panelHeight - 30, 16, Color.Gray);
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

    static Rectangle CalculateCodeEditor()
    {
        return new Rectangle(
            screenWidth * 0.04f,
            screenHeight * 0.125f,
            screenWidth * (CODE_EDITOR_WIDTH_PERCENT / 100f),
            screenHeight * (CODE_EDITOR_HEIGHT_PERCENT / 100f)
        );
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

    static Vector2 CalculateCodeEditorPosition()
    {
        return new Vector2(screenWidth * 0.08f, screenHeight * 0.187f);
    }

    static void DrawHouse(Vector2 position)
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

    static void DrawWaterWaves(Rectangle editor)
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
}