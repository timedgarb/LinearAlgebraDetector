using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using LinearAlgebraDetector.Core;

namespace LinearAlgebraDetector.Utils
{
    public static class ConfigManager
    {
        private static string configPath = "";
        
        public static void Initialize()
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BRAgInc", "LinearAlgebraDetector");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
            configPath = Path.Combine(appData, "config.json");
        }
        
        public static string GetTheme() => "Dark";
        public static void SetTheme(string theme) { }
    }
    
    public static class Logger
    {
        private static string logPath = "";
        
        static Logger()
        {
            var appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BRAgInc", "LinearAlgebraDetector");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
            logPath = Path.Combine(appData, "logs.txt");
        }
        
        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | INFO | " + message + Environment.NewLine);
            }
            catch { }
        }
        
        public static void LogError(string message, Exception ex)
        {
            try
            {
                File.AppendAllText(logPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | ERROR | " + message + ": " + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine);
            }
            catch { }
        }
    }
    
    public static class RegexUtils
    {
        public static List<string> FindAllMatches(string text, string pattern)
        {
            var matches = new List<string>();
            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var matchCollection = regex.Matches(text);
            
            foreach (Match match in matchCollection)
            {
                matches.Add(match.Value);
            }
            return matches;
        }
        
        public static int CountMatches(string text, string pattern)
        {
            return Regex.Matches(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant).Count;
        }
        
        public static bool ContainsPattern(string text, string pattern)
        {
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
    }
    
    public static class TextHighlighter
    {
        public static string Highlight(string text, List<MarkerMatch> matches)
        {
            if (matches == null || matches.Count == 0) return text;
            
            var result = text;
            foreach (var match in matches)
            {
                try
                {
                    string color = "#FF8800";
                    if (match.Marker.Severity == "critical") color = "#FF4444";
                    else if (match.Marker.Severity == "high") color = "#FF8800";
                    else if (match.Marker.Severity == "medium") color = "#FFDD44";
                    else color = "#AAAAAA";
                    
                    result = Regex.Replace(result, match.Marker.Pattern, 
                        "<span style='background-color:" + color + "; color:white; padding:2px 4px; border-radius:4px;'>$0</span>", 
                        RegexOptions.IgnoreCase);
                }
                catch { }
            }
            
            return "<html><body style='font-family:Consolas; font-size:12pt; background-color:#2A2A3C; color:#EEEEEE; padding:15px;'>" + result + "</body></html>";
        }
        
        public static string GetRtfHighlighted(string text, List<MarkerMatch> matches)
        {
            var rtf = new StringBuilder();
            rtf.Append("{\\rtf1\\ansi\\deff0 {\\fonttbl {\\f0 Consolas;}} \\f0\\fs24 ");
            
            var lastPos = 0;
            var allMatches = new List<Tuple<int, int, MarkerMatch>>();
            
            foreach (var match in matches)
            {
                try
                {
                    var regex = new Regex(match.Marker.Pattern, RegexOptions.IgnoreCase);
                    var matchesInText = regex.Matches(text);
                    
                    foreach (Match m in matchesInText)
                    {
                        allMatches.Add(Tuple.Create(m.Index, m.Length, match));
                    }
                }
                catch { }
            }
            
            allMatches.Sort((a, b) => a.Item1.CompareTo(b.Item1));
            
            foreach (var matchItem in allMatches)
            {
                int index = matchItem.Item1;
                int length = matchItem.Item2;
                MarkerMatch match = matchItem.Item3;
                
                if (lastPos < index)
                {
                    rtf.Append(EscapeRtf(text.Substring(lastPos, index - lastPos)));
                }
                
                string color = "170 170 170";
                if (match.Marker.Severity == "critical") color = "255 68 68";
                else if (match.Marker.Severity == "high") color = "255 136 0";
                else if (match.Marker.Severity == "medium") color = "255 221 68";
                
                rtf.Append("\\cf1\\highlight" + color + " ");
                rtf.Append(EscapeRtf(text.Substring(index, length)));
                rtf.Append("\\highlight0\\cf0 ");
                
                lastPos = index + length;
            }
            
            if (lastPos < text.Length)
            {
                rtf.Append(EscapeRtf(text.Substring(lastPos)));
            }
            
            rtf.Append("}");
            return rtf.ToString();
        }
        
        private static string EscapeRtf(string text)
        {
            return text.Replace("\\", "\\\\")
                      .Replace("{", "\\{")
                      .Replace("}", "\\}")
                      .Replace("\n", "\\par ");
        }
    }
    
    public static class HtmlExporter
    {
        public static string Export(AnalysisResult result, string fullText)
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='UTF-8'>");
            html.AppendLine("<title>Отчёт об анализе - BRAg inc.</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 40px; background: #1E1E2E; color: #EEEEEE; }");
            html.AppendLine(".container { max-width: 1200px; margin: 0 auto; background: #2A2A3C; border-radius: 16px; padding: 30px; }");
            html.AppendLine(".header { text-align: center; border-bottom: 2px solid #7C4DFF; padding-bottom: 20px; margin-bottom: 30px; }");
            html.AppendLine(".probability { font-size: 48px; font-weight: bold; text-align: center; margin: 20px 0; }");
            html.AppendLine(".verdict { text-align: center; font-size: 24px; margin: 20px 0; }");
            html.AppendLine(".markers { background: #1E1E2E; border-radius: 12px; padding: 20px; margin: 20px 0; }");
            html.AppendLine(".marker-item { border-left: 4px solid #7C4DFF; padding: 10px; margin: 10px 0; background: #252537; }");
            html.AppendLine(".text-preview { background: #1E1E2E; border-radius: 12px; padding: 20px; font-family: monospace; max-height: 400px; overflow: auto; }");
            html.AppendLine(".footer { text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #444; color: #888; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("<div class='container'>");
            html.AppendLine("<div class='header'>");
            html.AppendLine("<h1>🧮 BRAg inc. — Linear Algebra AI Detector</h1>");
            html.AppendLine("<p>Дата анализа: " + result.AnalyzedAt.ToString("dd.MM.yyyy HH:mm:ss") + "</p>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='probability'>");
            html.AppendLine("<div style='background: " + result.VerdictColor + "; width: " + result.Probability.ToString("F1") + "%; height: 40px; border-radius: 20px; line-height: 40px; text-align: center;'>");
            html.AppendLine(result.Probability.ToString("F1") + "%");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='verdict' style='color: " + result.VerdictColor + ";'>" + result.Verdict + "</div>");
            
            html.AppendLine("<div class='markers'>");
            html.AppendLine("<h2>🔴 Найденные маркеры</h2>");
            
            if (result.Matches.Count == 0)
            {
                html.AppendLine("<p>✅ Маркеры ИИ не обнаружены</p>");
            }
            else
            {
                foreach (var match in result.Matches)
                {
                    string severityIcon = "⚪";
                    if (match.Marker.Severity == "critical") severityIcon = "🔴";
                    else if (match.Marker.Severity == "high") severityIcon = "🟠";
                    else if (match.Marker.Severity == "medium") severityIcon = "🟡";
                    
                    html.AppendLine("<div class='marker-item'>");
                    html.AppendLine("<strong>" + severityIcon + " " + match.Marker.Id + "</strong> — " + match.Marker.Description + "<br/>");
                    html.AppendLine("Найдено: " + match.Count + " раз(а) | Вес: " + match.Marker.Weight + " | Очки: " + match.Score + "<br/>");
                    if (match.Examples.Count > 0)
                    {
                        html.AppendLine("Примеры: " + string.Join(", ", match.Examples));
                    }
                    html.AppendLine("</div>");
                }
            }
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='text-preview'>");
            html.AppendLine("<h2>📝 Фрагмент текста</h2>");
            string preview = fullText.Length > 1000 ? fullText.Substring(0, 1000) + "..." : fullText;
            html.AppendLine("<pre>" + System.Security.SecurityElement.Escape(preview) + "</pre>");
            html.AppendLine("</div>");
            
            html.AppendLine("<div class='footer'>");
            html.AppendLine("<p>BRAg inc. © 2026 | Разработчик: Timed Garb</p>");
            html.AppendLine("</div>");
            html.AppendLine("</div>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }
    }
    
    public static class JsonExporter
    {
        public static string Export(AnalysisResult result)
        {
            var json = new StringBuilder();
            json.AppendLine("{");
            json.AppendLine("  \"probability\": " + result.Probability + ",");
            json.AppendLine("  \"verdict\": \"" + result.Verdict + "\",");
            json.AppendLine("  \"totalScore\": " + result.TotalScore + ",");
            json.AppendLine("  \"maxPossibleScore\": " + result.MaxPossibleScore + ",");
            json.AppendLine("  \"analyzedAt\": \"" + result.AnalyzedAt.ToString("yyyy-MM-dd HH:mm:ss") + "\",");
            json.AppendLine("  \"matches\": [");
            
            for (int i = 0; i < result.Matches.Count; i++)
            {
                var match = result.Matches[i];
                json.AppendLine("    {");
                json.AppendLine("      \"id\": \"" + match.Marker.Id + "\",");
                json.AppendLine("      \"description\": \"" + match.Marker.Description + "\",");
                json.AppendLine("      \"count\": " + match.Count + ",");
                json.AppendLine("      \"score\": " + match.Score);
                json.AppendLine("    }" + (i < result.Matches.Count - 1 ? "," : ""));
            }
            
            json.AppendLine("  ]");
            json.AppendLine("}");
            
            return json.ToString();
        }
    }
    
    public static class FileDialogHelper
    {
        public static string ShowOpenFileDialog(string title, string filter)
        {
            using var dialog = new OpenFileDialog();
            dialog.Title = title;
            dialog.Filter = filter;
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
        }
        
        public static string ShowSaveFileDialog(string title, string filter, string defaultName)
        {
            using var dialog = new SaveFileDialog();
            dialog.Title = title;
            dialog.Filter = filter;
            dialog.FileName = defaultName;
            return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
        }
    }
    
    public static class ClipboardHelper
    {
        public static string GetText()
        {
            if (Clipboard.ContainsText())
            {
                return Clipboard.GetText();
            }
            return null;
        }
        
        public static void SetText(string text)
        {
            Clipboard.SetText(text);
        }
    }
    
    public static class PerformanceTimer
    {
        private static DateTime _startTime;
        
        public static void Start()
        {
            _startTime = DateTime.Now;
        }
        
        public static double Stop()
        {
            return (DateTime.Now - _startTime).TotalMilliseconds;
        }
        
        public static string GetElapsedString()
        {
            double elapsed = Stop();
            return elapsed < 1000 ? elapsed.ToString("F0") + " мс" : (elapsed / 1000).ToString("F2") + " сек";
        }
    }
}