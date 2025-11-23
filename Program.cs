using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace Odootoor;

enum GameState { Editing, Moving }

partial class Program
{
    // font 
    static int font_size = 31;
    static float spacing = 5f;


    static string regular_font_path = "assets/JetBrainsMono-Bold.ttf";
    static string extra_bold_font_path = "assets/JetBrainsMono-ExtraBold.ttf";
    static Font regular_font;
    static Font extra_bold_font;

    const bool DEBUGDisableDeliveries = true;

    static int screenWidth = 1400;
    static int screenHeight = 900;
    const int CODE_EDITOR_WIDTH_PERCENT = 70;
    const int CODE_EDITOR_HEIGHT_PERCENT = 85;

    static AchievementManager achievementManager = new AchievementManager();
    static UIButton executeButton;
    static UIButton achievementsButton;
    static ThemeToggle themeToggle; // VOEG DIT TOE
    static UIButton clearButton;
    static UIButton tipsButton;
    static UIButton saveButton;
    static VolumeSlider volumeSlider;
    static OutputWindow outputWindow = new();
    static TipsWindow tipsWindow = new();
    static SaveWindow saveWindow = new SaveWindow();

    static Random rand = new Random();
    static bool quickDeliveryActive = false;
    static string quickDeliveryLetter = "";
    static float quickDeliveryTimer = 0;
    static Vector2 quickDeliveryTargetPos;
    static Vector2 letterDropPosition;

    static GameState currentState = GameState.Editing;
    static string statusMessage = "Welcome to Stickman IDE! Type code to begin...";
    static int lettersDelivered = 0;

    static bool stickmanIsPunching = false;
    static bool stickmanIsStuck = false;
    static bool stickmanHasPunched = false;

    static float stickmanFacing = 1f;

    static int codeFontSize = 18;

    public static List<PunchAnimation> punchAnimationsInProgress = new List<PunchAnimation>();
    class SaveWindow
    {
        public bool IsVisible { get; set; }
        public Rectangle Bounds { get; set; }
        public string FileName { get; set; } = "code";
        public bool IsInputActive { get; set; }

        public SaveWindow()
        {
            IsVisible = false;
            Bounds = new Rectangle(300, 200, 500, 200);
        }

        public void Update()
        {
            if (!IsVisible) return;

            Vector2 mousePos = GetMousePosition();

            // Check close button
            Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 15, 20, 20);
            if (IsMouseButtonPressed(MouseButton.Left) && CheckCollisionPointRec(mousePos, closeButton))
            {
                IsVisible = false;
                FileName = "code";
                return;
            }

            // Check input field
            Rectangle inputField = new Rectangle(Bounds.X + 50, Bounds.Y + 80, Bounds.Width - 100, 40);
            if (IsMouseButtonPressed(MouseButton.Left))
            {
                if (CheckCollisionPointRec(mousePos, inputField))
                {
                    IsInputActive = true;
                }
                else
                {
                    IsInputActive = false;
                }
            }

            // Check save button
            Rectangle saveButton = new Rectangle(Bounds.X + 150, Bounds.Y + 140, 80, 40);
            bool saveHover = CheckCollisionPointRec(mousePos, saveButton);

            if (IsMouseButtonPressed(MouseButton.Left) && saveHover)
            {
                SaveFile();
                IsVisible = false;
            }

            // Check cancel button
            Rectangle cancelButton = new Rectangle(Bounds.X + 270, Bounds.Y + 140, 80, 40);
            bool cancelHover = CheckCollisionPointRec(mousePos, cancelButton);

            if (IsMouseButtonPressed(MouseButton.Left) && cancelHover)
            {
                IsVisible = false;
                FileName = "code";
            }

            // Handle text input
            if (IsInputActive)
            {
                int key = GetCharPressed();
                while (key > 0)
                {
                    if (key >= 32 && key <= 125) // Printable characters
                    {
                        FileName += (char)key;
                    }
                    key = GetCharPressed();
                }

                if (IsKeyPressed(KeyboardKey.Backspace) && FileName.Length > 0)
                {
                    FileName = FileName.Substring(0, FileName.Length - 1);
                }
            }
        }

        public void SaveFile()
        {
            // Ensure .py extension
            if (!FileName.ToLower().EndsWith(".py"))
            {
                FileName += ".py";
            }

            bool success = FileManager.SaveCodeToFile(editor.Text.Split('\n').ToList(), FileName);
            if (success)
            {
                statusMessage = $"Code saved as {FileName}!";
            }
            else
            {
                statusMessage = "Error saving file!";
            }
        }

        public void Draw(bool Hovered)
        {
            if (!IsVisible) return;

            // Background
            DrawRectangleRec(Bounds, ThemeManager.GetPanelBackground());
            DrawRectangleLines((int)Bounds.X, (int)Bounds.Y, (int)Bounds.Width, (int)Bounds.Height, ThemeManager.GetAccentColor());

            // Title
            DrawTextEx(regular_font, "SAVE CODE", new Vector2(Bounds.X + 180, Bounds.Y + 20), 28, spacing, Color.Gold);

            // File name label
            DrawTextEx(regular_font, "File name:", new Vector2(Bounds.X + 50, Bounds.Y + 50), 20, spacing, ThemeManager.GetTextColor());

            // Input field
            Rectangle inputField = new Rectangle(Bounds.X + 50, Bounds.Y + 80, Bounds.Width - 100, 40);
            Color inputColor = IsInputActive ? new Color(80, 80, 100, 255) : new Color(60, 60, 80, 255);
            DrawRectangleRec(inputField, inputColor);
            DrawRectangleLines((int)inputField.X, (int)inputField.Y, (int)inputField.Width, (int)inputField.Height, ThemeManager.GetBorderColor());

            // File name text
            string displayName = FileName;
            if (IsInputActive && ((int)(GetTime() * 2) % 2 == 0))
            {
                displayName += "|";
            }
            DrawTextEx(regular_font, displayName, new Vector2(inputField.X + 10, inputField.Y + 10), 20, spacing, Color.White);

            // Save button
            Rectangle saveButton = new Rectangle(Bounds.X + 150, Bounds.Y + 140, 80, 40);
            bool saveHover = CheckCollisionPointRec(GetMousePosition(), saveButton);
            Color saveColor = saveHover ? new Color(60, 160, 60, 255) : new Color(40, 120, 40, 255);

            DrawRectangleRec(saveButton, saveColor);
            DrawRectangleLines((int)saveButton.X, (int)saveButton.Y, (int)saveButton.Width, (int)saveButton.Height, new Color(80, 200, 80, 255));
            DrawTextEx(regular_font, "SAVE", new Vector2(saveButton.X + 13, saveButton.Y + 12), 20, spacing, Color.White);

            // Cancel button
            Rectangle cancelButton = new Rectangle(Bounds.X + 270, Bounds.Y + 140, 80, 40);
            bool cancelHover = CheckCollisionPointRec(GetMousePosition(), cancelButton);
            Color cancelColor = cancelHover ? new Color(160, 60, 60, 255) : new Color(120, 40, 40, 255);

            DrawRectangleRec(cancelButton, cancelColor);
            DrawRectangleLines((int)cancelButton.X, (int)cancelButton.Y, (int)cancelButton.Width, (int)cancelButton.Height, new Color(200, 80, 80, 255));
            DrawTextEx(regular_font, "BACK", new Vector2(cancelButton.X + 5, cancelButton.Y + 12), 20, spacing, Color.White);

            // Close button
            Rectangle closeButton = new Rectangle(Bounds.X + Bounds.Width - 35, Bounds.Y + 15, 20, 20);
            Color closeColor = Hovered ? Color.Red : new Color(200, 100, 100, 255);
            DrawRectangleRec(closeButton, closeColor);
            DrawTextEx(regular_font, "X", new Vector2(closeButton.X + 4, closeButton.Y + 2), codeFontSize, 0, Color.White);
        }
    }
    static void Main()
    {
        InitWindow(screenWidth, screenHeight, "Stickman IDE - Code Adventure");
        SetWindowState(ConfigFlags.ResizableWindow);
        SetTargetFPS(60);
        SetExitKey(KeyboardKey.Null);
        regular_font = LoadFont(regular_font_path);
        extra_bold_font = LoadFont(extra_bold_font_path);

        MusicManager.Initialize();
        MusicManager.LoadMusic();
        themeToggle = new ThemeToggle(CalculateThemeToggle());

        editor = new Editor(CalculateCodeEditor(), CalculateCodeEditorPosition());
        executeButton = new UIButton(CalculateExecuteButton(), "Execute Code");
        achievementsButton = new UIButton(CalculateAchievementsButton(), "Achievements");
        clearButton = new UIButton(CalculateClearButton(), "Clear Code");
        tipsButton = new UIButton(CalculateTipsButton(), "Tips");
        saveButton = new UIButton(CalculateSaveButton(), "Save Code");
        volumeSlider = new VolumeSlider(CalculateVolumeSlider(), CalculateVolumeSliderActual());

        Texture2D atlasPunch = LoadTexture("assets/Punch-Sheet.png");
        Texture2D atlasRun = LoadTexture("assets/Run-Sheet.png");
        Texture2D atlasIdle = LoadTexture("assets/Idle-Sheet.png");
        var punchFrames = new Frames(atlasPunch, 64, 64, 10, 3f);
        var runFrames = new Frames(atlasRun, 64, 64, 9, 4);
        var idleFrames = new Frames(atlasIdle, 64, 64, 6, 4);
        var stickmanPos = new Vector2(1200, 780);

        var stickmanSize = 3f;
        while (!WindowShouldClose())
        {
            pressedChar = false;
            bool stickmanMoved = false;
            Frames? stickmanFrames = null;
            float runSpeed = 8f;
            MusicManager.Update();
            ThemeManager.Update();
            saveWindow.Update();

            if (IsWindowResized())
            {
                screenWidth = GetScreenWidth();
                screenHeight = GetScreenHeight();

                editor.Bounds = CalculateCodeEditor();
                editor.Position = CalculateCodeEditorPosition();
                executeButton.Bounds = CalculateExecuteButton();
                achievementsButton.Bounds = CalculateAchievementsButton();
                clearButton.Bounds = CalculateClearButton();
                tipsButton.Bounds = CalculateTipsButton();
                saveButton.Bounds = CalculateSaveButton();
                volumeSlider.VisualBounds = CalculateVolumeSlider();
                volumeSlider.ActualBounds = CalculateVolumeSliderActual();
                outputWindow.Bounds = new Rectangle(screenWidth / 2 - 400, screenHeight / 2 - 300, 800, 500);
                tipsWindow.Bounds = new Rectangle(screenWidth / 2 - 300, screenHeight / 2 - 200, 600, 400);
                themeToggle.Bounds = CalculateThemeToggle();
            }

            // Handle input
            {
                var mousePos = GetMousePosition();
                volumeSlider.Update();
                HandleScroll(mousePos);
                outputWindow.HandleScroll(mousePos);

                // ALLE punch logica op één plaats
                // ALLE punch logica op één plaats
                // ALLE punch logica op één plaats
                if (stickmanHasPunched)
                {
                    stickmanHasPunched = false;

                    bool punchedButton = false;

                    // Check alle buttons eerst
                    if (StickmanOver(stickmanPos, achievementsButton.Bounds))
                    {
                        achievementManager.ShowAchievementsPanel = !achievementManager.ShowAchievementsPanel;
                        punchedButton = true;
                    }
                    else if (StickmanOver(stickmanPos, clearButton.Bounds))
                    {
                        ClearEditor();
                        statusMessage = "Code editor cleared!";
                        punchedButton = true;
                    }
                    else if (StickmanOver(stickmanPos, tipsButton.Bounds))
                    {
                        tipsWindow.IsVisible = !tipsWindow.IsVisible;
                        punchedButton = true;
                    }
                    else if (StickmanOver(stickmanPos, executeButton.Bounds))
                    {
                        if (outputWindow.IsVisible)
                        {
                            outputWindow.IsVisible = false;
                            outputWindow.piper.Stop();
                            lock (outputWindow.piper.OutputBuffer)
                            {
                                outputWindow.piper.OutputBuffer.Clear();
                            }
                        }
                        else
                        {
                            outputWindow.IsVisible = true;
                            outputWindow.piper.Run(editor.Text);
                            outputWindow.OutputText = "";
                        }
                        punchedButton = true;
                    }
                    else if (StickmanOver(stickmanPos, saveButton.Bounds))
                    {
                        saveWindow.IsVisible = true;
                        punchedButton = true;
                    }

                    // Check theme toggle button - ZET DIT BOVEN DE SAVE WINDOW CHECK
                    else if (StickmanOver(stickmanPos, themeToggle.Bounds))
                    {
                        if (!ThemeManager.IsLightMode)
                        {
                            ThemeManager.StartThemeSwitch();
                        }
                        punchedButton = true;
                    }
                    // VOEG DIT TOE: Check theme popup buttons - ZET DIT OOK BOVEN SAVE WINDOW
                    else if (ThemeManager.ShowThemePopup)
                    {
                        int popupWidth = 600;
                        int popupHeight = 300;
                        int popupX = (screenWidth - popupWidth) / 2;
                        int popupY = (screenHeight - popupHeight) / 2;

                        // Yes button (Accept light mode)
                        Rectangle yesButton = new Rectangle(popupX + 150, popupY + 200, 150, 50);
                        if (StickmanOver(stickmanPos, yesButton))
                        {
                            ThemeManager.StartThemeSwitch();
                            punchedButton = true;
                        }

                        // No button (Cancel)
                        Rectangle noButton = new Rectangle(popupX + 320, popupY + 200, 150, 50);
                        if (StickmanOver(stickmanPos, noButton))
                        {
                            ThemeManager.CancelThemeSwitch();
                            punchedButton = true;
                        }
                    }
                    // DAN PAS check save window buttons
                    else if (saveWindow.IsVisible)
                    {
                        // Check input field
                        Rectangle inputField = new Rectangle(saveWindow.Bounds.X + 50, saveWindow.Bounds.Y + 80, saveWindow.Bounds.Width - 100, 40);
                        if (StickmanOver(stickmanPos, inputField))
                        {
                            saveWindow.IsInputActive = true;
                            punchedButton = true;
                        }

                        // Check save button
                        Rectangle saveButton = new Rectangle(saveWindow.Bounds.X + 150, saveWindow.Bounds.Y + 140, 80, 40);
                        if (StickmanOver(stickmanPos, saveButton))
                        {
                            saveWindow.SaveFile();
                            saveWindow.IsVisible = false;
                            punchedButton = true;
                        }

                        // Check cancel button
                        Rectangle cancelButton = new Rectangle(saveWindow.Bounds.X + 270, saveWindow.Bounds.Y + 140, 80, 40);
                        if (StickmanOver(stickmanPos, cancelButton))
                        {
                            saveWindow.IsVisible = false;
                            saveWindow.FileName = "code";
                            punchedButton = true;
                        }

                        // Check close button
                        Rectangle closeButton = new Rectangle(saveWindow.Bounds.X + saveWindow.Bounds.Width - 35, saveWindow.Bounds.Y + 15, 20, 20);
                        if (StickmanOver(stickmanPos, closeButton))
                        {
                            saveWindow.IsVisible = false;
                            saveWindow.FileName = "code";
                            punchedButton = true;
                        }
                    }
                    // Check close buttons van openstaande windows
                    else if (outputWindow.IsVisible)
                    {
                        Rectangle closeButton = new Rectangle(outputWindow.Bounds.X + outputWindow.Bounds.Width - 35, outputWindow.Bounds.Y + 5, 20, 20);
                        if (StickmanOver(stickmanPos, closeButton))
                        {
                            outputWindow.IsVisible = false;
                            outputWindow.piper.Stop();
                            lock (outputWindow.piper.OutputBuffer)
                            {
                                outputWindow.piper.OutputBuffer.Clear();
                            }
                            punchedButton = true;
                        }
                    }
                    else if (tipsWindow.IsVisible)
                    {
                        Rectangle closeButton = new Rectangle(tipsWindow.Bounds.X + tipsWindow.Bounds.Width - 35, tipsWindow.Bounds.Y + 15, 20, 20);
                        if (StickmanOver(stickmanPos, closeButton))
                        {
                            tipsWindow.IsVisible = false;
                            punchedButton = true;
                        }
                    }
                    else if (achievementManager.ShowAchievementsPanel)
                    {
                        int panelWidth = 500;
                        int panelHeight = 600;
                        int panelX = (screenWidth - panelWidth) / 2;
                        int panelY = (screenHeight - panelHeight) / 2;
                        Rectangle closeButton = new Rectangle(panelX + panelWidth - 35, panelY + 15, 20, 20);

                        if (StickmanOver(stickmanPos, closeButton))
                        {
                            achievementManager.ShowAchievementsPanel = false;
                            punchedButton = true;
                        }
                    }

                    // NIEUWE LOGICA: Als stickman ergens anders slaat en er is een panel open, sluit het
                    if (!punchedButton)
                    {
                        if (achievementManager.ShowAchievementsPanel)
                        {
                            achievementManager.ShowAchievementsPanel = false;
                        }
                        else if (tipsWindow.IsVisible)
                        {
                            tipsWindow.IsVisible = false;
                        }
                        else if (outputWindow.IsVisible)
                        {
                            outputWindow.IsVisible = false;
                            outputWindow.piper.Stop();
                            lock (outputWindow.piper.OutputBuffer)
                            {
                                outputWindow.piper.OutputBuffer.Clear();
                            }
                        }
                        else if (ThemeManager.ShowThemePopup)
                        {
                            // Als stickman buiten de theme popup slaat, behandel het alsof "No" geklikt is
                            ThemeManager.CancelThemeSwitch();
                        }
                        else if (saveWindow.IsVisible)
                        {
                            saveWindow.IsVisible = false;
                            saveWindow.FileName = "code";
                        }
                    }
                }

                if (IsKeyPressed(KeyboardKey.LeftAlt) || IsKeyPressed(KeyboardKey.RightAlt))
                {
                    currentState = ((currentState == GameState.Moving) ? GameState.Editing : GameState.Moving);
                }

                var dPos = new Vector2(0, 0);

                if (currentState == GameState.Moving)
                {
                    if (IsKeyDown(KeyboardKey.Space))
                    {
                        if (!stickmanIsPunching)
                        {
                            stickmanIsStuck = true;
                            stickmanIsPunching = true;
                        }
                    }

                    if ((IsKeyDown(KeyboardKey.Down) || IsKeyDown(KeyboardKey.Up)) &&
                            (IsKeyDown(KeyboardKey.Left) || IsKeyDown(KeyboardKey.Right)))
                    {
                        runSpeed *= (float)(Math.Sqrt(2) / 2);
                    }




                    if (IsKeyDown(KeyboardKey.Space))
                    {
                        if (!stickmanIsPunching)
                        {
                            stickmanIsStuck = true;
                            stickmanIsPunching = true;
                        }
                    }

                    if (!stickmanIsStuck)
                    {
                        if (IsKeyDown(KeyboardKey.Down))
                        {
                            dPos.Y = runSpeed;
                        }
                        if (IsKeyDown(KeyboardKey.Up))
                        {
                            dPos.Y = -runSpeed;

                        }
                        if (IsKeyDown(KeyboardKey.Left))
                        {
                            dPos.X = -runSpeed;
                            stickmanFacing = 1f;
                        }
                        if (IsKeyDown(KeyboardKey.Right))
                        {
                            dPos.X = runSpeed;
                            stickmanFacing = -1f;
                        }
                    }
                }

		var turboMode = 1;

                if (!stickmanIsStuck)
                {
                    if (IsGamepadAvailable(0))
                    {
                        if (IsGamepadButtonDown(0, GamepadButton.RightFaceDown))
                        {
                            if (!stickmanIsPunching)
                            {
                                stickmanIsStuck = true;
                                stickmanIsPunching = true;
                            }
                        }

                        float leftStickX = GetGamepadAxisMovement(0, GamepadAxis.LeftX);
                        float leftStickY = GetGamepadAxisMovement(0, GamepadAxis.LeftY);
                        if (MathF.Abs(leftStickX) > 0.2f)
                        {
                            dPos.X = leftStickX * runSpeed;
                            stickmanFacing = (leftStickX < 0) ? 1f : -1f;
                        }
                        if (MathF.Abs(leftStickY) > 0.2f)
                        {
                            dPos.Y = leftStickY * runSpeed;
                        }

                        if (IsGamepadButtonDown(0, GamepadButton.RightFaceLeft))
                        {
				turboMode = 4;
                        }
			dPos *= turboMode;

                    }
                }

                if (dPos.X != 0 || dPos.Y != 0)
                {
                    stickmanMoved = true;
                }

                /*
                if(dPos.X != 0 && dPos.Y != 0)
                {

                    dPos *= 0.7071f; // Square root of 2 over 2
                }
                */

                stickmanPos += dPos;
                if (stickmanPos.X < 0) stickmanPos.X += screenWidth;
                if (stickmanPos.X > screenWidth) stickmanPos.X -= screenWidth;
                if (stickmanPos.Y < 0) stickmanPos.Y += screenHeight;
                if (stickmanPos.Y > screenHeight) stickmanPos.Y -= screenHeight;


                if (currentState == GameState.Editing)
                {
                    HandleArrowNavigation();
                    ProcessControlKeys();
                    ProcessCharacterInput();
                    UpdateKeyRepeatTiming();

                    // Update achievements elke frame
                    achievementManager.UpdateAchievements(editor.Text, editor.GetLineCount());
                }

                achievementManager.UpdateAchievementDisplays();

                if (false) { }
                else if (stickmanMoved)
                {
                    stickmanFrames = runFrames;
                }
                else if (stickmanIsPunching)
                {
                    stickmanFrames = punchFrames;
                }
                else
                {
                    stickmanFrames = idleFrames;
                }

                // Update run animation
                Frames.UpdateIndex(stickmanFrames);
                if (stickmanIsPunching)
                {
                    if (Frames.ChangedIndex(stickmanFrames) && stickmanFrames.index == 0)
                    {
                        stickmanIsPunching = false;
                        stickmanIsStuck = false;
                        stickmanHasPunched = true;
                        stickmanFrames.index = 0;
                    }
                }
                stickmanFrames.prevTimer = stickmanFrames.timer;
            }

            // Draw()
            {
                BeginDrawing();

                // Background
                ClearBackground(ThemeManager.GetBackgroundColor());

                // DrawHeader()
                {
                    // Header background
                    DrawRectangle(0, 0, screenWidth, 60, ThemeManager.GetHeaderColor());
                    DrawRectangle(0, 60, screenWidth, 2, ThemeManager.GetAccentColor());
                }

                DrawEditor();

                // Draw UI elements
                executeButton.Draw(StickmanOver(stickmanPos, executeButton.Bounds));
                achievementsButton.Draw(StickmanOver(stickmanPos, achievementsButton.Bounds));
                clearButton.Draw(StickmanOver(stickmanPos, clearButton.Bounds));
                tipsButton.Draw(StickmanOver(stickmanPos, tipsButton.Bounds));
                saveButton.Draw(StickmanOver(stickmanPos, saveButton.Bounds));
                volumeSlider.Draw();
                themeToggle.Draw();

                // DrawStatusMessage()
                {
                    Color statusColor = currentState switch
                    {
                        GameState.Moving => Color.Red,
                        GameState.Editing => Color.Green,
                        _ => ThemeManager.GetLightAccentColor()
                    };

                    DrawTextEx(regular_font, statusMessage, new Vector2(250, 20), font_size, spacing, statusColor);
                }

                int Column = (cursorPosition - GetCurrentLineStart() + 1);
                int Line = (GetLineNumberFromPosition(cursorPosition) + 1);
                DrawText($"{Line},{Column}", 28, 70, 20, new Color(80, 60, 120, 255));

                // Draw windows
                outputWindow.Draw(StickmanOver(stickmanPos, outputWindow.Bounds));
                tipsWindow.Draw(StickmanOver(stickmanPos, tipsWindow.Bounds));
                saveWindow.Draw(StickmanOver(stickmanPos, saveWindow.Bounds));
                achievementManager.DrawAchievementNotifications(screenWidth, screenHeight);
                achievementManager.DrawAchievementsPanel(screenWidth, screenHeight);
                ThemeManager.DrawThemePopup(screenWidth, screenHeight);

                var source = new Rectangle(stickmanFrames.index * stickmanFrames.width, 0, stickmanFrames.width, stickmanFrames.height);
                var dest = new Rectangle(stickmanPos.X, stickmanPos.Y, stickmanFrames.width, stickmanFrames.height);
                source.Width *= -stickmanFacing;
                dest.Width *= stickmanSize;
                dest.Height *= stickmanSize;


                DrawTexturePro(stickmanFrames.atlas,
                               source, dest, new Vector2(dest.Width / 2f, dest.Height / 2f),
                               0, Color.Blue);


                if (pressedChar)
                {
                    MusicManager.PlayTypeSound();

                    var punchAnimationFrames = new Frames(atlasPunch, 64, 64, 10, 1f);
                    punchAnimationFrames.prevTimer = 0;
                    punchAnimationFrames.timer = 0;

                    string charToDisplay = string.IsNullOrEmpty(lastCharString) ? " " : lastCharString;
                    var punchAnimation = new PunchAnimation(punchAnimationFrames, lastCharPos, charToDisplay);
                    punchAnimationsInProgress.Add(punchAnimation);
                }

                for (int animationIndex = 0; animationIndex < punchAnimationsInProgress.Count; animationIndex += 1)
                {
                    var animation = punchAnimationsInProgress[animationIndex];

                    Frames.UpdateIndex(animation.frames);
                    DrawCharacterWithPunchAnimation(animation.pos, animation.character, animation.frames);

                    if (animation.frames.done)
                    {
                        punchAnimationsInProgress.RemoveAt(animationIndex);
                        animationIndex--;
                    }
                    else
                    {
                        animation.frames.timer = animation.frames.prevTimer;
                    }
                }

                EndDrawing();
            }
        }

        CloseWindow();
        MusicManager.Stop();
    }
}

