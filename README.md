# LinearAlgebraDetector
AI Detector for Russian Technical Universities — инструмент для преподавателей, определяющий вероятность использования ИИ (ChatGPT, DeepSeek, GigaChat) в студенческих работах с учётом российской математической нотации, ГОСТов и технических дисциплин.
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
