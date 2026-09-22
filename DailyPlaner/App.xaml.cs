using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.IO;
using System.Reflection;

namespace DailyPlaner
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            CheckAndCreateDatabase();
            SetupAutoStart();
        }

        private void CheckAndCreateDatabase()
        {
            try
            {
                var databaseService = new Services.DatabaseService();
                var users = databaseService.GetAllUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupAutoStart()
        {
            try
            {
                string appName = "DailyPlanner";
                string exePath = Assembly.GetEntryAssembly().Location;
                
                using (var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        key.SetValue(appName, exePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to setup auto-start: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
