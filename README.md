# Linear Algebra AI Detector

**Разработчик:** Timed Garb  
**Компания:** BRAg inc.  
**Версия:** 1.0.0

## Описание

Десктопное приложение для определения вероятности использования ИИ при написании учебных работ по линейной алгебре с учётом российских математических стандартов.

## Возможности

- ✅ Поддержка форматов: PDF, DOCX, TXT, RTF, ODT, HTML
- ✅ Drag & Drop файлов
- ✅ Копирование текста из буфера обмена
- ✅ Анализ текста с поиском маркеров ИИ
- ✅ Расчёт вероятности использования ИИ
- ✅ Подсветка найденных маркеров в тексте
- ✅ Экспорт отчётов в HTML и JSON
- ✅ Управление базой маркеров (добавление/редактирование/удаление)
- ✅ История проверок
- ✅ Тёмная тема оформления

## Установка

### Требования
- Windows 10/11
- .NET 8 Runtime (встроен в приложение)

### Запуск
1. Скачайте `LinearAlgebraDetector.exe`
2. Запустите файл

## Сборка из исходников

```bash
dotnet build -c Release
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
LinearAlgebraDetector/
│
├── LinearAlgebraDetector.csproj          # 1. Файл проекта
├── Program.cs                             # 2. Точка входа
├── app.manifest                           # 3. Манифест приложения
├── build.bat                              # 4. Скрипт сборки
├── publish.bat                            # 5. Скрипт публикации
│
├── Forms/
│   ├── MainForm.cs                        # 6. Главное окно
│   ├── MainForm.Designer.cs               # 7. Дизайн главного окна
│   ├── ResultForm.cs                      # 8. Окно результатов
│   ├── ResultForm.Designer.cs             # 9. Дизайн результатов
│   ├── MarkerEditorForm.cs                # 10. Редактор маркеров
│   ├── MarkerEditorForm.Designer.cs       # 11. Дизайн редактора
│   ├── HistoryForm.cs                     # 12. История проверок
│   ├── HistoryForm.Designer.cs            # 13. Дизайн истории
│   └── SplashForm.cs                      # 14. Заставка
│
├── Core/
│   └── Core.cs                            # 15. Ядро (модели, анализатор, БД)
│
├── Handlers/
│   └── FileHandlers.cs                    # 16. Обработчики файлов (PDF, DOCX, TXT и др.)
│
├── Utils/
│   └── Utils.cs                           # 17. Утилиты (логгер, экспорт, подсветка)
│
├── UI/
│   └── UI.cs                              # 18. Пользовательские контролы
│
├── Resources/
│   ├── markers.json                       # 19. База маркеров (JSON)
│   └── icon.ico                           # 20. Иконка приложения
│
├── Properties/
│   └── Resources.resx                     # 21. Ресурсы приложения
│
├── Scripts/
│   ├── init_database.sql                  # 22. Инициализация БД
│   └── import_markers.sql                 # 23. Импорт маркеров
│
└── Docs/
    ├── README.md                          # 24. Документация
    └── USER_GUIDE.md                      # 25. Руководство пользователя
# 🎓 AI Detector for Educational Works

[![Python 3.10+](https://img.shields.io/badge/python-3.10+-blue.svg)](https://www.python.org/downloads/)
[![FastAPI](https://img.shields.io/badge/FastAPI-0.104.1-green.svg)](https://fastapi.tiangolo.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](http://makeapullrequest.com)

**Инструмент для преподавателей и методистов российских технических вузов**  
Определяет вероятность использования генеративных нейросетей (ChatGPT, DeepSeek, GigaChat, Llama и др.) при написании учебных работ.

## 🎯 Особенности

### 🇷🇺 Учёт российской специфики
- **Математическая нотация**: различает `tan(x)` (ИИ) vs `tg(x)` (российский стандарт)
- **Электротехника**: детектирует `i` вместо `j` в контексте ТОЭ
- **ГОСТ и ссылки**: находит несуществующие ГОСТы и отсутствие русскоязычных источников
- **Физические величины**: анализирует использование латиницы (`kg`) vs кириллицы (`кг`)

### 📊 Комплексный анализ
- Вероятность использования ИИ от 0% до 100%
- Подсветка подозрительных фрагментов текста
- Детализированный отчёт по 10+ метрикам
- Уровень уверенности: низкий / средний / высокий

### 📁 Поддерживаемые форматы
- Прямой ввод текста (copy-paste)
- Загрузка файлов: `.pdf`, `.docx`, `.txt`, `.md`, `.rtf`

### ⚙️ Гибкая настройка
- Выбор дисциплины (Математика, Физика, Электротехника, Информатика и др.)
- Регулировка чувствительности (слабая / средняя / строгая проверка)
- Режим «экзамен» (запрет на копирование)

## 🚀 Быстрый старт

### Установка

```bash
# Клонирование репозитория
git clone https://github.com/yourusername/ai-detector-edu.git
cd ai-detector-edu

# Создание виртуального окружения
python -m venv venv
source venv/bin/activate  # Linux/Mac
# или
venv\Scripts\activate     # Windows

# Установка зависимостей
pip install -r requirements.txt

# Загрузка модели spaCy для русского языка
python -m spacy download ru_core_news_sm
