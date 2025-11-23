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
    public static void PlayAchievementSound()
{
    try
    {
        string[] possiblePaths = {
            "music/achievement.mp3",
            "achievement.mp3"
        };

        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
            {
                Sound achievementSound = Raylib.LoadSound(path);
                Raylib.SetSoundVolume(achievementSound, 0.7f);
                Raylib.PlaySound(achievementSound);
                return;
            }
        }

        System.Console.WriteLine("Achievement sound file not found");
    }
    catch (System.Exception ex)
    {
        System.Console.WriteLine($"Error playing achievement sound: {ex.Message}");
    }
}
public static void PlayTypeSound()
{
    try
    {
        string[] possiblePaths = {
            "music/type.mp3",
            "type.mp3"
        };

        foreach (string path in possiblePaths)
        {
            if (File.Exists(path))
            {
                Sound typeSound = Raylib.LoadSound(path);
                Raylib.SetSoundVolume(typeSound, 0.3f); // Iets zachter voor type geluid
                Raylib.PlaySound(typeSound);
                return;
            }
        }

        System.Console.WriteLine("Type sound file not found");
    }
    catch (System.Exception ex)
    {
        System.Console.WriteLine($"Error playing type sound: {ex.Message}");
    }
}
}