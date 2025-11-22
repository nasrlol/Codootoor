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