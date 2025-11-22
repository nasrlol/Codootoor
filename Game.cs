using Raylib_cs;
using static Raylib_cs.Raylib;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.IO;

namespace Odootoor;

class Stickman
{
    public Vector2 Position;
    public Vector2 OriginalPosition;
    public string CurrentWord;
    public float Speed = 3.0f;
    public bool IsFalling;
    public float FallTimer;
    public Vector2 FallStartPosition;
    public string FallenLetter;
    public Vector2 FallTargetPosition;

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
            FallTimer -= GetFrameTime();
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
            DrawCircle(x, y - 20, 10, new Color(255, 218, 185, 255));
            DrawLine(x, y - 10, x + 20, y + 10, Color.Blue);
            DrawLine(x, y, x - 10, y + 5, Color.Blue);
            DrawLine(x, y, x + 15, y - 10, Color.Blue);
            DrawLine(x + 20, y + 10, x + 5, y + 30, Color.DarkBlue);
            DrawLine(x + 20, y + 10, x + 35, y + 25, Color.DarkBlue);

            if (!string.IsNullOrEmpty(FallenLetter))
            {
                DrawRectangle(x - 20, y - 40, 40, 20, Color.White);
                DrawRectangleLines(x - 20, y - 40, 40, 20, Color.Black);
                DrawText(FallenLetter, x - 15, y - 35, 12, Color.Black);
            }
        }
        else
        {
            DrawCircle(x, y - 20, 10, new Color(255, 218, 185, 255));
            DrawLine(x, y - 10, x, y + 20, Color.Blue);
            DrawLine(x, y, x - 15, y - 5, Color.Blue);
            DrawLine(x, y, x + 15, y - 5, Color.Blue);

            float walkOffset = (float)Math.Sin(GetTime() * 8) * 5;
            DrawLine(x, y + 20, x - 15, y + 40 + (int)walkOffset, Color.DarkBlue);
            DrawLine(x, y + 20, x + 15, y + 40 - (int)walkOffset, Color.DarkBlue);

            if (!string.IsNullOrEmpty(CurrentWord))
            {
                DrawRectangle(x - 40, y - 60, 80, 25, Color.White);
                DrawRectangleLines(x - 40, y - 60, 80, 25, Color.Black);
                DrawText(CurrentWord, x - 35, y - 55, 12, Color.Black);
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
