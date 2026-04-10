using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LinearAlgebraDetector.Core
{
    // ==================== МОДЕЛИ ====================
    
    public class Marker
    {
        public string Id { get; set; } = string.Empty;
        public string Pattern { get; set; } = string.Empty;
        public int Weight { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ModifiedAt { get; set; }
    }

    public class MarkerMatch
    {
        public Marker Marker { get; set; } = null!;
        public int Count { get; set; }
        public int Score => Marker.Weight * Math.Min(3, Count);
        public List<string> Examples { get; set; } = new();
    }

    public class AnalysisResult
    {
        public string Text { get; set; } = string.Empty;
        public double Probability { get; set; }
        public int TotalScore { get; set; }
        public int MaxPossibleScore { get; set; }
        public List<MarkerMatch> Matches { get; set; } = new();
        public DateTime AnalyzedAt { get; set; } = DateTime.Now;
        
        public string Verdict
        {
            get
            {
                if (Probability < 20) return "Низкая вероятность — текст человека";
                if (Probability < 45) return "Средняя вероятность — есть признаки ИИ";
                if (Probability < 70) return "Высокая вероятность — вероятно ИИ";
                return "Критический уровень — почти точно ИИ";
            }
        }
        
        public string VerdictColor
        {
            get
            {
                if (Probability < 20) return "#4CAF50";
                if (Probability < 45) return "#FF9800";
                if (Probability < 70) return "#F44336";
                return "#9C27B0";
            }
        }
    }

    public class DetectionHistory
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string TextSnippet { get; set; } = string.Empty;
        public double Probability { get; set; }
        public int TotalMarkersFound { get; set; }
        public DateTime CheckedAt { get; set; }
        public string FullResultJson { get; set; } = string.Empty;
    }

    // ==================== АНАЛИЗАТОР ====================
    
    public class TextAnalyzer
    {
        private List<Marker> _markers = new();
        private readonly string _dbPath;
        
        public TextAnalyzer()
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BRAgInc", "LinearAlgebraDetector");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
            _dbPath = Path.Combine(appData, "markers.db");
            
            InitializeDatabase();
            LoadMarkers();
        }
        
        private void InitializeDatabase()
        {
            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
                using var conn = new SQLiteConnection($"Data Source={_dbPath}");
                conn.Open();
                
                string createTable = @"
                    CREATE TABLE IF NOT EXISTS Markers (
                        Id TEXT PRIMARY KEY,
                        Pattern TEXT NOT NULL,
                        Weight INTEGER NOT NULL,
                        Category TEXT NOT NULL,
                        Description TEXT,
                        Severity TEXT NOT NULL,
                        IsActive INTEGER DEFAULT 1,
                        CreatedAt TEXT,
                        ModifiedAt TEXT
                    );
                    
                    CREATE TABLE IF NOT EXISTS History (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        FileName TEXT,
                        TextSnippet TEXT,
                        Probability REAL,
                        TotalMarkersFound INTEGER,
                        CheckedAt TEXT,
                        FullResultJson TEXT
                    );";
                
                using var cmd = new SQLiteCommand(createTable, conn);
                cmd.ExecuteNonQuery();
                
                LoadDefaultMarkersToDb(conn);
            }
        }
        
        private void LoadDefaultMarkersToDb(SQLiteConnection conn)
        {
            var defaultMarkers = new List<Marker>
            {
                new() { Id = "MATH-001", Pattern = @"\btan\s*\(", Weight = 8, Category = "trigonometry", Description = "tan() вместо tg()", Severity = "critical" },
                new() { Id = "MATH-002", Pattern = @"\bcot\s*\(", Weight = 8, Category = "trigonometry", Description = "cot() вместо ctg()", Severity = "critical" },
                new() { Id = "MATH-003", Pattern = @"\bsec\s*\(", Weight = 6, Category = "trigonometry", Description = "sec() редко используется", Severity = "medium" },
                new() { Id = "MATH-004", Pattern = @"\bcsc\s*\(", Weight = 6, Category = "trigonometry", Description = "csc() редко используется", Severity = "medium" },
                new() { Id = "MATH-005", Pattern = @"\bexp\s*\(", Weight = 4, Category = "exponential", Description = "exp() вместо e^", Severity = "medium" },
                new() { Id = "MATH-006", Pattern = @"\basin\s*\(", Weight = 5, Category = "trigonometry", Description = "asin() вместо arcsin()", Severity = "medium" },
                new() { Id = "MATH-007", Pattern = @"\bacos\s*\(", Weight = 5, Category = "trigonometry", Description = "acos() вместо arccos()", Severity = "medium" },
                new() { Id = "MATH-008", Pattern = @"\batan\s*\(", Weight = 5, Category = "trigonometry", Description = "atan() вместо arctg()", Severity = "medium" },
                new() { Id = "LA-001", Pattern = @"\beigenvalue\b", Weight = 8, Category = "linear_algebra", Description = "eigenvalue вместо 'собственное значение'", Severity = "critical" },
                new() { Id = "LA-002", Pattern = @"\beigenvector\b", Weight = 8, Category = "linear_algebra", Description = "eigenvector вместо 'собственный вектор'", Severity = "critical" },
                new() { Id = "LA-003", Pattern = @"\bbasis\b", Weight = 6, Category = "linear_algebra", Description = "basis вместо 'базис'", Severity = "high" },
                new() { Id = "LA-004", Pattern = @"null space", Weight = 7, Category = "linear_algebra", Description = "null space вместо 'ядро'", Severity = "high" },
                new() { Id = "LA-005", Pattern = @"column space", Weight = 7, Category = "linear_algebra", Description = "column space вместо 'пространство столбцов'", Severity = "high" },
                new() { Id = "TEMP-001", Pattern = @"следует отметить", Weight = 5, Category = "template", Description = "Канцелярский оборот", Severity = "medium" },
                new() { Id = "TEMP-002", Pattern = @"в заключение", Weight = 4, Category = "template", Description = "Шаблон вывода", Severity = "medium" },
                new() { Id = "TEMP-003", Pattern = @"таким образом", Weight = 3, Category = "template", Description = "Шаблон вывода", Severity = "low" },
                new() { Id = "TEMP-004", Pattern = @"что и требовалось доказать", Weight = 4, Category = "template", Description = "ИИ часто добавляет", Severity = "medium" },
                new() { Id = "LEX-001", Pattern = @"то есть", Weight = 2, Category = "lexical", Description = "ИИ редко использует 'т.е.'", Severity = "low" },
                new() { Id = "LEX-002", Pattern = @"так как", Weight = 2, Category = "lexical", Description = "ИИ редко использует 'т.к.'", Severity = "low" }
            };
            
            foreach (var marker in defaultMarkers)
            {
                using var cmd = new SQLiteCommand(@"
                    INSERT OR IGNORE INTO Markers (Id, Pattern, Weight, Category, Description, Severity, IsActive, CreatedAt)
                    VALUES (@Id, @Pattern, @Weight, @Category, @Description, @Severity, @IsActive, @CreatedAt)", conn);
                
                cmd.Parameters.AddWithValue("@Id", marker.Id);
                cmd.Parameters.AddWithValue("@Pattern", marker.Pattern);
                cmd.Parameters.AddWithValue("@Weight", marker.Weight);
                cmd.Parameters.AddWithValue("@Category", marker.Category);
                cmd.Parameters.AddWithValue("@Description", marker.Description);
                cmd.Parameters.AddWithValue("@Severity", marker.Severity);
                cmd.Parameters.AddWithValue("@IsActive", 1);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
        }
        
        private void LoadMarkers()
        {
            _markers.Clear();
            using var conn = new SQLiteConnection($"Data Source={_dbPath}");
            conn.Open();
            using var cmd = new SQLiteCommand("SELECT * FROM Markers WHERE IsActive = 1", conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                _markers.Add(new Marker
                {
                    Id = reader["Id"]?.ToString() ?? "",
                    Pattern = reader["Pattern"]?.ToString() ?? "",
                    Weight = Convert.ToInt32(reader["Weight"]),
                    Category = reader["Category"]?.ToString() ?? "",
                    Description = reader["Description"]?.ToString() ?? "",
                    Severity = reader["Severity"]?.ToString() ?? "",
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    CreatedAt = DateTime.TryParse(reader["CreatedAt"]?.ToString(), out var dt) ? dt : DateTime.Now
                });
            }
        }
        
        public async Task<AnalysisResult> Analyze(string text)
        {
            return await Task.Run(() =>
            {
                var matches = new List<MarkerMatch>();
                var totalScore = 0;
                var maxPossibleScore = 0;
                
                foreach (var marker in _markers.Where(m => m.IsActive))
                {
                    var count = Regex.Matches(text, marker.Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Count;
                    maxPossibleScore += marker.Weight * 3;
                    
                    if (count > 0)
                    {
                        var match = new MarkerMatch
                        {
                            Marker = marker,
                            Count = Math.Min(3, count),
                            Examples = ExtractExamples(text, marker.Pattern, Math.Min(3, count))
                        };
                        matches.Add(match);
                        totalScore += match.Score;
                    }
                }
                
                var probability = maxPossibleScore > 0 ? Math.Min(100, (double)totalScore / maxPossibleScore * 100) : 0;
                
                return new AnalysisResult
                {
                    Text = text,
                    Probability = probability,
                    TotalScore = totalScore,
                    MaxPossibleScore = maxPossibleScore,
                    Matches = matches
                };
            });
        }
        
        private List<string> ExtractExamples(string text, string pattern, int maxExamples)
        {
            var examples = new List<string>();
            var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
            
            for (int i = 0; i < Math.Min(matches.Count, maxExamples); i++)
            {
                var match = matches[i];
                var start = Math.Max(0, match.Index - 30);
                var length = Math.Min(text.Length - start, 80);
                var snippet = text.Substring(start, length);
                examples.Add("..." + snippet + "...");
            }
            
            return examples;
        }
        
        public List<Marker> GetAllMarkers()
        {
            var all = new List<Marker>();
            using var conn = new SQLiteConnection($"Data Source={_dbPath}");
            conn.Open();
            using var cmd = new SQLiteCommand("SELECT * FROM Markers ORDER BY Category, Id", conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                all.Add(new Marker
                {
                    Id = reader["Id"]?.ToString() ?? "",
                    Pattern = reader["Pattern"]?.ToString() ?? "",
                    Weight = Convert.ToInt32(reader["Weight"]),
                    Category = reader["Category"]?.ToString() ?? "",
                    Description = reader["Description"]?.ToString() ?? "",
                    Severity = reader["Severity"]?.ToString() ?? "",
                    IsActive = Convert.ToInt32(reader["IsActive"]) == 1,
                    CreatedAt = DateTime.TryParse(reader["CreatedAt"]?.ToString(), out var dt) ? dt : DateTime.Now
                });
            }
            return all;
        }
        
        public bool AddMarker(Marker marker)
        {
            try
            {
                marker.Id = GenerateMarkerId(marker.Category);
                using var conn = new SQLiteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var cmd = new SQLiteCommand(@"
                    INSERT INTO Markers (Id, Pattern, Weight, Category, Description, Severity, IsActive, CreatedAt)
                    VALUES (@Id, @Pattern, @Weight, @Category, @Description, @Severity, @IsActive, @CreatedAt)", conn);
                
                cmd.Parameters.AddWithValue("@Id", marker.Id);
                cmd.Parameters.AddWithValue("@Pattern", marker.Pattern);
                cmd.Parameters.AddWithValue("@Weight", marker.Weight);
                cmd.Parameters.AddWithValue("@Category", marker.Category);
                cmd.Parameters.AddWithValue("@Description", marker.Description);
                cmd.Parameters.AddWithValue("@Severity", marker.Severity);
                cmd.Parameters.AddWithValue("@IsActive", marker.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                
                LoadMarkers();
                return true;
            }
            catch { return false; }
        }
        
        public bool UpdateMarker(Marker marker)
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var cmd = new SQLiteCommand(@"
                    UPDATE Markers SET Pattern=@Pattern, Weight=@Weight, Category=@Category, 
                    Description=@Description, Severity=@Severity, IsActive=@IsActive, ModifiedAt=@ModifiedAt
                    WHERE Id=@Id", conn);
                
                cmd.Parameters.AddWithValue("@Id", marker.Id);
                cmd.Parameters.AddWithValue("@Pattern", marker.Pattern);
                cmd.Parameters.AddWithValue("@Weight", marker.Weight);
                cmd.Parameters.AddWithValue("@Category", marker.Category);
                cmd.Parameters.AddWithValue("@Description", marker.Description);
                cmd.Parameters.AddWithValue("@Severity", marker.Severity);
                cmd.Parameters.AddWithValue("@IsActive", marker.IsActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@ModifiedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                
                LoadMarkers();
                return true;
            }
            catch { return false; }
        }
        
        public bool DeleteMarker(string id)
        {
            try
            {
                using var conn = new SQLiteConnection($"Data Source={_dbPath}");
                conn.Open();
                using var cmd = new SQLiteCommand("DELETE FROM Markers WHERE Id=@Id", conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.ExecuteNonQuery();
                
                LoadMarkers();
                return true;
            }
            catch { return false; }
        }
        
        private string GenerateMarkerId(string category)
        {
            var prefix = category.Length >= 3 ? category[..3].ToUpper() : category.ToUpper();
            var existingIds = GetAllMarkers().Where(m => m.Id.StartsWith(prefix)).Select(m => 
            {
                var parts = m.Id.Split('-');
                return parts.Length > 1 && int.TryParse(parts[1], out int n) ? n : 0;
            }).ToList();
            
            var maxId = existingIds.Count == 0 ? 0 : existingIds.Max();
            return $"{prefix}-{(maxId + 1):D3}";
        }
        
        public void SaveHistory(DetectionHistory history)
        {
            using var conn = new SQLiteConnection($"Data Source={_dbPath}");
            conn.Open();
            using var cmd = new SQLiteCommand(@"
                INSERT INTO History (FileName, TextSnippet, Probability, TotalMarkersFound, CheckedAt, FullResultJson)
                VALUES (@FileName, @TextSnippet, @Probability, @TotalMarkersFound, @CheckedAt, @FullResultJson)", conn);
            
            cmd.Parameters.AddWithValue("@FileName", history.FileName);
            cmd.Parameters.AddWithValue("@TextSnippet", history.TextSnippet.Length > 500 ? history.TextSnippet[..500] : history.TextSnippet);
            cmd.Parameters.AddWithValue("@Probability", history.Probability);
            cmd.Parameters.AddWithValue("@TotalMarkersFound", history.TotalMarkersFound);
            cmd.Parameters.AddWithValue("@CheckedAt", history.CheckedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("@FullResultJson", history.FullResultJson);
            cmd.ExecuteNonQuery();
        }
        
        public List<DetectionHistory> GetHistory(int limit = 100)
        {
            var history = new List<DetectionHistory>();
            using var conn = new SQLiteConnection($"Data Source={_dbPath}");
            conn.Open();
            using var cmd = new SQLiteCommand($"SELECT * FROM History ORDER BY CheckedAt DESC LIMIT {limit}", conn);
            using var reader = cmd.ExecuteReader();
            
            while (reader.Read())
            {
                history.Add(new DetectionHistory
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    FileName = reader["FileName"]?.ToString() ?? "",
                    TextSnippet = reader["TextSnippet"]?.ToString() ?? "",
                    Probability = Convert.ToDouble(reader["Probability"]),
                    TotalMarkersFound = Convert.ToInt32(reader["TotalMarkersFound"]),
                    CheckedAt = DateTime.Parse(reader["CheckedAt"]?.ToString() ?? DateTime.Now.ToString()),
                    FullResultJson = reader["FullResultJson"]?.ToString() ?? ""
                });
            }
            return history;
        }
    }
}