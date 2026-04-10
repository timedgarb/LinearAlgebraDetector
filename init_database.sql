-- Инициализация базы данных маркеров

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
);

CREATE INDEX idx_markers_category ON Markers(Category);
CREATE INDEX idx_markers_active ON Markers(IsActive);
CREATE INDEX idx_history_date ON History(CheckedAt);