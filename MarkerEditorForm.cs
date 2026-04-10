using System;
using System.Drawing;
using System.Windows.Forms;
using LinearAlgebraDetector.Core;

namespace LinearAlgebraDetector.Forms
{
    public partial class MarkerEditorForm : Form
    {
        private TextAnalyzer analyzer;
        private ListBox lstMarkers;
        private TextBox txtPattern;
        private NumericUpDown nudWeight;
        private ComboBox cmbCategory;
        private ComboBox cmbSeverity;
        private TextBox txtDescription;
        private CheckBox chkActive;
        private Button btnAdd;
        private Button btnUpdate;
        private Button btnDelete;
        private Button btnClose;
        
        private Marker? selectedMarker;
        
        public MarkerEditorForm(TextAnalyzer analyzer)
        {
            this.analyzer = analyzer;
            InitializeComponent();
            LoadMarkers();
        }
        
        private void InitializeComponent()
        {
            this.Text = "✏️ Управление маркерами - BRAg inc.";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 46);
            
            var splitter = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
            splitter.SplitterDistance = 350;
            
            var leftPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 55), Padding = new Padding(10) };
            var titleLeft = new Label { Text = "📋 БАЗА МАРКЕРОВ", ForeColor = Color.White, Font = new Font("Segoe UI", 12, System.Drawing.FontStyle.Bold), Dock = DockStyle.Top, Height = 40 };
            
            lstMarkers = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 65),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                ItemHeight = 50
            };
            lstMarkers.SelectedIndexChanged += LstMarkers_SelectedIndexChanged;
            
            leftPanel.Controls.Add(lstMarkers);
            leftPanel.Controls.Add(titleLeft);
            
            var rightPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(40, 40, 55), Padding = new Padding(15) };
            
            var formLayout = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 7, Height = 350, Padding = new Padding(10) };
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            formLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            
            formLayout.Controls.Add(new Label { Text = "Регулярное выражение:", ForeColor = Color.White }, 0, 0);
            txtPattern = new TextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };
            formLayout.Controls.Add(txtPattern, 1, 0);
            
            formLayout.Controls.Add(new Label { Text = "Вес (1-10):", ForeColor = Color.White }, 0, 1);
            nudWeight = new NumericUpDown { Minimum = 1, Maximum = 10, Value = 5, Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };
            formLayout.Controls.Add(nudWeight, 1, 1);
            
            formLayout.Controls.Add(new Label { Text = "Категория:", ForeColor = Color.White }, 0, 2);
            cmbCategory = new ComboBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };
            cmbCategory.Items.AddRange(new[] { "trigonometry", "linear_algebra", "template", "lexical", "formatting", "custom" });
            formLayout.Controls.Add(cmbCategory, 1, 2);
            
            formLayout.Controls.Add(new Label { Text = "Серьёзность:", ForeColor = Color.White }, 0, 3);
            cmbSeverity = new ComboBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };
            cmbSeverity.Items.AddRange(new[] { "low", "medium", "high", "critical" });
            formLayout.Controls.Add(cmbSeverity, 1, 3);
            
            formLayout.Controls.Add(new Label { Text = "Описание:", ForeColor = Color.White }, 0, 4);
            txtDescription = new TextBox { Multiline = true, Height = 60, Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 65), ForeColor = Color.White };
            formLayout.Controls.Add(txtDescription, 1, 4);
            
            formLayout.Controls.Add(new Label { Text = "Активен:", ForeColor = Color.White }, 0, 5);
            chkActive = new CheckBox { Checked = true, Dock = DockStyle.Fill };
            formLayout.Controls.Add(chkActive, 1, 5);
            
            var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(10), FlowDirection = FlowDirection.LeftToRight };
            
            btnAdd = new Button { Text = "➕ Добавить", Size = new Size(100, 35), BackColor = Color.FromArgb(76, 175, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += (s, e) => AddMarker();
            
            btnUpdate = new Button { Text = "💾 Обновить", Size = new Size(100, 35), BackColor = Color.FromArgb(33, 150, 243), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnUpdate.Click += (s, e) => UpdateMarker();
            
            btnDelete = new Button { Text = "🗑 Удалить", Size = new Size(100, 35), BackColor = Color.FromArgb(244, 67, 54), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += (s, e) => DeleteMarker();
            
            btnClose = new Button { Text = "✖ Закрыть", Size = new Size(100, 35), BackColor = Color.FromArgb(60, 60, 80), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnClose.Click += (s, e) => Close();
            
            buttonPanel.Controls.Add(btnAdd);
            buttonPanel.Controls.Add(btnUpdate);
            buttonPanel.Controls.Add(btnDelete);
            buttonPanel.Controls.Add(btnClose);
            
            rightPanel.Controls.Add(buttonPanel);
            rightPanel.Controls.Add(formLayout);
            
            splitter.Panel1.Controls.Add(leftPanel);
            splitter.Panel2.Controls.Add(rightPanel);
            
            this.Controls.Add(splitter);
        }
        
        private void LoadMarkers()
        {
            lstMarkers.Items.Clear();
            foreach (var marker in analyzer.GetAllMarkers())
            {
                lstMarkers.Items.Add(new MarkerItemWrapper(marker, $"{marker.Id} | {marker.Category} | {(marker.IsActive ? "✅" : "❌")}"));
            }
        }
        
        private void LstMarkers_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstMarkers.SelectedItem != null)
            {
                var wrapper = (MarkerItemWrapper)lstMarkers.SelectedItem;
                selectedMarker = wrapper.Marker;
                txtPattern.Text = selectedMarker.Pattern;
                nudWeight.Value = selectedMarker.Weight;
                cmbCategory.Text = selectedMarker.Category;
                cmbSeverity.Text = selectedMarker.Severity;
                txtDescription.Text = selectedMarker.Description;
                chkActive.Checked = selectedMarker.IsActive;
            }
        }
        
        private void AddMarker()
        {
            var marker = new Marker
            {
                Pattern = txtPattern.Text,
                Weight = (int)nudWeight.Value,
                Category = cmbCategory.Text,
                Severity = cmbSeverity.Text,
                Description = txtDescription.Text,
                IsActive = chkActive.Checked
            };
            
            if (analyzer.AddMarker(marker))
            {
                LoadMarkers();
                ClearForm();
                MessageBox.Show("Маркер добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Ошибка при добавлении маркера", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateMarker()
        {
            if (selectedMarker == null) return;
            
            selectedMarker.Pattern = txtPattern.Text;
            selectedMarker.Weight = (int)nudWeight.Value;
            selectedMarker.Category = cmbCategory.Text;
            selectedMarker.Severity = cmbSeverity.Text;
            selectedMarker.Description = txtDescription.Text;
            selectedMarker.IsActive = chkActive.Checked;
            
            if (analyzer.UpdateMarker(selectedMarker))
            {
                LoadMarkers();
                MessageBox.Show("Маркер обновлён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Ошибка при обновлении маркера", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void DeleteMarker()
        {
            if (selectedMarker == null) return;
            
            if (MessageBox.Show($"Удалить маркер {selectedMarker.Id}?", "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (analyzer.DeleteMarker(selectedMarker.Id))
                {
                    LoadMarkers();
                    ClearForm();
                    MessageBox.Show("Маркер удалён!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Ошибка при удалении маркера", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private void ClearForm()
        {
            txtPattern.Text = "";
            nudWeight.Value = 5;
            cmbCategory.SelectedIndex = -1;
            cmbSeverity.SelectedIndex = -1;
            txtDescription.Text = "";
            chkActive.Checked = true;
            selectedMarker = null;
        }
        
        private class MarkerItemWrapper
        {
            public Marker Marker { get; }
            public string DisplayText { get; }
            
            public MarkerItemWrapper(Marker marker, string displayText)
            {
                Marker = marker;
                DisplayText = displayText;
            }
            
            public override string ToString()
            {
                return DisplayText;
            }
        }
    }
}