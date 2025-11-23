using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Numerics;

namespace Odootoor;

enum GameState { Editing, Moving }

public partial class Program
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
    static UIButton? executeButton;
    static UIButton? achievementsButton;
    static UIButton? clearButton;
    static UIButton? tipsButton;
    static UIButton? saveButton;
    static VolumeSlider? volumeSlider;
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

    static int codeFontSize = 18;

    public static List<PunchAnimation> punchAnimationsInProgress = new List<PunchAnimation>();

    static void Main()
    {
        InitWindow(screenWidth, screenHeight, "Stickman IDE - Code Adventure");
        SetWindowState(ConfigFlags.ResizableWindow);
        SetTargetFPS(60);
        SetExitKey(KeyboardKey.Null);
        regular_font = LoadFont(regular_font_path);
        extra_bold_font = LoadFont(extra_bold_font_path);

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
            pressedChar = false;
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
                outputWindow.Bounds = new Rectangle(screenWidth / 2 - 400, screenHeight / 2 - 300, 800, 500);
                tipsWindow.Bounds = new Rectangle(screenWidth / 2 - 300, screenHeight / 2 - 200, 600, 400);
            }

            // Handle input
            {
                var mousePos = GetMousePosition();
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
                        if (outputWindow.IsVisible == true)
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

                    }
                    else if (StickmanOver(stickmanPos, saveButton.Bounds))
                    {
                        //SaveCode();
                    }
                }

                // Close buttons for windows
                if (outputWindow.IsVisible)
                {

                    Rectangle closeButton = new Rectangle(outputWindow.Bounds.X + outputWindow.Bounds.Width - 35, outputWindow.Bounds.Y + 5, 20, 20);
                    if (stickmanIsPunching && StickmanOver(stickmanPos, closeButton))
                    {
                        outputWindow.IsVisible = false;
                    }
                }

                if (tipsWindow.IsVisible)
                {
                    Rectangle closeButton = new Rectangle(tipsWindow.Bounds.X + tipsWindow.Bounds.Width - 35, tipsWindow.Bounds.Y + 15, 20, 20);
                    if (stickmanIsPunching && StickmanOver(stickmanPos, closeButton))
                    {
                        tipsWindow.IsVisible = false;
                    }
                }

                // Close achievements panel when clicking outside
                if (achievementManager.ShowAchievementsPanel && stickmanIsPunching)
                {
                    Rectangle achievementsPanel = new Rectangle(
                            (screenWidth - 500) / 2,
                            (screenHeight - 600) / 2,
                            500,
                            600
                            );

                    if (!StickmanOver(stickmanPos, achievementsPanel) &&
                       !StickmanOver(stickmanPos, achievementsButton.Bounds))
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

                }

                DrawEditor();
                

                // Draw UI elements
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
                        GameState.Moving => Color.Red,
                        GameState.Editing => Color.Green,
                        _ => new Color(100, 200, 255, 255)
                    };

                    DrawTextEx(regular_font, statusMessage, new Vector2(250, 20), font_size, spacing, statusColor);
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

                if (currentState == GameState.Moving)
                {
                    DrawTexturePro(stickmanFrames.atlas,
                            source, dest, new Vector2(dest.Width / 2f, dest.Height / 2f),
                            0, Color.Blue);
                }

                // DrawText(string.Format("{0} {1}", stickmanPos.X, stickmanPos.Y), 20, 300, 20, Color.SkyBlue);



                if (pressedChar)
                {
                    var punchAnimationFrames = new Frames(atlasPunch, 64, 64, 10, 1f);
                    punchAnimationFrames.prevTimer = 0;
                    punchAnimationFrames.timer = 0;
                    var punchAnimation = new PunchAnimation(punchAnimationFrames, lastCharPos, lastCharString);
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

        } // shouldClose

        CloseWindow();
    }

}
