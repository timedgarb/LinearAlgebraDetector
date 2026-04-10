using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.ComponentModel;

namespace LinearAlgebraDetector.UI
{
    // ==================== СОВРЕМЕННАЯ КНОПКА ====================
    
    public class ModernButton : Button
    {
        private Color _hoverColor = Color.FromArgb(100, 124, 77, 255);
        private Color _pressColor = Color.FromArgb(150, 124, 77, 255);
        
        public ModernButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.FromArgb(124, 77, 255);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 10, FontStyle.Bold);
            Height = 40;
            Cursor = Cursors.Hand;
        }
        
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            BackColor = _hoverColor;
        }
        
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            BackColor = Color.FromArgb(124, 77, 255);
        }
        
        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            BackColor = _pressColor;
        }
        
        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            BackColor = _hoverColor;
        }
        
        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            using (var path = new GraphicsPath())
            {
                path.AddArc(0, 0, 12, 12, 180, 90);
                path.AddArc(Width - 13, 0, 12, 12, -90, 90);
                path.AddArc(Width - 13, Height - 13, 12, 12, 0, 90);
                path.AddArc(0, Height - 13, 12, 12, 90, 90);
                path.CloseFigure();
                Region = new Region(path);
            }
        }
    }
    
    // ==================== СОВРЕМЕННОЕ ТЕКСТОВОЕ ПОЛЕ ====================
    
    public class ModernTextBox : TextBox
    {
        private string _placeholder = "";
        
        public string Placeholder
        {
            get => _placeholder;
            set
            {
                _placeholder = value;
                Invalidate();
            }
        }
        
        public ModernTextBox()
        {
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = Color.FromArgb(45, 45, 65);
            ForeColor = Color.White;
            Font = new Font("Consolas", 11);
            Height = 30;
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (string.IsNullOrEmpty(Text) && !string.IsNullOrEmpty(Placeholder))
            {
                using (var brush = new SolidBrush(Color.Gray))
                {
                    e.Graphics.DrawString(Placeholder, Font, brush, new PointF(5, 5));
                }
            }
        }
        
        protected override void OnEnter(EventArgs e)
        {
            base.OnEnter(e);
            Invalidate();
        }
        
        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
            Invalidate();
        }
    }
    
    // ==================== СОВРЕМЕННЫЙ ПРОГРЕСС-БАР ====================
    
    public class ModernProgressBar : ProgressBar
    {
        public ModernProgressBar()
        {
            SetStyle(ControlStyles.UserPaint, true);
            Style = ProgressBarStyle.Continuous;
            Height = 8;
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            var rect = ClientRectangle;
            using (var backBrush = new SolidBrush(Color.FromArgb(60, 60, 80)))
            {
                e.Graphics.FillRectangle(backBrush, rect);
            }
            
            var progressWidth = (int)(rect.Width * ((double)Value / Maximum));
            if (progressWidth > 0)
            {
                var progressRect = new Rectangle(0, 0, progressWidth, rect.Height);
                using (var progressBrush = new LinearGradientBrush(progressRect, 
                    Color.FromArgb(124, 77, 255), Color.FromArgb(156, 39, 176), LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(progressBrush, progressRect);
                }
            }
        }
    }
    
    // ==================== СПИСОК МАРКЕРОВ ====================
    
    public class MarkerListView : ListBox
    {
        public MarkerListView()
        {
            BackColor = Color.FromArgb(45, 45, 65);
            ForeColor = Color.White;
            Font = new Font("Consolas", 10);
            DrawMode = DrawMode.OwnerDrawVariable;
            ItemHeight = 50;
        }
        
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            
            e.DrawBackground();
            var bounds = e.Bounds;
            
            using (var backBrush = new SolidBrush(e.State.HasFlag(DrawItemState.Selected) ? 
                Color.FromArgb(124, 77, 255) : Color.FromArgb(45, 45, 65)))
            {
                e.Graphics.FillRectangle(backBrush, bounds);
            }
            
            var text = GetItemText(Items[e.Index]);
            using (var textBrush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(text, Font, textBrush, bounds.X + 5, bounds.Y + 5);
            }
            
            e.DrawFocusRectangle();
        }
    }
    
    // ==================== ГРАДИЕНТНАЯ ПАНЕЛЬ ====================
    
    public class GradientPanel : Panel
    {
        private Color _startColor = Color.FromArgb(30, 30, 46);
        private Color _endColor = Color.FromArgb(45, 45, 65);
        
        public Color StartColor
        {
            get => _startColor;
            set { _startColor = value; Invalidate(); }
        }
        
        public Color EndColor
        {
            get => _endColor;
            set { _endColor = value; Invalidate(); }
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(ClientRectangle, _startColor, _endColor, LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
            base.OnPaint(e);
        }
    }
    
    // ==================== ТЕМЫ ====================
    
    public static class ThemeManager
    {
        public enum AppTheme { Dark, Light }
        private static AppTheme _currentTheme = AppTheme.Dark;
        
        public static AppTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                ApplyTheme();
            }
        }
        
        private static void ApplyTheme()
        {
            var form = Form.ActiveForm;
            if (form != null)
            {
                form.BackColor = _currentTheme == AppTheme.Dark ? Color.FromArgb(30, 30, 46) : Color.FromArgb(240, 240, 245);
                ApplyToControls(form.Controls);
            }
        }
        
        private static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (_currentTheme == AppTheme.Dark)
                {
                    control.BackColor = control is Panel ? Color.FromArgb(40, 40, 55) : Color.FromArgb(45, 45, 65);
                    control.ForeColor = Color.White;
                }
                else
                {
                    control.BackColor = control is Panel ? Color.FromArgb(235, 235, 240) : Color.FromArgb(250, 250, 255);
                    control.ForeColor = Color.Black;
                }
                
                if (control.HasChildren)
                {
                    ApplyToControls(control.Controls);
                }
            }
        }
    }
    
    // ==================== ЦВЕТОВАЯ СХЕМА ====================
    
    public static class Colors
    {
        public static Color BrandColor => Color.FromArgb(124, 77, 255);
        public static Color SuccessColor => Color.FromArgb(76, 175, 80);
        public static Color WarningColor => Color.FromArgb(255, 152, 0);
        public static Color ErrorColor => Color.FromArgb(244, 67, 54);
        public static Color CriticalColor => Color.FromArgb(156, 39, 176);
        
        public static Color DarkBg => Color.FromArgb(30, 30, 46);
        public static Color DarkSurface => Color.FromArgb(45, 45, 65);
        public static Color LightBg => Color.FromArgb(240, 240, 245);
        public static Color LightSurface => Color.FromArgb(250, 250, 255);
        
        public static Color GetSeverityColor(string severity)
        {
            return severity switch
            {
                "critical" => CriticalColor,
                "high" => ErrorColor,
                "medium" => WarningColor,
                _ => Color.Gray
            };
        }
    }
}