"""Builds the pitch presentation Docs/Labyrinthino_Pitch.pptx from screenshots."""
import os

from pptx import Presentation
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN
from pptx.util import Emu, Inches, Pt

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..")
SHOTS = os.path.join(ROOT, "Docs", "Screens")
BRAND = os.path.join(ROOT, "Assets", "Project", "Art", "Branding")
BG = RGBColor(0x1E, 0x10, 0x46)
GOLD = RGBColor(0xFF, 0xCF, 0x4A)
CREAM = RGBColor(0xFF, 0xF4, 0xDC)
TEAL = RGBColor(0x3E, 0xD6, 0xC3)

prs = Presentation()
prs.slide_width = Inches(13.333)
prs.slide_height = Inches(7.5)
blank = prs.slide_layouts[6]


def slide():
    s = prs.slides.add_slide(blank)
    s.background.fill.solid()
    s.background.fill.fore_color.rgb = BG
    return s


def text(s, x, y, w, h, content, size, color=CREAM, bold=False, align=PP_ALIGN.LEFT, font="Segoe UI"):
    box = s.shapes.add_textbox(Inches(x), Inches(y), Inches(w), Inches(h))
    frame = box.text_frame
    frame.word_wrap = True
    lines = content if isinstance(content, list) else [content]
    for i, line in enumerate(lines):
        p = frame.paragraphs[0] if i == 0 else frame.add_paragraph()
        p.alignment = align
        run = p.add_run()
        run.text = line
        run.font.size = Pt(size)
        run.font.bold = bold
        run.font.name = font
        run.font.color.rgb = color
        p.space_after = Pt(10)


def picture(s, path, x, y, h):
    s.shapes.add_picture(path, Inches(x), Inches(y), height=Inches(h))


def title(s, content):
    text(s, 0.6, 0.35, 12, 1, content, 40, GOLD, True)


s = slide()
picture(s, os.path.join(BRAND, "poster_labyrinthino.png"), 8.6, 0.3, 6.9)
picture(s, os.path.join(BRAND, "logo_labyrinthino.png"), 0.5, 1.2, 3.6)
text(s, 0.7, 4.2, 7.5, 1, "Сборник головоломок-лабиринтов для планшета", 28, CREAM)
text(s, 0.7, 5.2, 7.5, 1, "Питч игры • Региональный этап «Профессионалы» 2026", 18, TEAL)

s = slide()
title(s, "Об игре")
text(s, 0.6, 1.5, 7.5, 5, [
    "Название: Labyrinthino",
    "Жанр: казуальные головоломки, мини-игры, лабиринты",
    "Платформа: планшет, портретная ориентация",
    "Целевая аудитория: 6+ лет, семьи, любители коротких игровых сессий (3–5 минут)",
    "Идея: четыре разных головоломки объединены общей картой, прогрессом и сохранением",
], 24)
picture(s, os.path.join(SHOTS, "map.png"), 9.3, 0.4, 6.6)

for name, caption, lines in [
    ("quiz.png", "Викторина", ["Картинка скрыта маской, видно 25 %", "Собери слово из букв, 3 попытки и таймер",
                               "Каждая ошибка открывает больше"]),
    ("findpath.png", "Найди путь", ["Запомни путь за 3 секунды", "Пройди его по клеткам, обходя препятствия",
                                    "Кнопка «Сбросить» — снова на старт"]),
    ("meadow.png", "Лугомания", ["Покрась все коричневые клетки в зелёный", "Ограничены ходы и время",
                                 "Лишние касания тратят ходы"]),
    ("dots.png", "Соединяй точки", ["Маршрут начинается только со стартовой точки", "Пройди все звёзды и дойди до домика",
                                    "Нельзя пересекать себя и препятствия"]),
]:
    s = slide()
    title(s, "Механика: " + caption)
    text(s, 0.6, 1.6, 7.5, 5, lines, 26)
    picture(s, os.path.join(SHOTS, name), 9.3, 0.4, 6.6)

s = slide()
title(s, "Победа, поражение и прогресс")
picture(s, os.path.join(SHOTS, "victory.png"), 0.8, 1.2, 6.0)
picture(s, os.path.join(SHOTS, "defeat.png"), 4.9, 1.2, 6.0)
text(s, 9.1, 1.6, 4, 5, ["Анимированные окна с частицами", "Прогресс и громкость сохраняются",
                         "Пройденную игру нельзя повторить"], 22)

s = slide()
title(s, "Производительность")
text(s, 0.6, 1.5, 12, 5, [
    "Лёгкая 2D-игра: один Canvas на uGUI, без физики и динамического света",
    "Целевая частота кадров 60 FPS, анимации на корутинах без сторонних плагинов",
    "Спрайты 2D, 4 звука по ~20 КБ и одна музыкальная петля — сборка около 75 МБ",
    "Сохранение в компактный JSON с резервной копией, запись только при выходе из уровня",
    "Адаптивный интерфейс: Canvas Scaler 1080×1920 (Expand) подходит разным планшетам в портрете",
], 24)

s = slide()
title(s, "Чем Labyrinthino особенный")
text(s, 0.6, 1.5, 12, 5, [
    "Четыре разных механики на одной сквозной карте",
    "Единый стиль «лабиринт»: фирменный логотип, шрифт и палитра",
    "Живой интерфейс: покачивающиеся карточки, анимированная загрузка, конфетти и дождь",
    "Чистая архитектура: логика в тестируемых классах, 11 автотестов",
], 26)
text(s, 0.6, 6.4, 12, 1, "Спасибо за внимание!", 32, GOLD, True)

prs.save(os.path.join(ROOT, "Docs", "Labyrinthino_Pitch.pptx"))
print("saved")
