#include <raylib.h>
#include <stdbool.h>
#include <stdio.h>

// Global variables
static int      screen_height = 800;
static int      screen_width = 600; 
static int      default_font_size = 30;
static float    default_spacing = 5;
static char     *jb_regular = "res/JetBrainsMono-Regular.ttf";
static int      fps = 60;
static float    dt = 1.0 / 60.0;

struct frame;
struct punch_animation;

typedef struct {

    Texture2D atlas;
    int     width;
    int     height;
    int     count;
    int     index;
    int     prev_timer;
    int     timer;
    float   speed;
    bool    done;
    bool    stopped;

} frame;

typedef struct {

    Vector2 pos;
    char *character;
    frame frames;

} punch_animation;

bool changed_frame_index(frame frames);
void update_frame_index(frame frames);

bool changed_frame_index(frame frames) {

    return frames.timer != frames.prev_timer;
}

void update_frame_index(frame frames) {

    frames.timer = frames.count * (int)GetTime() * frames.speed;
    frames.index += (frames.timer != frames.prev_timer) ? 1 : 0;

    if (frames.index == frames.count) {
        frames.index -= frames.count;
        frames.done = true;
    }
}

void draw_character_with_punch_animation(Vector2 char_pos, char *character, frame frames) {

    if (character == NULL) 
        return;

    Vector2 font_dimension; 
    Vector2 man_pos;
    Rectangle source;
    Rectangle dest;
    Vector2 text_origin;

    font_dimension= MeasureTextEx(GetFontDefault(), character, default_font_size, 0);

    man_pos.x = frames.index * frames.index * frames.width;
    man_pos.y = char_pos.y + (int)(font_dimension.y / 2);

    man_pos.x -= 16;
    man_pos.y += 3;

    source.x = frames.index * frames.width;
    source.y = 0;
    source.width = -1.0 * frames.width;
    source.height = frames.height;

    text_origin.x = (float)frames.width / 2;
    text_origin.y = (float)frames.height / 2; 

    DrawTexturePro(frames.atlas, source, dest,  text_origin, 0, WHITE);

    return;
}
