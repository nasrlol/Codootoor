﻿using Raylib_cs;
using static Raylib_cs.Raylib;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.IO;

namespace Odootoor;

enum GameState { Editing, Delivering, Returning, Success, QuickDelivery, Falling }

public partial class Program
{
    static int screenWidth = 1400;
    static int screenHeight = 900;
    const int CODE_EDITOR_WIDTH_PERCENT = 70;
    const int CODE_EDITOR_HEIGHT_PERCENT = 85;
    
    static AchievementManager achievementManager = new AchievementManager();
    static Editor editor;
    static Stickman stickman;
    static UIButton executeButton, achievementsButton, clearButton, tipsButton, saveButton;
    static VolumeSlider volumeSlider;
    static ThemeToggle themeToggle;
    static OutputWindow outputWindow = new OutputWindow();
    static TipsWindow tipsWindow = new TipsWindow();
    
    static GameState currentState = GameState.Editing;
    static string statusMessage = "Welcome to Stickman IDE! Type code to begin...";

    static void Main()
    {
        try
        {
            InitWindow(screenWidth, screenHeight, "Stickman IDE - Code Delivery Adventure");
            SetWindowState(ConfigFlags.ResizableWindow);
            SetTargetFPS(60);
            SetExitKey(KeyboardKey.Null);

            MusicManager.Initialize();
            MusicManager.LoadMusic();

            InitializeComponents();

            while (!WindowShouldClose())
            {
                if (IsWindowResized())
                {
                    screenWidth = GetScreenWidth();
                    screenHeight = GetScreenHeight();
                    UpdateComponentPositions();
                }

                Update();
                Draw();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            MusicManager.Stop();
            CloseWindow();
        }
    }

    static void InitializeComponents()
    {
        editor = new Editor(CalculateCodeEditor(), CalculateCodeEditorPosition());
        stickman = new Stickman(CalculateStickmanStartPosition());
        executeButton = new UIButton(CalculateExecuteButton(), "Execute Code");
        achievementsButton = new UIButton(CalculateAchievementsButton(), "Achievements");
        clearButton = new UIButton(CalculateClearButton(), "Clear Code");
        tipsButton = new UIButton(CalculateTipsButton(), "Tips");
        saveButton = new UIButton(CalculateSaveButton(), "Save Code");
        volumeSlider = new VolumeSlider(CalculateVolumeSlider(), CalculateVolumeSliderActual());
        themeToggle = new ThemeToggle(CalculateThemeToggle());
    }

    static void UpdateComponentPositions()
    {
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
        themeToggle.Bounds = CalculateThemeToggle();
        
        outputWindow.Bounds = new Rectangle(screenWidth / 2 - 400, screenHeight / 2 - 250, 800, 500);
        tipsWindow.Bounds = new Rectangle(screenWidth / 2 - 300, screenHeight / 2 - 200, 600, 400);
    }

    static void Update()
    {
        MusicManager.Update();
        Vector2 mousePos = GetMousePosition();

        themeToggle.Update();
        ThemeManager.Update();
        
        if (IsKeyPressed(KeyboardKey.Escape))
        {
            achievementManager.ShowAchievementsPanel = false;
            outputWindow.IsVisible = false;
            tipsWindow.IsVisible = false;
            ThemeManager.CancelThemeSwitch();
        }
        
        if (IsKeyPressed(KeyboardKey.F1))
        {
            tipsWindow.IsVisible = !tipsWindow.IsVisible;
        }
        
        volumeSlider.Update();
        HandleScroll(mousePos);
        outputWindow.HandleScroll(mousePos);
        
        if (IsMouseButtonPressed(MouseButton.Left))
        {
            if (executeButton.IsMouseOver())
            {
                ExecuteCode();
            }
            else if (achievementsButton.IsMouseOver())
            {
                achievementManager.ShowAchievementsPanel = !achievementManager.ShowAchievementsPanel;
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
            else if (saveButton.IsMouseOver())
            {
                SaveCode();
            }
        }

        if (outputWindow.CloseButtonClicked())
        {
            outputWindow.IsVisible = false;
        }

        if (tipsWindow.CloseButtonClicked())
        {
            tipsWindow.IsVisible = false;
        }

        if (achievementManager.ShowAchievementsPanel)
        {
            achievementManager.HandleAchievementsPanelInteraction(mousePos, screenWidth, screenHeight);
        }

        if (currentState == GameState.Editing)
        {
            UpdateEditingState(mousePos);
        }

        UpdateStickman();
        achievementManager.UpdateAchievementDisplays();
    }

    static void UpdateEditingState(Vector2 mousePos)
    {
        HandleInput();
        achievementManager.CheckAchievements(editor.CurrentInput, editor.Lines.Count);
    }

    static void UpdateStickman()
    {
        if (currentState == GameState.QuickDelivery)
        {
            // Quick delivery logic removed
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
        BeginDrawing();
        
        ClearBackground(ThemeManager.GetBackgroundColor());
        
        DrawHeader();
        
        DrawEditor();
        EnvironmentRenderer.DrawWaterWaves(editor.Bounds);
        EnvironmentRenderer.DrawHouse(CalculateHousePosition());
        stickman.Draw();


        executeButton.Draw();
        achievementsButton.Draw();
        clearButton.Draw();
        tipsButton.Draw();
        saveButton.Draw();
        volumeSlider.Draw();
        themeToggle.Draw();

        DrawStatusMessage();

        outputWindow.Draw();
        tipsWindow.Draw();
        achievementManager.DrawAchievementsPanel(screenWidth, screenHeight);
        achievementManager.DrawAchievementNotifications(screenWidth, screenHeight);
        
        ThemeManager.DrawThemePopup(screenWidth, screenHeight);

        EndDrawing();
    }

    static void DrawHeader()
    {
        DrawRectangle(0, 0, screenWidth, 60, ThemeManager.GetHeaderColor());
        DrawRectangle(0, 60, screenWidth, 2, ThemeManager.GetAccentColor());
        
        DrawText("STICKMAN IDE", screenWidth / 2 - 150, 10, 36, ThemeManager.GetTextColor());
        DrawText("Code Delivery Adventure", screenWidth / 2 - 120, 45, 18, ThemeManager.GetLightAccentColor());
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
        
        DrawText("Status: " + statusMessage, 20, 70, 20, statusColor);
    }

    static void ExecuteCode()
    {
        string fullCode = string.Join("\n", editor.Lines) +
                          (string.IsNullOrEmpty(editor.CurrentInput) ? "" : "\n" + editor.CurrentInput);

        if (!string.IsNullOrWhiteSpace(editor.CurrentInput))
        {
            editor.Lines.Add(editor.CurrentInput);
            editor.CurrentInput = "";
        }

        if (fullCode.Length > 0)
        {
            outputWindow.OutputText = ExecuteCode(editor.Lines);
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