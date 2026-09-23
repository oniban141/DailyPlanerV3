using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace DailyPlaner
{
    public partial class App : Application
    {
        public static bool IsDarkTheme { get; private set; }

        private const string SettingsRegistryKey = "SOFTWARE\\DailyPlanner";
        private const string AutoStartRegistryKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        private const string AppName = "DailyPlanner";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            ApplyTheme(LoadDarkThemeSetting());
            CheckAndCreateDatabase();
            SetAutoStart(LoadAutoStartSetting());
        }

        public static bool LoadDarkThemeSetting()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(SettingsRegistryKey, false))
                {
                    if (key != null && key.GetValue("DarkTheme") is int value)
                    {
                        return value == 1;
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static bool LoadAutoStartSetting()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(SettingsRegistryKey, false))
                {
                    if (key != null && key.GetValue("AutoStart") is int value)
                    {
                        return value == 1;
                    }
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        private static void SaveSetting(string name, bool value)
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey(SettingsRegistryKey))
                {
                    if (key != null)
                    {
                        key.SetValue(name, value ? 1 : 0, RegistryValueKind.DWord);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void SetAutoStart(bool enabled)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(AutoStartRegistryKey, true))
                {
                    if (key != null)
                    {
                        if (enabled)
                        {
                            string exePath = Assembly.GetEntryAssembly().Location;
                            key.SetValue(AppName, exePath);
                        }
                        else if (key.GetValue(AppName) != null)
                        {
                            key.DeleteValue(AppName);
                        }
                    }
                }
                SaveSetting("AutoStart", enabled);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось изменить настройки автозапуска: {ex.Message}", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        public static void ApplyTheme(bool isDark)
        {
            IsDarkTheme = isDark;
            SaveSetting("DarkTheme", isDark);
            try
            {
                var res = Current.Resources;
                SetThemeBrushes(res, isDark);
                foreach (var dict in res.MergedDictionaries)
                {
                    SetThemeBrushes(dict, isDark);
                }
                foreach (var windowObject in Current.Windows)
                {
                    var window = windowObject as Window;
                    if (window == null)
                    {
                        continue;
                    }
                    SetThemeBrushes(window.Resources, isDark);
                    foreach (var dict in window.Resources.MergedDictionaries)
                    {
                        SetThemeBrushes(dict, isDark);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static void SetThemeBrushes(System.Windows.ResourceDictionary resources, bool isDark)
        {
            if (resources == null)
            {
                return;
            }

            var background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#34445D" : "#E8D8C9"));
            var foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#E8D8C9" : "#2E3949"));
            var card = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#4B607F" : "#FDFBF8"));
            var border = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#4B607F" : "#DCc9b6"));
            var subtle = new SolidColorBrush((Color)ColorConverter.ConvertFromString(isDark ? "#3D4F6B" : "#F5F0E8"));

            if (resources.Contains("WindowBackgroundBrush"))
            {
                resources["WindowBackgroundBrush"] = background;
                resources["WindowForegroundBrush"] = foreground;
                resources["WindowCardBrush"] = card;
                resources["WindowSubtleBrush"] = subtle;
            }

            if (resources.Contains("LightBackgroundBrush"))
            {
                resources["LightBackgroundBrush"] = background;
                resources["LightForegroundBrush"] = foreground;
                resources["LightCardBrush"] = card;
                resources["LightBorderBrush"] = border;
            }
        }

        private void CheckAndCreateDatabase()
        {
            try
            {
                var databaseService = new Services.DatabaseService();
                databaseService.TestConnection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к базе данных:\n{ex.Message}\n\nПроверьте, что SQL Server (PCGl1tch) запущен и база DailyPlannerDB создана.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
