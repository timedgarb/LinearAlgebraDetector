using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LinearAlgebraDetector.Core;
using LinearAlgebraDetector.Utils;

namespace LinearAlgebraDetector.Forms
{
    public partial class ResultForm : Form
    {
        private AnalysisResult result;
        private string fullText;
        private RichTextBox txtHighlighted;
        private ListBox lstMarkers;
        private Button btnExport;
        private Button btnClose;
        
        public ResultForm(AnalysisResult result, string fullText)
        {
            this.result = result;
            this.fullText = fullText;
            InitializeComponent();
            LoadData();
        }
        
        private void InitializeComponent()
        {
            this.Text = "📊 Результаты анализа - BRAg inc.";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 46);
            
            var splitter = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
            splitter.SplitterDistance = 350;
            
            // Левая панель - список маркеров
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 55), Padding = new Padding(10) };
            var titleLeft = new Label { Text = "🔴 НАЙДЕННЫЕ МАРКЕРЫ", ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 40 };
            
            lstMarkers = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                ItemHeight = 60
            };
            
            leftPanel.Controls.Add(lstMarkers);
            leftPanel.Controls.Add(titleLeft);
            
            // Правая панель - текст с подсветкой
            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 55), Padding = new Padding(10) };
            var titleRight = new Label { Text = "📝 ТЕКСТ С ПОДСВЕТКОЙ", ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 40 };
            
            txtHighlighted = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                Font = new Font("Consolas", 11),
                ReadOnly = true,
                WordWrap = true
            };
            
            rightPanel.Controls.Add(txtHighlighted);
            rightPanel.Controls.Add(titleRight);
            
            // Нижняя панель с кнопками
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = Color.FromArgb(35, 35, 50) };
            btnExport = new Button { Text = "💾 Сохранить отчёт", Location = new Point(10, 10), Size = new Size(130, 35), BackColor = Color.FromArgb(60, 60, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnExport.Click += (s, e) => ExportReport();
            
            btnClose = new Button { Text = "✖ Закрыть", Location = new Point(760, 10), Size = new Size(100, 35), BackColor = Color.FromArgb(60, 60, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClose.Click += (s, e) => Close();
            
            bottomPanel.Controls.Add(btnExport);
            bottomPanel.Controls.Add(btnClose);
            
            splitter.Panel1.Controls.Add(leftPanel);
            splitter.Panel2.Controls.Add(rightPanel);
            
            this.Controls.Add(splitter);
            this.Controls.Add(bottomPanel);
        }
        
        private void LoadData()
        {
            // Заполняем список маркеров
            foreach (var match in result.Matches.OrderByDescending(m => m.Score))
            {
                var severity = match.Marker.Severity == "critical" ? "🔴" : match.Marker.Severity == "high" ? "🟠" : "🟡";
                lstMarkers.Items.Add($"{severity} {match.Marker.Id}: {match.Marker.Description}\n   Найдено: {match.Count} раз(а) | Вес: {match.Marker.Weight} | Очки: {match.Score}\n   {string.Join(", ", match.Examples)}");
            }
            
            if (result.Matches.Count == 0)
            {
                lstMarkers.Items.Add("✅ Маркеры ИИ не обнаружены");
            }
            
            // Подсветка текста
            var highlighted = TextHighlighter.Highlight(fullText, result.Matches);
            txtHighlighted.Rtf = highlighted;
        }
        
        private void ExportReport()
        {
            using var dialog = new SaveFileDialog();
            dialog.Filter = "HTML файлы (*.html)|*.html|Текстовые файлы (*.txt)|*.txt";
            dialog.FileName = $"AI_Report_{DateTime.Now:yyyyMMdd_HHmmss}";
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (dialog.FilterIndex == 1)
                {
                    var html = HtmlExporter.Export(result, fullText);
                    System.IO.File.WriteAllText(dialog.FileName, html);
                }
                else
                {
                    var text = $"Отчёт об анализе\n===============\n\nВероятность ИИ: {result.Probability:F1}%\nВердикт: {result.Verdict}\n\nНайденные маркеры:\n";
                    foreach (var match in result.Matches)
                    {
                        text += $"\n- {match.Marker.Id}: {match.Marker.Description} (найдено {match.Count} раз)\n";
                    }
                    System.IO.File.WriteAllText(dialog.FileName, text);
                }
                
                MessageBox.Show("Отчёт сохранён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}