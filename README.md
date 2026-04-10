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
