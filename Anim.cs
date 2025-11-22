using System;
using System.Numerics;

using Raylib_cs;
using static Raylib_cs.Raylib;
using System.Diagnostics;

namespace Odootoor;

class Frames
{
    public Texture2D atlas;
    public int width;
    public int height;
    public int count;

    public int index;
    // NOTE(luca): timers are used to know when to advance the frame index.
    public int timer;
    public int prevTimer;

    public float speed;
    public bool stopped;

    public Frames(Texture2D _atlas, int _width, int _height, int _count, float _speed)
{
    atlas = _atlas;
    width = _width;
    height = _height;
    count = _count;
    speed = _speed;
    stopped = false; 
}

    public static bool ChangedIndex(Frames frames)
    {
        bool result = (frames.timer != frames.prevTimer);
        return result;
    }

    public static void UpdateIndex(Frames frames)
    {
        frames.timer = (int)(frames.count * GetTime() * frames.speed);
        frames.index += (frames.timer != frames.prevTimer) ? 1 : 0;

        Debug.Assert(frames.index <= frames.count);

        if (frames.index == frames.count)
        {
            frames.index -= frames.count;
        }
    }
}

class Animation
{
    const bool ShowDebugBoxes = false;
    const float FPS = 60f;
    const float dt = 1 / 60f;

    static void DrawCharacterWithPunchAnimation(Vector2 charPos, string message, int charIndex,
                                                                                                                                                                                 int fontSize,
                                                                                                                                                                                    Frames frames)
    {
        Font defaultFont = GetFontDefault();
        var character = message.Substring(charIndex, 1);

        var fontDim = MeasureTextEx(defaultFont, character, fontSize, 0);

        var manPos = new Vector2(charPos.X + (int)(fontDim.X + frames.width / 2), charPos.Y + (int)(fontDim.Y / 2));
        // manual hand offset
        manPos.X -= 16;
        manPos.Y += 3;

        Rectangle source = new Rectangle(frames.index * frames.width, 0, frames.width, frames.height);
        Rectangle dest = new Rectangle(manPos.X, manPos.Y, frames.width, frames.height);

        if (ShowDebugBoxes)
        {
            DrawRectangle((int)dest.X - frames.width / 2, (int)dest.Y - frames.height / 2, frames.width, frames.height, Color.Green);
        }
        if (ShowDebugBoxes)
        {
            DrawRectangleV(charPos, fontDim, Color.Pink);
        }

        DrawText(character, (int)charPos.X, (int)charPos.Y, fontSize, Color.Red);
        DrawTexturePro(frames.atlas, source, dest, new Vector2(frames.width / 2, frames.height / 2), 0, Color.Green);
    }

    static void DebugMain(string[] args)
    {
        Console.WriteLine(dt);
        InitWindow(800, 600, "TEST");

        var charIndex = 0;
        var message = "Odoootoor";

        int fontSize = 20;
        var toggle = false;
        Texture2D atlasPunch = LoadTexture("assets/Punch-Sheet.png");
        Texture2D atlasRun = LoadTexture("assets/Run-Sheet.png");

        var punchFrames = new Frames(atlasPunch, 64, 64, 10, 6);
        var runFrames = new Frames(atlasRun, 64, 64, 9, 2);
        float runSpeed = 1f;

        SetTargetFPS(60);

        var screenWidth = GetScreenWidth();
        var screenHeight = GetScreenHeight();

        var manPos = new Vector2(screenWidth / 2, screenHeight / 2);

        while (!WindowShouldClose())
        {
            BeginDrawing();
            ClearBackground(Color.Blue);

            Frames.UpdateIndex(punchFrames);
            if (Frames.ChangedIndex(punchFrames))
            {
                // Ended
                if (punchFrames.index == 0)
                {
                    if (!(toggle))
                    {
                        charIndex += 1;
                    }

                    if (charIndex >= message.Length)
                    {
                        // NOTE: if you want it not to loop, enable the code below
                        // punchFrames.stopped = true;
                        charIndex -= message.Length;
                    }

                }
            }
            punchFrames.prevTimer = punchFrames.timer;

            var manRan = false;
            // Handle input
            {
                if (IsKeyPressed(KeyboardKey.D))
                {
                    fontSize -= 5;
                }

                if (IsKeyPressed(KeyboardKey.I))
                {
                    fontSize += 5;
                }

                if (IsKeyPressed(KeyboardKey.Enter))
                {
                    toggle = !toggle;
                }

                var runSpeedDt = 0.02f * 60f * dt;

                if (IsKeyDown(KeyboardKey.Comma))
                {
                    runSpeed += runSpeedDt;
                    if (runSpeed > 3f)
                    {
                        runSpeed = 3f;
                    }
                }

                if (IsKeyDown(KeyboardKey.Period))
                {
                    runSpeed -= runSpeedDt;
                    if (runSpeed < 1f)
                    {
                        runSpeed = 1f;
                    }
                }

                var speedNormal = 1f;
                // Fix diagonal movement
                if ((IsKeyDown(KeyboardKey.Up) || IsKeyDown(KeyboardKey.Down)) &&
                            (IsKeyDown(KeyboardKey.Left) || IsKeyDown(KeyboardKey.Right)))
                {
                    speedNormal *= (float)(Math.Sqrt(2f) / 2f);
                }

                if (IsKeyDown(KeyboardKey.Up))
                {
                    manPos.Y -= 3f * speedNormal * runSpeed;
                    manRan = true;
                    if (manPos.Y < 0)
                    {
                        manPos.Y += screenHeight;
                    }
                }

                if (IsKeyDown(KeyboardKey.Down))
                {
                    manPos.Y += 3f * speedNormal * runSpeed;
                    manRan = true;
                    if (manPos.Y > screenHeight)
                    {
                        manPos.Y -= screenHeight;
                    }
                }


                if (IsKeyDown(KeyboardKey.Left))
                {
                    manPos.X -= 3 * runSpeed;
                    manRan = true;
                    if (manPos.X < 0)
                    {
                        manPos.X += screenWidth;
                    }
                }

                if (IsKeyDown(KeyboardKey.Right))
                {
                    manPos.X += 3 * runSpeed;
                    manRan = true;
                    if (manPos.X > screenWidth)
                    {
                        manPos.X -= screenWidth;
                    }
                }
            }

            runFrames.speed = 4.0f * runSpeed;

            if (!punchFrames.stopped)
            {
                var charPos2 = new Vector2(80, 200);
                charPos2.X += fontSize * charIndex;

                DrawCharacterWithPunchAnimation(charPos2, message, charIndex, fontSize, punchFrames);
            }

            if (manRan)
            {
                Frames.UpdateIndex(runFrames);
                runFrames.prevTimer = runFrames.timer;
            }

            var source = new Rectangle(runFrames.index * runFrames.width, 0, runFrames.width, runFrames.height);
            var dest = new Rectangle(manPos.X, manPos.Y, runFrames.width, runFrames.height);
            DrawTexturePro(runFrames.atlas, source, dest, new Vector2(runFrames.width / 2, runFrames.height / 2), 0, Color.Blue);

            // var source = new Rectangle(frameIndex*width, 0);
            // var dest   = new Rectangle(runManPos.X*frameWidth, 0);

            DrawText(string.Format("rs: {0}", runSpeed), 0, 0, 20, Color.Red);

            EndDrawing();
        }
        CloseWindow();
    }
}