using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace LinearAlgebraDetector.Handlers
{
    // Убираем конфликт с iTextSharp.text.pdf.parser.Path
    using SysPath = System.IO.Path;
    
    public interface IFileHandler
    {
        bool CanHandle(string filePath);
        Task<string> ReadTextAsync(string filePath);
        string FileType { get; }
        string[] Extensions { get; }
    }
    
    public class FileHandlerFactory
    {
        private readonly List<IFileHandler> _handlers;
        
        public FileHandlerFactory()
        {
            _handlers = new List<IFileHandler>
            {
                new PdfHandler(),
                new DocxHandler(),
                new TxtHandler(),
                new RtfHandler(),
                new OdtHandler(),
                new HtmlHandler()
            };
        }
        
        public IFileHandler? GetHandler(string filePath)
        {
            foreach (var handler in _handlers)
            {
                if (handler.CanHandle(filePath))
                    return handler;
            }
            return null;
        }
        
        public async Task<string> ReadFileAsync(string filePath)
        {
            var handler = GetHandler(filePath);
            if (handler == null)
                throw new NotSupportedException($"Формат файла не поддерживается: {System.IO.Path.GetExtension(filePath)}");
            
            return await handler.ReadTextAsync(filePath);
        }
        
        public string[] GetAllSupportedExtensions()
        {
            var extensions = new List<string>();
            foreach (var handler in _handlers)
            {
                extensions.AddRange(handler.Extensions);
            }
            return extensions.ToArray();
        }
        
        public string GetFileFilter()
        {
            var sb = new StringBuilder();
            sb.Append("Все поддерживаемые файлы|");
            var extensions = GetAllSupportedExtensions();
            sb.Append(string.Join(";", extensions));
            
            foreach (var handler in _handlers)
            {
                sb.Append($"|{handler.FileType}|");
                sb.Append(string.Join(";", handler.Extensions));
            }
            
            sb.Append("|Все файлы (*.*)|*.*");
            return sb.ToString();
        }
    }
    
    public class PdfHandler : IFileHandler
    {
        public string FileType => "PDF документы";
        public string[] Extensions => new[] { "*.pdf" };
        
        public bool CanHandle(string filePath) => 
            filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        
        public async Task<string> ReadTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var text = new StringBuilder();
                using var reader = new PdfReader(filePath);
                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    var strategy = new SimpleTextExtractionStrategy();
                    var currentText = PdfTextExtractor.GetTextFromPage(reader, page, strategy);
                    text.Append(currentText);
                    text.AppendLine();
                }
                return text.ToString();
            });
        }
    }
    
    public class DocxHandler : IFileHandler
    {
        public string FileType => "Документы Word";
        public string[] Extensions => new[] { "*.docx", "*.doc" };
        
        public bool CanHandle(string filePath) => 
            filePath.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ||
            filePath.EndsWith(".doc", StringComparison.OrdinalIgnoreCase);
        
        public async Task<string> ReadTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var text = new StringBuilder();
                using (var wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    var body = wordDoc.MainDocumentPart?.Document.Body;
                    if (body != null)
                    {
                        foreach (var paragraph in body.Elements<Paragraph>())
                        {
                            foreach (var run in paragraph.Elements<Run>())
                            {
                                foreach (var textElement in run.Elements<DocumentFormat.OpenXml.Wordprocessing.Text>())
                                {
                                    text.Append(textElement.Text);
                                }
                            }
                            text.AppendLine();
                        }
                    }
                }
                return text.ToString();
            });
        }
    }
    
    public class TxtHandler : IFileHandler
    {
        public string FileType => "Текстовые файлы";
        public string[] Extensions => new[] { "*.txt", "*.text", "*.log", "*.csv", "*.md" };
        
        public bool CanHandle(string filePath)
        {
            var ext = System.IO.Path.GetExtension(filePath).ToLower();
            return ext == ".txt" || ext == ".text" || ext == ".log" || ext == ".csv" || ext == ".md";
        }
        
        public async Task<string> ReadTextAsync(string filePath)
        {
            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }
    }
    
    public class RtfHandler : IFileHandler
    {
        public string FileType => "RTF документы";
        public string[] Extensions => new[] { "*.rtf" };
        
        public bool CanHandle(string filePath) => 
            filePath.EndsWith(".rtf", StringComparison.OrdinalIgnoreCase);
        
        public async Task<string> ReadTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var content = File.ReadAllText(filePath);
                var sb = new StringBuilder();
                var inTag = false;
                for (int i = 0; i < content.Length; i++)
                {
                    if (content[i] == '{') { inTag = true; continue; }
                    if (content[i] == '}') { inTag = false; continue; }
                    if (content[i] == '\\' && i + 1 < content.Length && content[i + 1] == '\\') { i++; continue; }
                    if (!inTag && !char.IsControl(content[i])) sb.Append(content[i]);
                }
                return sb.ToString();
            });
        }
    }
    
    public class OdtHandler : IFileHandler
    {
        public string FileType => "OpenDocument текстовые";
        public string[] Extensions => new[] { "*.odt" };
        
        public bool CanHandle(string filePath) => 
            filePath.EndsWith(".odt", StringComparison.OrdinalIgnoreCase);
        
        public async Task<string> ReadTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                using (var archive = System.IO.Compression.ZipFile.OpenRead(filePath))
                {
                    var contentEntry = archive.GetEntry("content.xml");
                    if (contentEntry != null)
                    {
                        using var reader = new StreamReader(contentEntry.Open());
                        var content = reader.ReadToEnd();
                        var text = new StringBuilder();
                        var inTag = false;
                        for (int i = 0; i < content.Length; i++)
                        {
                            if (content[i] == '<') { inTag = true; continue; }
                            if (content[i] == '>') { inTag = false; continue; }
                            if (!inTag) text.Append(content[i]);
                        }
                        return text.ToString();
                    }
                }
                return "";
            });
        }
    }
    
    public class HtmlHandler : IFileHandler
    {
        public string FileType => "HTML страницы";
        public string[] Extensions => new[] { "*.html", "*.htm" };
        
        public bool CanHandle(string filePath) => 
            filePath.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
            filePath.EndsWith(".htm", StringComparison.OrdinalIgnoreCase);
        
        public async Task<string> ReadTextAsync(string filePath)
        {
            return await Task.Run(() =>
            {
                var html = File.ReadAllText(filePath);
                var text = new StringBuilder();
                var inTag = false;
                var inScript = false;
                
                for (int i = 0; i < html.Length; i++)
                {
                    if (i + 6 < html.Length && html.Substring(i, 7).ToLower() == "<script") inScript = true;
                    if (inScript && i + 8 < html.Length && html.Substring(i, 9).ToLower() == "</script>")
                    {
                        inScript = false;
                        i += 8;
                        continue;
                    }
                    if (inScript) continue;
                    if (html[i] == '<') { inTag = true; continue; }
                    if (html[i] == '>') { inTag = false; continue; }
                    if (!inTag && !char.IsControl(html[i])) text.Append(html[i]);
                }
                return text.ToString();
            });
        }
    }
}