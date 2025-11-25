#include "raylib.h"

#define RADDBG_MARKUP_IMPLEMENTATION
#include "raddbg_markup.h"

int main(void)
{
    const int screenWidth = 800;
    const int screenHeight = 600;
    
    InitWindow(screenWidth, screenHeight, "Odootoor");
    
    SetTargetFPS(60);
    
    while(!WindowShouldClose())
    {
        BeginDrawing();
        ClearBackground(RAYWHITE);
        DrawText("Odootoor", 190, 200, 20, LIGHTGRAY);
        
        EndDrawing();
    }
    
    CloseWindow();
    
    return 0;
}