using System;
using System.Drawing;
using System.Windows.Forms;
using LinearAlgebraDetector.Core;

namespace LinearAlgebraDetector.Forms
{
    public partial class HistoryForm : Form
    {
        private TextAnalyzer analyzer;
        private ListBox lstHistory;
        private RichTextBox txtDetails;
        
        public HistoryForm(TextAnalyzer analyzer)
        {
            this.analyzer = analyzer;
            InitializeComponent();
            LoadHistory();
        }
        
        private void InitializeComponent()
        {
            this.Text = "📜 История проверок - BRAg inc.";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 46);
            
            var splitter = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
            splitter.SplitterDistance = 350;
            
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 55), Padding = new Padding(10) };
            var titleLeft = new Label { Text = "📋 ИСТОРИЯ ПРОВЕРОК", ForeColor = Color.White, Font = new Font("Segoe UI", 12, System.Drawing.FontStyle.Bold), Dock = DockStyle.Top, Height = 40 };
            
            lstHistory = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                ItemHeight = 40
            };
            lstHistory.SelectedIndexChanged += LstHistory_SelectedIndexChanged;
            
            leftPanel.Controls.Add(lstHistory);
            leftPanel.Controls.Add(titleLeft);
            
            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 55), Padding = new Padding(10) };
            var titleRight = new Label { Text = "📄 ДЕТАЛИ", ForeColor = Color.White, Font = new Font("Segoe UI", 12, System.Drawing.FontStyle.Bold), Dock = DockStyle.Top, Height = 40 };
            
            txtDetails = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                ReadOnly = true
            };
            
            rightPanel.Controls.Add(txtDetails);
            rightPanel.Controls.Add(titleRight);
            
            splitter.Panel1.Controls.Add(leftPanel);
            splitter.Panel2.Controls.Add(rightPanel);
            
            var btnClose = new Button { Text = "✖ Закрыть", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.FromArgb(60, 60, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClose.Click += (s, e) => Close();
            
            this.Controls.Add(splitter);
            this.Controls.Add(btnClose);
        }
        
        private void LoadHistory()
        {
            var history = analyzer.GetHistory();
            lstHistory.Items.Clear();
            foreach (var item in history)
            {
                lstHistory.Items.Add(new HistoryItemWrapper(item, $"{item.CheckedAt:dd.MM.yyyy HH:mm} | {item.Probability:F0}% | {item.TotalMarkersFound} маркеров | {item.FileName}"));
            }
        }
        
        private void LstHistory_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstHistory.SelectedItem != null)
            {
                var wrapper = (HistoryItemWrapper)lstHistory.SelectedItem;
                var item = wrapper.History;
                txtDetails.Text = $"Дата: {item.CheckedAt:dd.MM.yyyy HH:mm:ss}\n" +
                                  $"Файл: {item.FileName}\n" +
                                  $"Вероятность ИИ: {item.Probability:F1}%\n" +
                                  $"Найдено маркеров: {item.TotalMarkersFound}\n\n" +
                                  $"Фрагмент текста:\n{item.TextSnippet}\n\n" +
                                  $"Полный результат:\n{item.FullResultJson}";
            }
        }
        
        private class HistoryItemWrapper
        {
            public DetectionHistory History { get; }
            public string DisplayText { get; }
            
            public HistoryItemWrapper(DetectionHistory history, string displayText)
            {
                History = history;
                DisplayText = displayText;
            }
            
            public override string ToString()
            {
                return DisplayText;
            }
        }
    }
}