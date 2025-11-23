using Raylib_cs;
using static Raylib_cs.Raylib;
using System;
using static System.Console;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.IO;

namespace Odootoor;

enum GameState { Editing, Moving }

public partial class Program
{
    const bool DEBUGDisableDeliveries = true;

    static int screenWidth = 1400;
    static int screenHeight = 900;
    const int CODE_EDITOR_WIDTH_PERCENT = 70;
    const int CODE_EDITOR_HEIGHT_PERCENT = 85;

    static AchievementManager achievementManager = new AchievementManager();
    static UIButton executeButton;
    static UIButton achievementsButton;
    static UIButton clearButton;
    static UIButton tipsButton;
    static UIButton saveButton;
    static VolumeSlider volumeSlider;
    static OutputWindow outputWindow = new();
    static TipsWindow tipsWindow = new();

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

    static void Main()
    {
        InitWindow(screenWidth, screenHeight, "Stickman IDE - Code Delivery Adventure");
        SetWindowState(ConfigFlags.ResizableWindow);
        SetTargetFPS(60);
        SetExitKey(KeyboardKey.Null);

        // InitializeComponents();
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
            bool stickmanMoved = false;
            Frames stickmanFrames = null;
            float runSpeed = 12f;

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
                outputWindow.Bounds = new Rectangle(screenWidth / 2 - 400, screenHeight / 2 - 250, 800, 500);
                tipsWindow.Bounds = new Rectangle(screenWidth / 2 - 300, screenHeight / 2 - 200, 600, 400);
            }

            // Handle input
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
                if (stickmanHasPunched)
                {
                    stickmanHasPunched = false;

                    if (StickmanOver(stickmanPos, achievementsButton.Bounds))
                    {
                        achievementManager.ShowAchievementsPanel = !achievementManager.ShowAchievementsPanel;
                        Console.WriteLine("Achievements button clicked!"); // Debug line
                    }
                    else if (StickmanOver(stickmanPos, clearButton.Bounds))
                    {
                        ClearEditor();
                        statusMessage = "Code editor cleared!";
                    }
                    else if (StickmanOver(stickmanPos, tipsButton.Bounds))
                    {
                        tipsWindow.IsVisible = !tipsWindow.IsVisible;
                    }
                    else if (StickmanOver(stickmanPos, executeButton.Bounds))
                    {
			            outputWindow.IsVisible = !outputWindow.IsVisible;
                        outputWindow.piper.Run(ToBuffer(editor.Lines));
                        outputWindow.OutputText = "";
                        outputWindow.Draw();
                    }
                    else if (StickmanOver(stickmanPos, saveButton.Bounds))
                    {
                        //SaveCode();
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

                if (IsKeyPressed(KeyboardKey.LeftAlt) || IsKeyPressed(KeyboardKey.RightAlt))
                {
                    currentState = ((currentState == GameState.Moving) ? GameState.Editing : GameState.Moving);
                }

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

                    if (!stickmanIsStuck)
                    {
                        if (IsKeyDown(KeyboardKey.Down))
                        {
                            stickmanPos.Y += runSpeed;
                            if (stickmanPos.Y > screenHeight)
                            {
                                stickmanPos.Y -= screenHeight;
                            }
                            stickmanMoved = true;
                        }
                        if (IsKeyDown(KeyboardKey.Up))
                        {
                            stickmanPos.Y -= runSpeed;
                            if (stickmanPos.Y < 0)
                            {
                                stickmanPos.Y += screenHeight;
                            }
                            stickmanMoved = true;
                        }
                        if (IsKeyDown(KeyboardKey.Left))
                        {
                            stickmanPos.X -= runSpeed;
                            if (stickmanPos.X < 0)
                            {
                                stickmanPos.X += screenWidth;
                            }
                            stickmanMoved = true;
                            stickmanFacing = 1f;
                        }
                        if (IsKeyDown(KeyboardKey.Right))
                        {
                            stickmanPos.X += runSpeed;
                            if (stickmanPos.X > screenWidth)
                            {
                                stickmanPos.X -= screenWidth;
                            }
                            stickmanMoved = true;
                            stickmanFacing = -1f;
                        }


                    }
                }

                if (currentState == GameState.Editing)
                {
                    //string previousInput = editor.CurrentInput;

                    HandleArrowNavigation();
                    ProcessControlKeys();
                    ProcessCharacterInput();
                    UpdateKeyRepeatTiming();

                    //achievementManager.CheckAchievements(editor.CurrentInput, editor.Lines.Count);
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

                // Draw UI elements
                var mousePos = GetMousePosition();
                executeButton.Draw(StickmanOver(stickmanPos, executeButton.Bounds));
                achievementsButton.Draw(StickmanOver(stickmanPos, achievementsButton.Bounds));
                clearButton.Draw(StickmanOver(stickmanPos, clearButton.Bounds));
                tipsButton.Draw(StickmanOver(stickmanPos, tipsButton.Bounds));
                saveButton.Draw(StickmanOver(stickmanPos, saveButton.Bounds));
                volumeSlider.Draw();

                // DrawStatusMessage();
                {
                    Color statusColor = currentState switch
                    {
                        GameState.Moving => Color.Green,
                        GameState.Editing => Color.Red,
                        _ => new Color(100, 200, 255, 255)
                    };

                    DrawText("Status: " + statusMessage, 20, 70, 20, statusColor);
                }

                // Draw windows
                outputWindow.Draw();
                tipsWindow.Draw();
                achievementManager.DrawAchievementsPanel(screenWidth, screenHeight);
                achievementManager.DrawAchievementNotifications(screenWidth, screenHeight);

                var source = new Rectangle(stickmanFrames.index * stickmanFrames.width, 0, stickmanFrames.width, stickmanFrames.height);
                var dest = new Rectangle(stickmanPos.X, stickmanPos.Y, stickmanFrames.width, stickmanFrames.height);
                source.Width *= -stickmanFacing;
                dest.Width *= stickmanSize;
                dest.Height *= stickmanSize;
                DrawTexturePro(stickmanFrames.atlas,
                                     source, dest, new Vector2(dest.Width / 2f, dest.Height / 2f),
                                                                            0, Color.Blue);

                // DrawText(string.Format("{0} {1}", stickmanPos.X, stickmanPos.Y), 20, 300, 20, Color.SkyBlue);


                EndDrawing();
            }

        } // shouldClose

        CloseWindow();
    }

}
