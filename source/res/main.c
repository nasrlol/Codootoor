#include <stdio.h>
#include <stdlib.h>
#include <raylib.h>

#define SCREEN_HEIGHT 800
#define SCREEN_WIDTH  600


int main(int argc, char **argv) {

    InitWindow(800, 600, "Codootoor");
    SetTargetFPS(60);

    while (!WindowShouldClose()) {
        BeginDrawing();
            ClearBackground(RAYWHITE);
                DrawText("Hello World!", 190, 200, 29, LIGHTGRAY);
        EndDrawing();

    }

    CloseWindow();
    
    return 0;
}
