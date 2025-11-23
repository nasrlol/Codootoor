namespace Odootoor;

using Raylib_cs;
using static Raylib_cs.Raylib;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

public partial class Program
{
    class Achievement
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsUnlocked { get; set; }
        public Func<bool> CheckCondition { get; set; }
        public float DisplayTime { get; set; }

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
        public List<Achievement> Achievements { get; set; } = new List<Achievement>();
        public bool ShowAchievementsPanel { get; set; }
        public float AchievementsScrollOffset { get; set; }

        private int totalLinesWritten = 0;
        private bool hasTypedFirstLetter = false;
        private int programsExecuted = 0;

        public AchievementManager()
        {
            InitializeAchievements();
        }

        private void InitializeAchievements()
        {
            Achievements.Add(new Achievement("First Letter", "Type your first letter", () => hasTypedFirstLetter));
            Achievements.Add(new Achievement("Code Novice", "Write 50 lines of code", () => totalLinesWritten >= 50));
            Achievements.Add(new Achievement("Code Apprentice", "Write 100 lines of code", () => totalLinesWritten >= 100));
            Achievements.Add(new Achievement("Code Journeyman", "Write 250 lines of code", () => totalLinesWritten >= 250));
            Achievements.Add(new Achievement("Code Master", "Write 500 lines of code", () => totalLinesWritten >= 500));
            Achievements.Add(new Achievement("First Program", "Execute your first program", () => programsExecuted >= 1));
            Achievements.Add(new Achievement("Productive Programmer", "Execute 5 programs", () => programsExecuted >= 5));
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
            
            // Speel achievement sound
            MusicManager.PlayAchievementSound();
        }
    }
}

public void MarkProgramExecuted()
{
    programsExecuted++;
    CheckAchievements("", totalLinesWritten);
}

        public void UpdateAchievementDisplays()
        {
            foreach (var achievement in Achievements)
            {
                if (achievement.DisplayTime > 0)
                {
                    achievement.DisplayTime -= Raylib.GetFrameTime();
                }
            }
        }

        public void HandleAchievementsPanelInteraction(Vector2 mousePos, int screenWidth, int screenHeight)
        {
            int panelWidth = 500;
            int panelHeight = 600;
            int panelX = (screenWidth - panelWidth) / 2;
            int panelY = (screenHeight - panelHeight) / 2;

            Rectangle panelBounds = new Rectangle(panelX, panelY, panelWidth, panelHeight);

            // Handle scrolling
            if (CheckCollisionPointRec(mousePos, panelBounds))
            {
                float mouseWheel = GetMouseWheelMove();
                AchievementsScrollOffset -= mouseWheel * 20;
                float maxScroll = Math.Max(0, Achievements.Count * 65 - panelHeight + 120);
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

            Vector2 mousePos = Raylib.GetMousePosition();

            DrawRectangle(panelX - 2, panelY - 2, panelWidth + 4, panelHeight + 4, new Color(0, 0, 0, 100));
            DrawRectangle(panelX, panelY, panelWidth, panelHeight, ThemeManager.GetPanelBackground());
            DrawRectangleLines(panelX, panelY, panelWidth, panelHeight, ThemeManager.GetAccentColor());
            DrawRectangleLines(panelX - 1, panelY - 1, panelWidth + 2, panelHeight + 2, ThemeManager.GetLightAccentColor());

            int unlockedCount = Achievements.Count(a => a.IsUnlocked);
            DrawTextEx(regular_font, $"{unlockedCount}/{Achievements.Count} Unlocked", new Vector2((int)panelX + 150, (int)panelY + 40), font_size + 10, spacing, Color.White);
            DrawLine(panelX + 50, panelY + 85, panelX + panelWidth - 50, panelY + 85, ThemeManager.GetAccentColor());


            // Close button toevoegen
            Rectangle closeButton = new Rectangle(panelX + panelWidth - 35, panelY + 15, 20, 20);
            Color closeColor = CheckCollisionPointRec(mousePos, closeButton) ? Color.Red : new Color(200, 100, 100, 255);
            DrawRectangleRec(closeButton, closeColor);
            DrawTextEx(regular_font, "X", new Vector2((int)closeButton.X + 6, (int)closeButton.Y + 2), font_size, spacing, Color.White);

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

                    DrawRectangle(panelX + 20, (int)itemY, panelWidth - 40, 50, bgColor);
                    DrawRectangleLines(panelX + 20, (int)itemY, panelWidth - 40, 50, borderColor);

                    DrawTextEx(regular_font, $"{achievement.Name}", new Vector2(panelX + 35, (int)itemY + 5), 20, spacing, textColor);
                    DrawTextEx(regular_font, achievement.Description, new Vector2(panelX + 35, (int)itemY + 28), 14, spacing, descColor);
                    DrawTextEx(regular_font, status, new Vector2(panelX + panelWidth - 120, (int)itemY + 15), 16, spacing, statusColor);
                }
            }
        }

        public void DrawAchievementNotifications(int screenWidth, int screenHeight)
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

                    DrawRectangle(centerX - 210, centerY - 70, 420, 140, new Color(0, 0, 0, (int)(100 * alpha)));
                    DrawRectangle(centerX - 200, centerY - 60, 400, 120, bgColor);
                    DrawRectangleLines(centerX - 200, centerY - 60, 400, 120, borderColor);

                    DrawTextEx(regular_font, "ACHIEVEMENT UNLOCKED!", new Vector2(centerX - 120, centerY - 35), 12, spacing, goldColor);
                    DrawTextEx(regular_font, achievement.Name, new Vector2(centerX - 120, centerY - 5), 28, spacing, textColor);
                    DrawTextEx(regular_font, achievement.Description, new Vector2(centerX - 120, centerY + 25), 18, spacing, textColor);
                }
            }
        }

    }
}
