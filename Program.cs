using Raylib_cs;
using static Raylib_cs.Raylib;
using System;
using static System.Console;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.IO;

namespace Odootoor;

enum GameState { Editing, Delivering, Returning, Success, QuickDelivery, Falling }

public partial class Program
{
    const bool DEBUGDisableDeliveries = true;

    static int screenWidth = 1400;
    static int screenHeight = 900;
    const int CODE_EDITOR_WIDTH_PERCENT = 70;
    const int CODE_EDITOR_HEIGHT_PERCENT = 85;


    static AchievementManager achievementManager = new AchievementManager();
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

    static void UpdateQuickDelivery()
    {
        quickDeliveryTimer -= GetFrameTime();

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
            letterDropPosition = new Vector2(stickman.Position.X, editor.Bounds.Y + editor.Bounds.Height + 30);
            stickman.StartFall(stickman.Position, letterDropPosition, quickDeliveryLetter);
            statusMessage = "Oh no! Stickman dropped the letter in the water!";
            quickDeliveryActive = false;
        }
    }

    static void StartQuickDeliveryForLetters()
    {
        if (!string.IsNullOrEmpty(editor.CurrentInput) &&
            char.IsLetter(editor.CurrentInput[^1]) &&
            !quickDeliveryActive &&
            currentState == GameState.Editing)
        {
            quickDeliveryActive = true;
            quickDeliveryLetter = editor.CurrentInput[^1].ToString();
            quickDeliveryTimer = 1.5f;

            quickDeliveryTargetPos = new Vector2(
                editor.Bounds.X + 100f,
                editor.Bounds.Y + 50f
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

    static void Main()
    {
        InitWindow(screenWidth, screenHeight, "Stickman IDE - Code Delivery Adventure");
        SetWindowState(ConfigFlags.ResizableWindow);
        SetTargetFPS(60);
        SetExitKey(KeyboardKey.Null);

        // InitializeComponents();
        editor = new Editor(CalculateCodeEditor(), CalculateCodeEditorPosition());
        stickman = new Stickman(CalculateStickmanStartPosition());
        executeButton = new UIButton(CalculateExecuteButton(), "Execute Code");
        achievementsButton = new UIButton(CalculateAchievementsButton(), "Achievements");
        clearButton = new UIButton(CalculateClearButton(), "Clear Code");
        tipsButton = new UIButton(CalculateTipsButton(), "Tips");
        saveButton = new UIButton(CalculateSaveButton(), "Save Code");
        volumeSlider = new VolumeSlider(CalculateVolumeSlider(), CalculateVolumeSliderActual());

        Texture2D atlasPunch = LoadTexture("assets/Punch-Sheet.png");
        Texture2D atlasRun = LoadTexture("assets/Run-Sheet.png");
        var punchFrames = new Frames(atlasPunch, 64, 64, 10, 6);
        var runFrames = new Frames(atlasRun, 64, 64, 9, 2);
        var manPos = new Vector2(screenWidth / 2, screenHeight / 2);

        while (!WindowShouldClose())
        {
            if (IsWindowResized())
            {
                screenWidth = GetScreenWidth();
                screenHeight = GetScreenHeight();

                editor.Bounds = CalculateCodeEditor();
                editor.Position = CalculateCodeEditorPosition();
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

            // Update();
            {


                Vector2 mousePos = GetMousePosition();

                // Handle ESC for panels
                if (IsKeyPressed(KeyboardKey.Escape))
                {
                    achievementManager.ShowAchievementsPanel = false;
                    outputWindow.IsVisible = false;
                    tipsWindow.IsVisible = false;
                }

                // Handle F1 for tips
                if (IsKeyPressed(KeyboardKey.F1))
                {
                    tipsWindow.IsVisible = !tipsWindow.IsVisible;
                }

                volumeSlider.Update();
                HandleScroll(mousePos);
                outputWindow.HandleScroll(mousePos);

                // FIXED: Achievements button - simplified click detection
                if (IsMouseButtonPressed(MouseButton.Left))
                {
                    if (achievementsButton.IsMouseOver())
                    {
                        achievementManager.ShowAchievementsPanel = !achievementManager.ShowAchievementsPanel;
                        Console.WriteLine("Achievements button clicked!"); // Debug line
                    }
                    else if (clearButton.IsMouseOver())
                    {
                        ClearEditor();
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
                if (achievementManager.ShowAchievementsPanel && IsMouseButtonPressed(MouseButton.Left))
                {
                    Rectangle achievementsPanel = new Rectangle(
                                    (screenWidth - 500) / 2,
                                    (screenHeight - 600) / 2,
                                    500,
                                    600
                    );

                    if (!CheckCollisionPointRec(mousePos, achievementsPanel) &&
                                    !CheckCollisionPointRec(mousePos, achievementsButton.Bounds))
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
                    string previousInput = editor.CurrentInput;

                    HandleArrowNavigation();
                    ProcessControlKeys();
                    ProcessCharacterInput();
                    UpdateKeyRepeatTiming();

                    achievementManager.CheckAchievements(editor.CurrentInput, editor.Lines.Count);

                    if (editor.CurrentInput.Length > previousInput.Length &&
                                    char.IsLetter(editor.CurrentInput[^1]) &&
                                    !quickDeliveryActive &&
                                    currentState == GameState.Editing)
                    {
                        if (!DEBUGDisableDeliveries)
                        {
                            StartQuickDeliveryForLetters();
                        }
                    }
                }

                UpdateStickman();
                achievementManager.UpdateAchievementDisplays();

                // Update run animation
                Frames.UpdateIndex(runFrames);
                runFrames.prevIndex = runFrames.index;
            }

            // Draw()
            {
                BeginDrawing();

                // Background
                ClearBackground(new Color(20, 20, 30, 255));

                // DrawHeader()
                {
                    // Header background
                    DrawRectangle(0, 0, screenWidth, 60, new Color(40, 40, 60, 255));
                    DrawRectangle(0, 60, screenWidth, 2, new Color(80, 60, 120, 255));

                    // Title
                    DrawText("STICKMAN IDE", screenWidth / 2 - 150, 10, 36, Color.White);
                    DrawText("Code Delivery Adventure", screenWidth / 2 - 120, 45, 18, new Color(200, 180, 255, 255));
                }

                DrawEditor();
                EnvironmentRenderer.DrawWaterWaves(editor.Bounds);
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

                // DrawStatusMessage();
                {
                    Color statusColor = currentState switch
                    {
                        GameState.Success => Color.Green,
                        GameState.Falling => Color.Red,
                        GameState.QuickDelivery => Color.Yellow,
                        _ => new Color(100, 200, 255, 255)
                    };

                    DrawText("Status: " + statusMessage, 20, 70, 20, statusColor);
                }

                // Draw windows
                outputWindow.Draw();
                tipsWindow.Draw();
                achievementManager.DrawAchievementsPanel(screenWidth, screenHeight);
                achievementManager.DrawAchievementNotifications(screenWidth, screenHeight);


                var facing = 1;
                var size = 3f;
                var source = new Rectangle(runFrames.index * runFrames.width, 0, runFrames.width, runFrames.height);
                var dest = new Rectangle(manPos.X, manPos.Y, runFrames.width, runFrames.height);
                source.Width *= -facing;
                dest.Width *= size;
                dest.Height *= size;
                DrawTexturePro(runFrames.atlas, source, dest, new Vector2(dest.Width / 2f, dest.Height / 2f), 0, Color.Blue);


                EndDrawing();
            }

        } // shouldClose

        CloseWindow();
    }


}
