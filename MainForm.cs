using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using LinearAlgebraDetector.Core;
using LinearAlgebraDetector.Handlers;
using LinearAlgebraDetector.Utils;

namespace LinearAlgebraDetector.Forms
{
    public partial class MainForm : Form
    {
        private TextBox txtInput;
        private Panel dropZone;
        private Label lblDropHint;
        private Button btnAnalyze;
        private Button btnSelectFile;
        private Button btnManageMarkers;
        private Button btnHistory;
        private ProgressBar progressBar;
        private Label lblStatus;
        private Panel resultPanel;
        private Label lblResultPercent;
        private ProgressBar resultProgress;
        private Label lblVerdict;
        private Button btnDetails;
        
        private TextAnalyzer analyzer;
        private FileHandlerFactory fileHandler;
        private AnalysisResult? lastResult;
        
        public MainForm()
        {
            InitializeComponent();
            analyzer = new TextAnalyzer();
            fileHandler = new FileHandlerFactory();
            SetupDragDrop();
        }
        
        private void InitializeComponent()
        {
            this.Text = "🔍 Linear Algebra AI Detector - BRAg inc.";
            this.Size = new Size(1000, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.Icon = Icon.ExtractAssociatedIcon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "icon.ico"));
            
            // Заголовок
            var header = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.FromArgb(45, 45, 65) };
            var title = new Label
            {
                Text = "🧮 BRAg inc. — Linear Algebra AI Detector",
                ForeColor = Color.FromArgb(124, 77, 255),
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            var version = new Label
            {
                Text = "Timed Garb | v1.0.0",
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 55),
                AutoSize = true
            };
            header.Controls.Add(title);
            header.Controls.Add(version);
            
            // Основная панель
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                ColumnCount = 1,
                RowCount = 5,
                BackColor = Color.FromArgb(30, 30, 46)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 180));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            
            // Drop zone для файлов
            dropZone = new Panel
            {
                BackColor = Color.FromArgb(45, 45, 65),
                BorderStyle = BorderStyle.FixedSingle,
                Height = 160,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand
            };
            
            lblDropHint = new Label
            {
                Text = "📁 Перетащите файл сюда\n\nили\n\nнажмите для выбора\n\nПоддерживаемые форматы: PDF, DOCX, TXT, RTF, ODT, HTML",
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12)
            };
            dropZone.Controls.Add(lblDropHint);
            dropZone.Click += (s, e) => SelectFile();
            
            // Панель кнопок
            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(0, 10, 0, 10) };
            
            btnSelectFile = new Button { Text = "📂 Выбрать файл", Size = new Size(130, 40), BackColor = Color.FromArgb(60, 60, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnSelectFile.Click += (s, e) => SelectFile();
            
            btnAnalyze = new Button { Text = "🔍 ПРОВЕРИТЬ НА ИИ", Size = new Size(150, 40), BackColor = Color.FromArgb(124, 77, 255), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnAnalyze.Click += async (s, e) => await AnalyzeText();
            
            btnManageMarkers = new Button { Text = "✏️ Управление маркерами", Size = new Size(160, 40), BackColor = Color.FromArgb(60, 60, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnManageMarkers.Click += (s, e) => OpenMarkerEditor();
            
            btnHistory = new Button { Text = "📜 История", Size = new Size(100, 40), BackColor = Color.FromArgb(60, 60, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnHistory.Click += (s, e) => OpenHistory();
            
            buttonPanel.Controls.Add(btnSelectFile);
            buttonPanel.Controls.Add(btnAnalyze);
            buttonPanel.Controls.Add(btnManageMarkers);
            buttonPanel.Controls.Add(btnHistory);
            
            // Текстовое поле
            txtInput = new TextBox
            {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                Font = new Font("Consolas", 11),
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Прогресс бар
            progressBar = new ProgressBar { Style = ProgressBarStyle.Marquee, Height = 5, Dock = DockStyle.Top, Visible = false };
            lblStatus = new Label { Text = "", ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 25, TextAlign = ContentAlignment.MiddleCenter };
            
            // Панель результатов
            resultPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 55), Visible = false, Padding = new Padding(15) };
            
            var resultTitle = new Label { Text = "📊 РЕЗУЛЬТАТ АНАЛИЗА", ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), Dock = DockStyle.Top, Height = 30 };
            
            var resultContent = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 3, Padding = new Padding(10) };
            resultContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            resultContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            
            resultContent.Controls.Add(new Label { Text = "Вероятность ИИ:", ForeColor = Color.White, Font = new Font("Segoe UI", 11), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            resultProgress = new ProgressBar { Height = 30, Style = ProgressBarStyle.Continuous, Value = 0 };
            lblResultPercent = new Label { Text = "0%", ForeColor = Color.FromArgb(124, 77, 255), Font = new Font("Segoe UI", 16, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
            resultContent.Controls.Add(resultProgress, 1, 0);
            resultContent.Controls.Add(lblResultPercent, 1, 1);
            
            resultContent.Controls.Add(new Label { Text = "Вердикт:", ForeColor = Color.White, Font = new Font("Segoe UI", 11), TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            lblVerdict = new Label { Text = "", ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
            resultContent.Controls.Add(lblVerdict, 1, 2);
            
            btnDetails = new Button { Text = "Подробный отчёт →", BackColor = Color.Transparent, ForeColor = Color.FromArgb(124, 77, 255), FlatStyle = FlatStyle.Flat, Dock = DockStyle.Bottom, Height = 40 };
            btnDetails.Click += (s, e) => ShowDetails();
            
            resultPanel.Controls.Add(btnDetails);
            resultPanel.Controls.Add(resultContent);
            resultPanel.Controls.Add(resultTitle);
            
            mainPanel.Controls.Add(dropZone, 0, 0);
            mainPanel.Controls.Add(buttonPanel, 0, 1);
            mainPanel.Controls.Add(txtInput, 0, 2);
            mainPanel.Controls.Add(progressBar, 0, 3);
            mainPanel.Controls.Add(lblStatus, 0, 4);
            
            this.Controls.Add(mainPanel);
            this.Controls.Add(header);
        }
        
        private void SetupDragDrop()
        {
            this.AllowDrop = true;
            this.DragEnter += (s, e) =>
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effect = DragDropEffects.Copy;
            };
            this.DragDrop += async (s, e) =>
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    await LoadFile(files[0]);
                }
            };
        }
        
        private async void SelectFile()
        {
            using var dialog = new OpenFileDialog();
            dialog.Filter = fileHandler.GetFileFilter();
            dialog.Title = "Выберите файл для анализа";
            
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                await LoadFile(dialog.FileName);
            }
        }
        
        private async Task LoadFile(string filePath)
        {
            try
            {
                progressBar.Visible = true;
                lblStatus.Text = "Загрузка файла...";
                
                var text = await fileHandler.ReadFileAsync(filePath);
                txtInput.Text = text;
                
                lblStatus.Text = $"Файл загружен: {Path.GetFileName(filePath)}";
                await AnalyzeText();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Не удалось загрузить файл:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
            }
        }
        
        private async Task AnalyzeText()
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text))
            {
                MessageBox.Show("Введите текст или загрузите файл для анализа", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                progressBar.Visible = true;
                lblStatus.Text = "Анализ текста...";
                btnAnalyze.Enabled = false;
                
                lastResult = await analyzer.Analyze(txtInput.Text);
                
                resultProgress.Value = (int)lastResult.Probability;
                lblResultPercent.Text = $"{lastResult.Probability:F1}%";
                lblVerdict.Text = lastResult.Verdict;
                lblVerdict.ForeColor = ColorTranslator.FromHtml(lastResult.VerdictColor);
                
                resultPanel.Visible = true;
                lblStatus.Text = $"Анализ завершён. Найдено маркеров: {lastResult.Matches.Count}";
                
                // Сохраняем в историю
                analyzer.SaveHistory(new DetectionHistory
                {
                    FileName = "Текстовый ввод",
                    TextSnippet = txtInput.Text.Length > 200 ? txtInput.Text[..200] + "..." : txtInput.Text,
                    Probability = lastResult.Probability,
                    TotalMarkersFound = lastResult.Matches.Count,
                    CheckedAt = DateTime.Now,
                    FullResultJson = Newtonsoft.Json.JsonConvert.SerializeObject(lastResult)
                });
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Ошибка анализа: {ex.Message}";
                MessageBox.Show($"Ошибка при анализе:\n{ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                progressBar.Visible = false;
                btnAnalyze.Enabled = true;
            }
        }
        
        private void ShowDetails()
        {
            if (lastResult != null)
            {
                var resultForm = new ResultForm(lastResult, txtInput.Text);
                resultForm.ShowDialog();
            }
        }
        
        private void OpenMarkerEditor()
        {
            var editor = new MarkerEditorForm(analyzer);
            editor.ShowDialog();
        }
        
        private void OpenHistory()
        {
            var history = new HistoryForm(analyzer);
            history.ShowDialog();
        }
    }
}