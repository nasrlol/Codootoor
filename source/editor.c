#include <stdio.h>
#include <stdlib.h>
#include <raylib.h>
#include <string.h>


static float delay = 0.3f;
static float repeat_rate = 0.05f;
static float key_hold_timer = 0f;
static int cursor_position = 0;
static KeyboardKey last_held_key = KEY_NULL;
static char *last_char;
static Vector2 last_char_pos;
static bool is_repeating = false;

typedef struct {

    Rectangle bounds;
    char *text;
    float scroll_offset;
    int current_line;
    
} editor;

int get_text_lines(char *text) {

    if (!(text == NULL && strcmp(text, ""))) 
        return 
}
