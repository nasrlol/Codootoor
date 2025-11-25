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
        private int lastCheckedLines = 0;

        public AchievementManager()
        {
            InitializeAchievements();
        }

        private void InitializeAchievements()
        {
            Achievements.Add(new Achievement("First steps", "Write 10 lines of code", () => totalLinesWritten >= 10));
            Achievements.Add(new Achievement("Code Novice", "Write 50 lines of code", () => totalLinesWritten >= 50));
            Achievements.Add(new Achievement("Code Apprentice", "Write 100 lines of code", () => totalLinesWritten >= 100));
            Achievements.Add(new Achievement("Code Journeyman", "Write 250 lines of code", () => totalLinesWritten >= 250));
            Achievements.Add(new Achievement("Code Master", "Write 500 lines of code", () => totalLinesWritten >= 500));
            Achievements.Add(new Achievement("First Program", "Execute your first program", () => programsExecuted >= 1));
            Achievements.Add(new Achievement("Productive Programmer", "Execute 5 programs", () => programsExecuted >= 5));
        }

        public void UpdateAchievements(string inputText, int linesWritten)
        {
            // Update tellers
            totalLinesWritten = linesWritten;
            
            // Check voor eerste letter
            if (!hasTypedFirstLetter && !string.IsNullOrEmpty(inputText) && inputText.Any(char.IsLetter))
            {
                hasTypedFirstLetter = true;
            }

            // Check alle achievements
            CheckAllAchievements();
        }

        private void CheckAllAchievements()
        {
            foreach (var achievement in Achievements)
            {
                if (!achievement.IsUnlocked && achievement.CheckCondition())
                {
                    UnlockAchievement(achievement);
                }
            }
        }

        private void UnlockAchievement(Achievement achievement)
        {
            achievement.IsUnlocked = true;
            achievement.DisplayTime = 3.0f;

            // Debug output om te zien of achievements werken
            Console.WriteLine($"Achievement unlocked: {achievement.Name}");
            
            // Speel achievement sound
            MusicManager.PlayAchievementSound();
        }

        public void MarkProgramExecuted()
        {
            programsExecuted++;
            CheckAllAchievements();
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
            if (!ShowAchievementsPanel) return;

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

            // Close button handling
            Rectangle closeButton = new Rectangle(panelX + panelWidth - 35, panelY + 15, 20, 20);
            if (IsMouseButtonPressed(MouseButton.Left) && CheckCollisionPointRec(mousePos, closeButton))
            {
                ShowAchievementsPanel = false;
            }
        }

        public void DrawAchievementsPanel(int screenWidth, int screenHeight)
        {
            if (!ShowAchievementsPanel) return;

            int panelWidth = 500;
            int panelHeight = 600;
            int panelX = (screenWidth - panelWidth) / 2;
            int panelY = (screenHeight - panelHeight) / 2;

            // Donker overlay achter panel
            DrawRectangle(0, 0, screenWidth, screenHeight, new Color(0, 0, 0, 128));

            Rectangle panelBounds = new Rectangle(panelX, panelY, panelWidth, panelHeight);

            Vector2 mousePos = Raylib.GetMousePosition();

            DrawRectangle(panelX - 2, panelY - 2, panelWidth + 4, panelHeight + 4, new Color(0, 0, 0, 100));
            DrawRectangle(panelX, panelY, panelWidth, panelHeight, ThemeManager.GetPanelBackground());
            DrawRectangleLines(panelX, panelY, panelWidth, panelHeight, ThemeManager.GetAccentColor());
            DrawRectangleLines(panelX - 1, panelY - 1, panelWidth + 2, panelHeight + 2, ThemeManager.GetLightAccentColor());

            int unlockedCount = Achievements.Count(a => a.IsUnlocked);
            DrawTextEx(regular_font, $"ACHIEVEMENTS - {unlockedCount}/{Achievements.Count}", 
                      new Vector2(panelX + 20, panelY + 20), 24, spacing, Color.Gold);
            
            DrawLine(panelX + 20, panelY + 60, panelX + panelWidth - 20, panelY + 60, ThemeManager.GetAccentColor());

            // Close button
            Rectangle closeButton = new Rectangle(panelX + panelWidth - 35, panelY + 15, 20, 20);
            Color closeColor = CheckCollisionPointRec(mousePos, closeButton) ? Color.Red : new Color(200, 100, 100, 255);
            DrawRectangleRec(closeButton, closeColor);
            DrawTextEx(regular_font, "X", new Vector2((int)closeButton.X + 4, (int)closeButton.Y + 2), codeFontSize, spacing, Color.White);

            int yOffset = 80;
            int startIndex = (int)(AchievementsScrollOffset / 65);
            int visibleCount = (int)((panelHeight - 120) / 65);

            for (int i = startIndex; i < Math.Min(startIndex + visibleCount + 1, Achievements.Count); i++)
            {
                var achievement = Achievements[i];
                float itemY = panelY + yOffset + (i - startIndex) * 65 - (AchievementsScrollOffset % 65);

                if (itemY >= panelY + 80 && itemY <= panelY + panelHeight - 50)
                {
                    Color bgColor = achievement.IsUnlocked ? 
                        new Color(60, 100, 60, 100) : 
                        new Color(60, 60, 60, 100);
                    Color borderColor = achievement.IsUnlocked ? 
                        new Color(100, 200, 100, 255) : 
                        new Color(100, 100, 100, 255);
                    Color textColor = achievement.IsUnlocked ? 
                        new Color(144, 238, 144, 255) : 
                        Color.LightGray;
                    Color descColor = achievement.IsUnlocked ? 
                        new Color(200, 255, 200, 255) : 
                        new Color(180, 180, 180, 255);
                    string status = achievement.IsUnlocked ? "UNLOCKED" : "LOCKED";
                    Color statusColor = achievement.IsUnlocked ? Color.Gold : Color.Gray;

                    DrawRectangle(panelX + 20, (int)itemY, panelWidth - 40, 50, bgColor);
                    DrawRectangleLines(panelX + 20, (int)itemY, panelWidth - 40, 50, borderColor);

                    DrawTextEx(regular_font, $"{achievement.Name}", 
                              new Vector2(panelX + 35, (int)itemY + 5), 20, spacing, textColor);
                    DrawTextEx(regular_font, achievement.Description, 
                              new Vector2(panelX + 35, (int)itemY + 28), 14, spacing, descColor);
                    DrawTextEx(regular_font, status, 
                              new Vector2(panelX + panelWidth - 140, (int)itemY + 15), 16, spacing, statusColor);
                }
            }

            // Scrollbar
            if (Achievements.Count * 65 > panelHeight - 120)
            {
                float scrollbarHeight = (panelHeight - 120) * ((panelHeight - 120) / (float)(Achievements.Count * 65));
                float scrollbarY = panelY + 80 + (AchievementsScrollOffset / (Achievements.Count * 65)) * (panelHeight - 120 - scrollbarHeight);

                DrawRectangle(panelX + panelWidth - 12, (int)scrollbarY, 8, (int)scrollbarHeight, ThemeManager.GetScrollbarColor());
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

                    // Donkere achtergrond voor notification
                    DrawRectangle(centerX - 210, centerY - 70, 420, 140, new Color(0, 0, 0, (int)(100 * alpha)));
                    
                    // Notification background
                    DrawRectangle(centerX - 200, centerY - 60, 400, 120, bgColor);
                    DrawRectangleLines(centerX - 200, centerY - 60, 400, 120, borderColor);

                    // Achievement icon (ster)
                    DrawTextEx(regular_font, "★", new Vector2(centerX - 180, centerY - 40), 30, spacing, goldColor);

                    DrawTextEx(regular_font, "ACHIEVEMENT UNLOCKED!", 
                              new Vector2(centerX - 100, centerY - 35), 16, spacing, goldColor);
                    DrawTextEx(regular_font, achievement.Name, 
                              new Vector2(centerX - 100, centerY - 5), 28, spacing, textColor);
                    DrawTextEx(regular_font, achievement.Description, 
                              new Vector2(centerX - 100, centerY + 25), 18, spacing, textColor);
                }
            }
        }
    }
}