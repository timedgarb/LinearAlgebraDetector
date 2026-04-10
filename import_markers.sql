-- Импорт маркеров из JSON (пример)

INSERT OR IGNORE INTO Markers (Id, Pattern, Weight, Category, Description, Severity, IsActive, CreatedAt)
VALUES 
    ('MATH-001', '\btan\s*\(', 8, 'trigonometry', 'tan() вместо tg()', 'critical', 1, datetime('now')),
    ('MATH-002', '\bcot\s*\(', 8, 'trigonometry', 'cot() вместо ctg()', 'critical', 1, datetime('now')),
    ('LA-001', '\beigenvalue\b', 8, 'linear_algebra', 'eigenvalue вместо собственное значение', 'critical', 1, datetime('now')),
    ('LA-002', '\beigenvector\b', 8, 'linear_algebra', 'eigenvector вместо собственный вектор', 'critical', 1, datetime('now')),
    ('TEMP-001', 'следует отметить', 5, 'template', 'Канцелярский оборот', 'medium', 1, datetime('now'));