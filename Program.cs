using System;
using System.Windows.Forms;
using LinearAlgebraDetector.Forms;
using LinearAlgebraDetector.Utils;

namespace LinearAlgebraDetector
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            try
            {
                ConfigManager.Initialize();
                Logger.Log("Приложение запущено");
                
                using (var splash = new SplashForm())
                {
                    splash.Show();
                    Application.DoEvents();
                    System.Threading.Thread.Sleep(2000);
                }
                
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                Logger.LogError("Ошибка при запуске", ex);
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}