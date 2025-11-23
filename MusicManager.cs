namespace Odootoor;

using Raylib_cs;
using System.IO;

class MusicManager
{
    public static Music BackgroundMusic;
    private static bool isInitialized = false;
    public static bool isLoaded = false;

    public static void Initialize()
    {
        if (!isInitialized)
        {
            Raylib.InitAudioDevice();
            isInitialized = true;
        }
    }

    public static bool LoadMusic()
    {
        try
        {
            // Probeer verschillende mogelijke locaties
            string[] possiblePaths = {
                "music/background.mp3",
                "assets/music/background.mp3",
                "background.mp3"
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    BackgroundMusic = Raylib.LoadMusicStream(path);
                    Raylib.PlayMusicStream(BackgroundMusic);
                    Raylib.SetMusicVolume(BackgroundMusic, 0.5f);
                    isLoaded = true;
                    return true;
                }
            }

            // Als geen muziekbestand gevonden, ga door zonder muziek
            System.Console.WriteLine("Music file not found, continuing without music");
            return false;
        }
        catch (System.Exception ex)
        {
            System.Console.WriteLine($"Error loading music: {ex.Message}");
            return false;
        }
    }

    public static void Update()
    {
        if (isLoaded)
        {
            Raylib.UpdateMusicStream(BackgroundMusic);
        }
    }

    public static void Stop()
    {
        if (isLoaded)
        {
            Raylib.StopMusicStream(BackgroundMusic);
            Raylib.UnloadMusicStream(BackgroundMusic);
            isLoaded = false;
        }
        if (isInitialized)
        {
            Raylib.CloseAudioDevice();
            isInitialized = false;
        }
    }
}