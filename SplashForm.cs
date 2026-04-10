using System;
using System.Drawing;
using System.Windows.Forms;

namespace LinearAlgebraDetector.Forms
{
    public class SplashForm : Form
    {
        private Timer _timer;
        
        public SplashForm()
        {
            InitializeSplash();
        }
        
        private void InitializeSplash()
        {
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(500, 350);
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.ShowInTaskbar = false;
            this.TopMost = true;
            
            // Логотип
            var lblTitle = new Label
            {
                Text = "🧮 BRAg inc.",
                ForeColor = Color.FromArgb(124, 77, 255),
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 100
            };
            
            // Подзаголовок
            var lblSubtitle = new Label
            {
                Text = "Linear Algebra AI Detector",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };
            
            // Разработчик
            var lblDev = new Label
            {
                Text = "Timed Garb",
                ForeColor = Color.FromArgb(124, 77, 255),
                Font = new Font("Segoe UI", 10, FontStyle.Italic),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 30
            };
            
            // Загрузка
            var lblLoading = new Label
            {
                Text = "Загрузка компонентов...",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Bottom,
                Height = 50
            };
            
            // Прогресс-бар
            var progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Marquee,
                Height = 3,
                Dock = DockStyle.Bottom,
                ForeColor = Color.FromArgb(124, 77, 255)
            };
            
            this.Controls.Add(lblTitle);
            this.Controls.Add(lblSubtitle);
            this.Controls.Add(lblDev);
            this.Controls.Add(lblLoading);
            this.Controls.Add(progressBar);
            
            // Таймер для автоматического закрытия
            _timer = new Timer();
            _timer.Interval = 2000;
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                this.Close();
            };
            _timer.Start();
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new Pen(Color.FromArgb(124, 77, 255), 3))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }
    }
}