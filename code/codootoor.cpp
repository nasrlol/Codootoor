#include "raylib.h"

#ifdef OS_WINDOWS
# define RADDBG_MARKUP_IMPLEMENTATION
#else
# define RADDBG_MARKUP_STUBS
#endif
#include "raddbg_markup.h"

#include <stdio.h>

struct frames
{
    Texture2D Atlas;
    int Width;
    int Height;
    int Count;
    int Index;
    int Timer;
    int PrevTimer;
    float Speed;
    int Done;
};

typedef int b32;

b32 UpdateIndex(frames *Frames)
{
    Frames->Timer = (int)((float)Frames->Count*Frames->Speed*(float)GetTime());
    
    b32 Changed = (Frames->Timer != Frames->PrevTimer);
    if(Changed)
    {
        Frames->Index += 1;
        Frames->PrevTimer = Frames->Timer;
    }
    
    if(Frames->Index == Frames->Count)
    {
        Frames->Index -= Frames->Count;
        Frames->Done += 1;
    }
    
    
    return Changed;
}

Rectangle RectangleFromFrames(frames Frames)
{
    Rectangle Result = {};
    
    Result.x      = (float)(Frames.Width*Frames.Index);
    Result.y      = 0;
    Result.width  = -1.0f*(float)Frames.Width;
    Result.height = (float)Frames.Height;
    
    return Result;
}

int main(void)
{
    const int screenWidth = 800;
    const int screenHeight = 600;
    
    InitWindow(screenWidth, screenHeight, "Odootoor");
    
    SetTargetFPS(60);
    
    Texture2D PunchTexture = LoadTexture("../archived/assets/Punch-Sheet.png");
    frames PunchFrames = {.Atlas = PunchTexture, .Width = 64, .Height = 64, .Count = 10, .Speed = 8.0f};
    
    while(!WindowShouldClose())
    {
        // Input
        
        // Update
        UpdateIndex(&PunchFrames);
        
        // Render
        
        BeginDrawing();
        ClearBackground(RAYWHITE);
        
        DrawText("Odootoor", 190, 200, 20, LIGHTGRAY);
        
        // Draw punch animation
        {        
            Rectangle Source = RectangleFromFrames(PunchFrames);
            Rectangle Dest = {screenWidth/2, screenHeight/2, (float)PunchFrames.Width*2.0f, (float)PunchFrames.Height*2.0f};
            
            DrawTexturePro(PunchFrames.Atlas, Source, Dest, 
                           Vector2{0.5f*(float)PunchFrames.Width, 0.5f*(float)PunchFrames.Height}, 
                           0, BLUE);
        }
        
        EndDrawing();
    }
    
    CloseWindow();
    
    return 0;
}
