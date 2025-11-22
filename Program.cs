using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Numerics;

// GameState enum moet buiten alle klassen staan
enum GameState { Editing, Delivering, Returning, Success }

class Stickman
{
    public Vector2 Position { get; set; }
    public Vector2 OriginalPosition { get; set; }
    public string CurrentWord { get; set; }
    public float Speed { get; set; } = 3.0f;
    
    public Stickman(Vector2 startPosition)
    {
        Position = startPosition;
        OriginalPosition = startPosition;
        CurrentWord = "";
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
        else if (state == GameState.Editing || state == GameState.Success)
        {
            Position = OriginalPosition;
        }
    }
    
    public void Draw()
    {
        int x = (int)Position.X;
        int y = (int)Position.Y;
        
        // Hoofd
        Raylib.DrawCircle(x, y - 20, 10, new Color(255, 218, 185, 255));
        
        // Lichaam
        Raylib.DrawLine(x, y - 10, x, y + 20, Color.Blue);
        
        // Armen in dragende positie
        Raylib.DrawLine(x, y, x - 15, y - 5, Color.Blue);
        Raylib.DrawLine(x, y, x + 15, y - 5, Color.Blue);
        
        // Benen met loop animatie
        float walkOffset = (float)Math.Sin(Raylib.GetTime() * 8) * 5;
        Raylib.DrawLine(x, y + 20, x - 15, y + 40 + (int)walkOffset, Color.DarkBlue);
        Raylib.DrawLine(x, y + 20, x + 15, y + 40 - (int)walkOffset, Color.DarkBlue);
        
        // Woord bubble als stickman iets draagt
        if (!string.IsNullOrEmpty(CurrentWord))
        {
            Raylib.DrawRectangle(x - 40, y - 60, 80, 25, Color.White);
            Raylib.DrawRectangleLines(x - 40, y - 60, 80, 25, Color.Black);
            Raylib.DrawText(CurrentWord, x - 35, y - 55, 12, Color.Black);
        }
    }
    
    public void Reset()
    {
        Position = OriginalPosition;
        CurrentWord = "";
    }
}

class Program
{
    static int screenWidth = 1200;
    static int screenHeight = 800;
    const int CODE_EDITOR_WIDTH_PERCENT = 58;
    const int CODE_EDITOR_HEIGHT_PERCENT = 75;
    const int LINE_HEIGHT = 25;
    
    static void Main()
    {
        Raylib.InitWindow(screenWidth, screenHeight, "Stickman IDE");
        Raylib.SetWindowState(ConfigFlags.ResizableWindow);
        Raylib.SetTargetFPS(60);

        // Stickman instantie
        Stickman stickman = new Stickman(CalculateStickmanStartPosition());
        
        // Dynamic layout variabelen
        Rectangle codeEditor = CalculateCodeEditor();
        Rectangle executeButton = CalculateExecuteButton();
        Rectangle volumeSliderVisual = CalculateVolumeSlider();
        Rectangle volumeSliderActual = CalculateVolumeSliderActual();
        Vector2 housePos = CalculateHousePosition();
        Vector2 codeEditorPos = CalculateCodeEditorPosition();
        
        List<string> currentLineWords = new List<string>();
        int currentWordIndex = 0;

        string inputText = "";
        string statusMessage = "Type code in the editor...";
        
        Color executeButtonColor = Color.LightGray;
        
        GameState currentState = GameState.Editing;
        Random rand = new Random();
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
                volumeSliderVisual = CalculateVolumeSlider();
                volumeSliderActual = CalculateVolumeSliderActual();
                housePos = CalculateHousePosition();
                codeEditorPos = CalculateCodeEditorPosition();
                
                // Update stickman position
                stickman.OriginalPosition = CalculateStickmanStartPosition();
                if (currentState == GameState.Editing || currentState == GameState.Success)
                {
                    stickman.Reset();
                }
            }

            Vector2 mousePos = Raylib.GetMousePosition();
            bool mouseOverButton = Raylib.CheckCollisionPointRec(mousePos, executeButton);
            bool mouseOverVolume = Raylib.CheckCollisionPointRec(mousePos, volumeSliderActual);
            
            executeButtonColor = mouseOverButton ? Color.Gray : Color.LightGray;

            if (Raylib.IsMouseButtonDown(MouseButton.Left) && mouseOverVolume)
            {
                float relativeY = mousePos.Y - volumeSliderActual.Y;
                volume = Math.Clamp(1.0f - (relativeY / volumeSliderActual.Height), 0f, 1f);
                Raylib.SetMasterVolume(volume);
            }

            float mouseWheel = Raylib.GetMouseWheelMove();
            if (Raylib.CheckCollisionPointRec(mousePos, codeEditor))
            {
                scrollOffset -= mouseWheel * 20;
                float maxScroll = Math.Max(0, codeLines.Count * LINE_HEIGHT - codeEditor.Height + 50);
                scrollOffset = Math.Clamp(scrollOffset, 0, maxScroll);
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
                        inputText = "";
                    }
                }

                if (mouseOverButton && Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    if (!string.IsNullOrWhiteSpace(inputText))
                    {
                        codeLines.Add(inputText);
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

            // Update stickman
            stickman.Update(currentState, new Vector2(codeEditorPos.X + codeEditor.Width * 0.3f, stickman.Position.Y));

            switch (currentState)
            {
                case GameState.Delivering:
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
                    break;

                case GameState.Returning:
                    if (stickman.Position.X >= stickman.OriginalPosition.X)
                    {
                        if (currentState != GameState.Success)
                        {
                            currentState = GameState.Delivering;
                            statusMessage = $"Getting next word: {stickman.CurrentWord}";
                        }
                    }
                    break;

                case GameState.Success:
                    if (Raylib.IsKeyPressed(KeyboardKey.Space))
                    {
                        currentState = GameState.Editing;
                        stickman.Reset();
                        codeLines.Clear();
                        inputText = "";
                        statusMessage = "Type code in the editor...";
                        scrollOffset = 0;
                    }
                    break;
            }

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

            // Draw stickman
            stickman.Draw();

            Raylib.DrawRectangleRec(executeButton, executeButtonColor);
            Raylib.DrawRectangleLines((int)executeButton.X, (int)executeButton.Y, (int)executeButton.Width, (int)executeButton.Height, Color.DarkGray);
            Raylib.DrawText("Execute", (int)executeButton.X + 15, (int)executeButton.Y + 15, 20, Color.Black);

            Raylib.DrawText("Volume", (int)volumeSliderVisual.X, (int)volumeSliderVisual.Y - 25, 20, Color.White);
            
            Raylib.DrawRectangleRec(volumeSliderVisual, new Color(60, 60, 80, 255));
            
            float fillHeight = volumeSliderVisual.Height * volume;
            Raylib.DrawRectangle((int)volumeSliderVisual.X, (int)(volumeSliderVisual.Y + volumeSliderVisual.Height - fillHeight), 
                               (int)volumeSliderVisual.Width, (int)fillHeight, Color.Green);
            
            Raylib.DrawRectangleLines((int)volumeSliderVisual.X, (int)volumeSliderVisual.Y, 
                                    (int)volumeSliderVisual.Width, (int)volumeSliderVisual.Height, Color.White);
            
            Raylib.DrawText($"{(int)(volume * 100)}%", (int)volumeSliderVisual.X + (int)volumeSliderVisual.Width + 10, 
                          (int)volumeSliderVisual.Y, 20, Color.White);

            Color statusColor = currentState switch
            {
                GameState.Success => Color.Green,
                _ => new Color(100, 150, 255, 255)
            };
            Raylib.DrawText(statusMessage, 50, 30, 25, statusColor);

            if (!string.IsNullOrEmpty(stickman.CurrentWord) && currentState != GameState.Editing && currentState != GameState.Success)
            {
                Raylib.DrawText($"Carrying: {stickman.CurrentWord}", (int)(screenWidth * 0.7f), 30, 20, Color.Yellow);
            }
            
            if (currentState == GameState.Success)
            {
                Raylib.DrawText("Press SPACE to write new code", screenWidth / 2 - 200, screenHeight - 100, 22, Color.Green);
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
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
            screenHeight * 0.125f,
            150,
            20
        );
    }

    static Rectangle CalculateVolumeSliderActual()
    {
        return new Rectangle(
            screenWidth * 0.83f,
            screenHeight * 0.118f,
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
        
        Raylib.DrawTriangle(
            new Vector2(x - 70, y),
            new Vector2(x + 70, y),
            new Vector2(x, y - 60),
            Color.Red
        );
        
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