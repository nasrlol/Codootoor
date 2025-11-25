#include <stdlib.h>
#include <raylib.h>
#include <stdbool.h>

// Global variables
static int      screen_height = 800;
static int      screen_width = 600; 
static int      default_font_size = 30;
static float    default_spacing = 5;
static char     *jb_regular = "res/JetBrainsMono-Regular.ttf";
static int      fps = 60;
static float    dt = 1.0 / 60.0;

int main(int argc, char **argv) {

    InitWindow(800, 450, "Window");

    while (!WindowShouldClose())
    {
        BeginDrawing();
        ClearBackground(BLACK);
        EndDrawing();
    }

    CloseWindow();

    return 0;

}
