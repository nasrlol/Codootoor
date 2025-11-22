namespace Odootoor;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.IO;

class Achievement
{
    public string Name;
    public string Description;
    public bool IsUnlocked;
    public Func<bool> CheckCondition;
    public float DisplayTime;

    public Achievement(string name, string description, Func<bool> condition)
    {
        Name = name;
        Description = description;
        CheckCondition = condition;
        IsUnlocked = false;
        DisplayTime = 0;
    }
}

class AchievementManager
{
    public bool DEBUGDisableAchivementNotifications = true;
    public List<Achievement> Achievements = new List<Achievement>();
    public bool ShowAchievementsPanel;
    public float AchievementsScrollOffset;

    public int totalLinesWritten = 0;
    public bool hasTypedFirstLetter = false;
    public int programsExecuted = 0;
    public int quickDeliveries = 0;

    public AchievementManager()
    {
        Achievements.Add(new Achievement("First Letter", "Type your first letter", () => hasTypedFirstLetter));
        Achievements.Add(new Achievement("Code Novice", "Write 50 lines of code", () => totalLinesWritten >= 50));
        Achievements.Add(new Achievement("Code Apprentice", "Write 100 lines of code", () => totalLinesWritten >= 100));
        Achievements.Add(new Achievement("Code Journeyman", "Write 250 lines of code", () => totalLinesWritten >= 250));
        Achievements.Add(new Achievement("Code Master", "Write 500 lines of code", () => totalLinesWritten >= 500));
        Achievements.Add(new Achievement("Code Legend", "Write 1000 lines of code", () => totalLinesWritten >= 1000));
        Achievements.Add(new Achievement("First Program", "Execute your first program", () => programsExecuted >= 1));
        Achievements.Add(new Achievement("Productive Programmer", "Execute 5 programs", () => programsExecuted >= 5));
        Achievements.Add(new Achievement("Quick Fingers", "Make 10 quick deliveries", () => quickDeliveries >= 10));
        Achievements.Add(new Achievement("Code Marathon", "Write 2000 lines of code", () => totalLinesWritten >= 2000));
    }

    public void CheckAchievements(string inputText, int linesWritten)
    {
        totalLinesWritten = linesWritten;

        if (!hasTypedFirstLetter && !string.IsNullOrEmpty(inputText) && inputText.Any(char.IsLetter))
        {
            hasTypedFirstLetter = true;
        }

        foreach (var achievement in Achievements)
        {
            if (!achievement.IsUnlocked && achievement.CheckCondition())
            {
                achievement.IsUnlocked = true;
                achievement.DisplayTime = 3.0f;
            }
        }
    }

    public void MarkProgramExecuted()
    {
        programsExecuted++;
        CheckAchievements("", totalLinesWritten);
    }

    public void MarkQuickDelivery()
    {
        quickDeliveries++;
        CheckAchievements("", totalLinesWritten);
    }

    public void UpdateAchievementDisplays()
    {
        foreach (var achievement in Achievements)
        {
            if (achievement.DisplayTime > 0)
            {
                achievement.DisplayTime -= GetFrameTime();
            }
        }
    }

    public void HandleAchievementsScroll(Vector2 mousePos, Rectangle panelBounds)
    {
        if (CheckCollisionPointRec(mousePos, panelBounds))
        {
            float mouseWheel = GetMouseWheelMove();
            AchievementsScrollOffset -= mouseWheel * 20;
            float maxScroll = Math.Max(0, Achievements.Count * 65 - panelBounds.Height + 120);
            AchievementsScrollOffset = Math.Clamp(AchievementsScrollOffset, 0, maxScroll);
        }
    }

    public void DrawAchievementsPanel(int screenWidth, int screenHeight)
    {
        if (!ShowAchievementsPanel) return;

        int panelWidth = 500;
        int panelHeight = 600;
        int panelX = (screenWidth - panelWidth) / 2;
        int panelY = (screenHeight - panelHeight) / 2;

        Rectangle panelBounds = new Rectangle(panelX, panelY, panelWidth, panelHeight);

        // Handle scrolling
        Vector2 mousePos = GetMousePosition();
        HandleAchievementsScroll(mousePos, panelBounds);

        // Panel background
        DrawRectangle(panelX - 2, panelY - 2, panelWidth + 4, panelHeight + 4, new Color(0, 0, 0, 100));
        DrawRectangle(panelX, panelY, panelWidth, panelHeight, new Color(30, 30, 40, 255));
        DrawRectangleLines(panelX, panelY, panelWidth, panelHeight, new Color(80, 60, 120, 255));
        DrawRectangleLines(panelX - 1, panelY - 1, panelWidth + 2, panelHeight + 2, new Color(120, 100, 160, 255));

        // Title and progress
        int unlockedCount = Achievements.Count(a => a.IsUnlocked);
        DrawText("ACHIEVEMENTS", panelX + 150, panelY + 25, 32, Color.Gold);
        DrawText($"{unlockedCount}/{Achievements.Count} Unlocked", panelX + 180, panelY + 60, 20, Color.LightGray);
        DrawLine(panelX + 50, panelY + 85, panelX + panelWidth - 50, panelY + 85, new Color(80, 60, 120, 255));

        // Achievements list with scrolling
        int yOffset = 100;
        int startIndex = (int)(AchievementsScrollOffset / 65);
        int visibleCount = (int)((panelHeight - 120) / 65);

        for (int i = startIndex; i < Math.Min(startIndex + visibleCount + 1, Achievements.Count); i++)
        {
            var achievement = Achievements[i];
            float itemY = panelY + yOffset + (i - startIndex) * 65 - (AchievementsScrollOffset % 65);

            if (itemY >= panelY + 100 && itemY <= panelY + panelHeight - 50)
            {
                Color bgColor = achievement.IsUnlocked ? new Color(60, 100, 60, 100) : new Color(60, 60, 60, 100);
                Color borderColor = achievement.IsUnlocked ? new Color(100, 200, 100, 255) : new Color(100, 100, 100, 255);
                Color textColor = achievement.IsUnlocked ? new Color(144, 238, 144, 255) : Color.LightGray;
                Color descColor = achievement.IsUnlocked ? new Color(200, 255, 200, 255) : new Color(180, 180, 180, 255);
                string status = achievement.IsUnlocked ? "UNLOCKED" : "LOCKED";
                Color statusColor = achievement.IsUnlocked ? Color.Gold : Color.Gray;

                // Achievement item background
                DrawRectangle(panelX + 20, (int)itemY, panelWidth - 40, 50, bgColor);
                DrawRectangleLines(panelX + 20, (int)itemY, panelWidth - 40, 50, borderColor);

                DrawText($"{achievement.Name}", panelX + 35, (int)itemY + 5, 20, textColor);
                DrawText(achievement.Description, panelX + 35, (int)itemY + 28, 14, descColor);
                DrawText(status, panelX + panelWidth - 120, (int)itemY + 15, 16, statusColor);
            }
        }

        // Scroll bar for achievements
        if (Achievements.Count * 65 > panelHeight - 120)
        {
            float scrollbarHeight = (panelHeight - 120) * ((panelHeight - 120) / (Achievements.Count * 65));
            float scrollbarY = panelY + 100 + (AchievementsScrollOffset / (Achievements.Count * 65)) * (panelHeight - 120 - scrollbarHeight);

            DrawRectangle(panelX + panelWidth - 20, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(80, 80, 100, 255));
            DrawRectangleLines(panelX + panelWidth - 20, (int)scrollbarY, 8, (int)scrollbarHeight, new Color(120, 120, 140, 255));
        }

        // Close hint
        DrawText("Press ESC or click outside to close", panelX + 120, panelY + panelHeight - 30, 16, Color.Gray);
    }

    public void DrawAchievementNotifications(int screenWidth, int screenHeight)
    {
        if (!DEBUGDisableAchivementNotifications)
        {
            foreach (var achievement in Achievements)
            {
                if (achievement.DisplayTime > 0)
                {
                    float alpha = Math.Clamp(achievement.DisplayTime / 1.0f, 0f, 1f);
                    Color bgColor = new Color(40, 80, 40, (int)(220 * alpha));
                    Color borderColor = new Color(120, 200, 120, (int)(255 * alpha));
                    Color textColor = new Color(255, 255, 255, (int)(255 * alpha));
                    Color goldColor = new Color(255, 215, 0, (int)(255 * alpha));

                    int centerX = screenWidth / 2;
                    int centerY = screenHeight / 3;

                    // Notification background with shadow
                    DrawRectangle(centerX - 210, centerY - 70, 420, 140, new Color(0, 0, 0, (int)(100 * alpha)));
                    DrawRectangle(centerX - 200, centerY - 60, 400, 120, bgColor);
                    DrawRectangleLines(centerX - 200, centerY - 60, 400, 120, borderColor);

                    DrawText("ACHIEVEMENT UNLOCKED!", centerX - 120, centerY - 35, 22, goldColor);
                    DrawText(achievement.Name, centerX - 120, centerY - 5, 28, textColor);
                    DrawText(achievement.Description, centerX - 120, centerY + 25, 18, textColor);
                }
            }
        }
    }
}
