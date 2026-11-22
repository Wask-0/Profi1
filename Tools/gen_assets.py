"""Generates all raster art and audio for Labyrinthino (Pillow + wave)."""
import math
import os
import random
import struct
import wave

from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Assets", "Project")
SPR = os.path.join(ROOT, "Art", "Sprites")
BRAND = os.path.join(ROOT, "Art", "Branding")
FONT = os.path.join(ROOT, "Art", "Fonts", "Lobster-Regular.ttf")
BODY = os.path.join(ROOT, "Art", "Fonts", "Nunito-Regular.ttf")

BG_TOP = (58, 36, 112)
BG_BOTTOM = (22, 12, 52)
GOLD = (255, 207, 74)
TEAL = (62, 214, 195)
CORAL = (255, 107, 107)
CREAM = (255, 244, 220)
VIOLET = (140, 100, 230)


def save(img, *parts):
    path = os.path.join(*parts)
    os.makedirs(os.path.dirname(path), exist_ok=True)
    img.save(path)


def font(size, path=FONT):
    return ImageFont.truetype(path, size)


def ss_canvas(w, h, scale=3):
    return Image.new("RGBA", (w * scale, h * scale), (0, 0, 0, 0)), scale


def down(img, scale):
    return img.resize((img.width // scale, img.height // scale), Image.LANCZOS)


def gradient(w, h, top, bottom):
    img = Image.new("RGB", (w, h))
    px = img.load()
    for y in range(h):
        t = y / (h - 1)
        c = tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(3))
        for x in range(w):
            px[x, y] = c
    return img


def rounded(w, h, r, fill, outline=None, width=0):
    img, s = ss_canvas(w, h)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle([0, 0, w * s - 1, h * s - 1], r * s, fill=fill, outline=outline, width=width * s)
    return down(img, s)


def maze_pattern(w, h, seed, color, cell=120):
    """Draws a faint maze texture used for backgrounds."""
    rnd = random.Random(seed)
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cols, rows = w // cell + 1, h // cell + 1
    for r in range(rows):
        for c in range(cols):
            x, y = c * cell, r * cell
            if rnd.random() < 0.5:
                d.line([x, y, x + cell, y], fill=color, width=10)
            else:
                d.line([x, y, x, y + cell], fill=color, width=10)
    return img


def make_ui():
    d = os.path.join(SPR, "UI")
    save(rounded(128, 128, 36, (255, 255, 255, 255)), d, "ui_panel.png")
    save(rounded(128, 128, 36, (255, 255, 255, 255), outline=(255, 255, 255, 255), width=0), d, "ui_card.png")
    btn = Image.new("RGBA", (128, 136), (0, 0, 0, 0))
    btn.paste(rounded(128, 128, 34, (0, 0, 0, 90)), (0, 8))
    btn.alpha_composite(rounded(128, 120, 34, (255, 255, 255, 255)), (0, 0))
    save(btn, d, "ui_button.png")
    img, s = ss_canvas(128, 128)
    ImageDraw.Draw(img).ellipse([0, 0, 128 * s - 1, 128 * s - 1], fill=(255, 255, 255, 255))
    save(down(img, s), d, "ui_circle.png")
    save(rounded(96, 96, 22, (255, 255, 255, 255)), d, "ui_cell.png")
    save(Image.new("RGBA", (16, 16), (255, 255, 255, 255)), d, "ui_square.png")
    bar = rounded(128, 48, 24, (255, 255, 255, 255))
    save(bar, d, "ui_bar.png")
    sh = Image.new("RGBA", (64, 64), (0, 0, 0, 0))
    ImageDraw.Draw(sh).ellipse([8, 8, 56, 56], fill=(255, 255, 255, 255))
    save(sh.filter(ImageFilter.GaussianBlur(0.6)), d, "ui_glow.png")


def icon_canvas(size=256):
    img, s = ss_canvas(size, size)
    return img, ImageDraw.Draw(img), s


def make_icons():
    d = os.path.join(SPR, "Icons")
    white = (255, 255, 255, 255)

    img, g, s = icon_canvas(128)
    cx = cy = 64 * s
    for i in range(8):
        a = i * math.pi / 4
        g.polygon([(cx + math.cos(a - .2) * 40 * s, cy + math.sin(a - .2) * 40 * s),
                   (cx + math.cos(a - .2) * 58 * s, cy + math.sin(a - .2) * 58 * s),
                   (cx + math.cos(a + .2) * 58 * s, cy + math.sin(a + .2) * 58 * s),
                   (cx + math.cos(a + .2) * 40 * s, cy + math.sin(a + .2) * 40 * s)], fill=white)
    g.ellipse([cx - 44 * s, cy - 44 * s, cx + 44 * s, cy + 44 * s], fill=white)
    g.ellipse([cx - 18 * s, cy - 18 * s, cx + 18 * s, cy + 18 * s], fill=(0, 0, 0, 0))
    save(down(img, s), d, "icon_gear.png")

    img, g, s = icon_canvas(128)
    g.line([(86 * s, 24 * s), (40 * s, 64 * s), (86 * s, 104 * s)], fill=white, width=16 * s, joint="curve")
    g.ellipse([78 * s, 16 * s, 94 * s, 32 * s], fill=white)
    g.ellipse([78 * s, 96 * s, 94 * s, 112 * s], fill=white)
    save(down(img, s), d, "icon_back.png")

    img, g, s = icon_canvas(128)
    pts = []
    for i in range(10):
        r = 58 if i % 2 == 0 else 24
        a = -math.pi / 2 + i * math.pi / 5
        pts.append((64 * s + math.cos(a) * r * s, 68 * s + math.sin(a) * r * s))
    g.polygon(pts, fill=white)
    save(down(img, s), d, "icon_star.png")

    img, g, s = icon_canvas(128)
    g.line([(26 * s, 66 * s), (54 * s, 94 * s), (102 * s, 34 * s)], fill=white, width=18 * s, joint="curve")
    save(down(img, s), d, "icon_check.png")

    img, g, s = icon_canvas(128)
    g.rounded_rectangle([28 * s, 56 * s, 100 * s, 112 * s], 10 * s, fill=white)
    g.arc([40 * s, 12 * s, 88 * s, 76 * s], 180, 360, fill=white, width=11 * s)
    g.ellipse([58 * s, 74 * s, 70 * s, 86 * s], fill=(0, 0, 0, 0))
    save(down(img, s), d, "icon_lock.png")

    img, g, s = icon_canvas(128)
    g.polygon([(36 * s, 16 * s), (36 * s, 112 * s), (112 * s, 64 * s)], fill=white)
    save(down(img, s), d, "icon_play.png")

    img, g, s = icon_canvas(128)
    g.polygon([(14 * s, 48 * s), (38 * s, 48 * s), (66 * s, 22 * s), (66 * s, 106 * s), (38 * s, 80 * s), (14 * s, 80 * s)], fill=white)
    g.arc([62 * s, 40 * s, 94 * s, 88 * s], -60, 60, fill=white, width=8 * s)
    g.arc([62 * s, 24 * s, 114 * s, 104 * s], -60, 60, fill=white, width=8 * s)
    save(down(img, s), d, "icon_sound.png")

    img, g, s = icon_canvas(128)
    g.ellipse([20 * s, 78 * s, 56 * s, 108 * s], fill=white)
    g.rectangle([48 * s, 20 * s, 58 * s, 94 * s], fill=white)
    g.polygon([(48 * s, 20 * s), (104 * s, 34 * s), (104 * s, 56 * s), (48 * s, 42 * s)], fill=white)
    save(down(img, s), d, "icon_music.png")

    img, g, s = icon_canvas(128)
    g.line([(28 * s, 28 * s), (100 * s, 100 * s)], fill=white, width=16 * s)
    g.line([(100 * s, 28 * s), (28 * s, 100 * s)], fill=white, width=16 * s)
    save(down(img, s), d, "icon_close.png")

    img, g, s = icon_canvas(128)
    g.arc([22 * s, 22 * s, 106 * s, 106 * s], 40, 340, fill=white, width=14 * s)
    g.polygon([(86 * s, 8 * s), (116 * s, 40 * s), (80 * s, 44 * s)], fill=white)
    save(down(img, s), d, "icon_restart.png")

    img, g, s = icon_canvas(128)
    g.polygon([(64 * s, 12 * s), (116 * s, 62 * s), (104 * s, 62 * s), (104 * s, 112 * s), (76 * s, 112 * s), (76 * s, 78 * s), (52 * s, 78 * s), (52 * s, 112 * s), (24 * s, 112 * s), (24 * s, 62 * s), (12 * s, 62 * s)], fill=white)
    save(down(img, s), d, "icon_home.png")

    img, g, s = icon_canvas(128)
    g.ellipse([20 * s, 20 * s, 108 * s, 108 * s], fill=white)
    g.ellipse([34 * s, 34 * s, 94 * s, 94 * s], fill=(0, 0, 0, 0))
    g.line([(64 * s, 46 * s), (64 * s, 66 * s), (78 * s, 74 * s)], fill=white, width=8 * s, joint="curve")
    save(down(img, s), d, "icon_timer.png")

    img, g, s = icon_canvas(128)
    g.polygon([(64 * s, 108 * s), (14 * s, 56 * s), (14 * s, 36 * s), (30 * s, 20 * s), (50 * s, 20 * s), (64 * s, 36 * s),
               (78 * s, 20 * s), (98 * s, 20 * s), (114 * s, 36 * s), (114 * s, 56 * s)], fill=white)
    save(down(img, s), d, "icon_heart.png")

    make_game_icons(d)


def make_game_icons(d):
    size = 256
    bg = [(255, 170, 60), (62, 214, 195), (110, 220, 90), (255, 107, 150)]
    names = ["icon_game_quiz", "icon_game_findpath", "icon_game_meadow", "icon_game_dots"]
    for idx, name in enumerate(names):
        img, g, s = icon_canvas(size)
        g.ellipse([8 * s, 8 * s, 248 * s, 248 * s], fill=bg[idx] + (255,))
        g.ellipse([20 * s, 20 * s, 236 * s, 236 * s], fill=tuple(int(c * 0.85) for c in bg[idx]) + (255,))
        w = (255, 255, 255, 255)
        if idx == 0:
            g.rounded_rectangle([60 * s, 70 * s, 196 * s, 190 * s], 14 * s, fill=w)
            g.ellipse([104 * s, 100 * s, 152 * s, 148 * s], fill=bg[idx] + (255,))
            g.polygon([(128 * s, 36 * s), (170 * s, 70 * s), (86 * s, 70 * s)], fill=w)
            f = ImageFont.truetype(FONT, 90 * s)
            g.text((128 * s, 138 * s), "?", font=f, fill=bg[idx] + (255,), anchor="mm")
        elif idx == 1:
            pts = [(60, 190), (60, 130), (110, 130), (110, 80), (170, 80), (170, 50), (200, 50)]
            g.line([(x * s, y * s) for x, y in pts], fill=w, width=18 * s, joint="curve")
            g.ellipse([46 * s, 176 * s, 74 * s, 204 * s], fill=(255, 207, 74, 255))
            g.polygon([(190 * s, 34 * s), (214 * s, 46 * s), (190 * s, 58 * s)], fill=(255, 107, 107, 255))
        elif idx == 2:
            for r in range(3):
                for c in range(3):
                    col = (255, 255, 255, 255) if (r + c) % 2 == 0 else (150, 90, 40, 255)
                    g.rounded_rectangle([(58 + c * 46) * s, (58 + r * 46) * s, (98 + c * 46) * s, (98 + r * 46) * s], 8 * s, fill=col)
        else:
            pts = [(70, 190), (70, 110), (130, 110), (130, 70), (190, 70)]
            g.line([(x * s, y * s) for x, y in pts], fill=w, width=12 * s, joint="curve")
            for x, y in pts[::2]:
                g.ellipse([(x - 20) * s, (y - 20) * s, (x + 20) * s, (y + 20) * s], fill=(255, 207, 74, 255))
        save(down(img, s), d, name + ".png")


def make_backgrounds():
    d = os.path.join(SPR, "Backgrounds")
    w, h = 1080, 1920
    base = gradient(w, h, BG_TOP, BG_BOTTOM).convert("RGBA")
    base.alpha_composite(maze_pattern(w, h, 7, (255, 255, 255, 16)))
    glow = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    ImageDraw.Draw(glow).ellipse([w // 2 - 600, -300, w // 2 + 600, 700], fill=(150, 110, 255, 70))
    base.alpha_composite(glow.filter(ImageFilter.GaussianBlur(120)))
    save(base.convert("RGB"), d, "bg_main.png")

    # soft vignette for popups
    save(Image.new("RGBA", (8, 8), (10, 4, 30, 200)), d, "bg_dim.png")


def make_quiz_images():
    d = os.path.join(SPR, "Quiz")
    size = 512
    sky = lambda: gradient(size, size, (150, 220, 255), (225, 245, 255)).convert("RGBA")

    def base():
        img = sky()
        return img, ImageDraw.Draw(img)

    img, g = base()
    g.rectangle([0, 400, size, size], fill=(120, 200, 100))
    g.rectangle([130, 220, 382, 410], fill=(255, 214, 140))
    g.polygon([(100, 230), (256, 90), (412, 230)], fill=(220, 80, 70))
    g.rectangle([225, 300, 295, 410], fill=(120, 70, 40))
    g.rectangle([160, 260, 210, 310], fill=(150, 210, 255), outline=(90, 60, 40), width=5)
    g.rectangle([302, 260, 352, 310], fill=(150, 210, 255), outline=(90, 60, 40), width=5)
    g.rectangle([320, 110, 355, 190], fill=(150, 90, 70))
    save(img.convert("RGB"), d, "quiz_house.png")

    img = gradient(size, size, (40, 120, 190), (10, 50, 110)).convert("RGBA")
    g = ImageDraw.Draw(img)
    for i in range(6):
        g.ellipse([60 + i * 70, 400 - i * 10, 90 + i * 70, 430 - i * 10], outline=(200, 235, 255), width=3)
    g.ellipse([110, 190, 380, 330], fill=(255, 150, 60))
    g.polygon([(360, 260), (460, 190), (460, 330)], fill=(255, 120, 40))
    g.polygon([(200, 195), (250, 130), (300, 205)], fill=(255, 120, 40))
    g.ellipse([150, 235, 180, 265], fill=(255, 255, 255))
    g.ellipse([160, 243, 174, 257], fill=(20, 20, 40))
    for i in range(4):
        g.arc([230 + i * 25, 215, 270 + i * 25, 305], 270, 90, fill=(255, 200, 120), width=5)
    save(img.convert("RGB"), d, "quiz_fish.png")

    img, g = base()
    g.rectangle([0, 410, size, size], fill=(110, 190, 90))
    g.rectangle([220, 280, 292, 420], fill=(250, 235, 210))
    g.pieslice([110, 120, 402, 400], 180, 360, fill=(210, 60, 60))
    for cx, cy in [(190, 230), (270, 190), (330, 260), (240, 280)]:
        g.ellipse([cx - 22, cy - 22, cx + 22, cy + 22], fill=(255, 255, 255))
    save(img.convert("RGB"), d, "quiz_mushroom.png")

    img = gradient(size, size, (255, 190, 120), (255, 120, 120)).convert("RGBA")
    g = ImageDraw.Draw(img)
    g.rectangle([0, 340, size, size], fill=(40, 110, 190))
    for i in range(5):
        g.arc([i * 110 - 30, 350 + (i % 2) * 20, i * 110 + 80, 390 + (i % 2) * 20], 0, 180, fill=(180, 220, 255), width=4)
    g.polygon([(70, 300), (442, 300), (390, 390), (122, 390)], fill=(150, 90, 50))
    g.rectangle([250, 90, 262, 300], fill=(90, 60, 40))
    g.polygon([(268, 100), (400, 270), (268, 270)], fill=(255, 255, 255))
    g.polygon([(244, 130), (130, 270), (244, 270)], fill=(255, 220, 90))
    save(img.convert("RGB"), d, "quiz_boat.png")

    img = gradient(size, size, (255, 150, 70), (255, 220, 120)).convert("RGBA")
    g = ImageDraw.Draw(img)
    for i in range(12):
        a = i * math.pi / 6
        g.line([256 + math.cos(a) * 130, 256 + math.sin(a) * 130, 256 + math.cos(a) * 220, 256 + math.sin(a) * 220], fill=(255, 245, 160), width=18)
    g.ellipse([126, 126, 386, 386], fill=(255, 225, 70))
    g.ellipse([200, 220, 225, 245], fill=(80, 50, 20))
    g.ellipse([290, 220, 315, 245], fill=(80, 50, 20))
    g.arc([190, 230, 322, 340], 20, 160, fill=(80, 50, 20), width=9)
    save(img.convert("RGB"), d, "quiz_sun.png")


def make_branding():
    w, h = 1024, 512
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    g = ImageDraw.Draw(img)
    f = font(210)
    text = "Labyrinthino"
    bbox = g.textbbox((0, 0), text, font=f)
    tw = bbox[2] - bbox[0]
    scale = 0.86 * w / tw
    f = font(int(210 * scale))
    bbox = g.textbbox((0, 0), text, font=f)
    x = (w - (bbox[2] - bbox[0])) // 2 - bbox[0]
    y = 250 - (bbox[1] + bbox[3]) // 2 + 90
    shadow = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    ImageDraw.Draw(shadow).text((x + 6, y + 10), text, font=f, fill=(10, 0, 40, 200))
    img.alpha_composite(shadow.filter(ImageFilter.GaussianBlur(4)))
    g = ImageDraw.Draw(img)
    g.text((x, y), text, font=f, fill=GOLD, stroke_width=6, stroke_fill=(110, 50, 170))
    draw_maze_emblem(img, w // 2, 120, 92)
    save(img, BRAND, "logo_labyrinthino.png")

    icon = gradient(512, 512, (88, 52, 170), (30, 16, 70)).convert("RGBA")
    draw_maze_emblem(icon, 256, 256, 210)
    save(icon.convert("RGB"), BRAND, "app_icon.png")

    make_poster()
    make_result_art()


def draw_maze_emblem(img, cx, cy, r):
    s = 3
    layer = Image.new("RGBA", (r * 2 * s + 40, r * 2 * s + 40), (0, 0, 0, 0))
    g = ImageDraw.Draw(layer)
    c = (r * s + 20, r * s + 20)
    g.ellipse([c[0] - r * s, c[1] - r * s, c[0] + r * s, c[1] + r * s], fill=(255, 255, 255, 28), outline=GOLD, width=int(r * s * 0.06))
    for k, (rad, a0, a1) in enumerate([(0.78, 30, 330), (0.56, 200, 500), (0.34, 340, 640)]):
        rr = r * s * rad
        g.arc([c[0] - rr, c[1] - rr, c[0] + rr, c[1] + rr], a0, a1, fill=TEAL, width=int(r * s * 0.09))
    g.ellipse([c[0] - r * s * 0.09, c[1] - r * s * 0.09, c[0] + r * s * 0.09, c[1] + r * s * 0.09], fill=CORAL)
    layer = layer.resize((layer.width // s, layer.height // s), Image.LANCZOS)
    img.alpha_composite(layer, (cx - layer.width // 2, cy - layer.height // 2))


def make_poster():
    w, h = 1080, 1620
    img = gradient(w, h, (70, 40, 140), (20, 10, 48)).convert("RGBA")
    img.alpha_composite(maze_pattern(w, h, 21, (255, 255, 255, 22), 135))
    glow = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    ImageDraw.Draw(glow).ellipse([w // 2 - 420, 180, w // 2 + 420, 940], fill=(255, 190, 90, 110))
    img.alpha_composite(glow.filter(ImageFilter.GaussianBlur(100)))
    draw_maze_emblem(img, w // 2, 520, 330)
    g = ImageDraw.Draw(img)
    f = font(190)
    g.text((w // 2 + 5, 1010), "Labyrinthino", font=f, fill=(10, 0, 40), anchor="mm")
    g.text((w // 2, 1000), "Labyrinthino", font=f, fill=GOLD, anchor="mm", stroke_width=6, stroke_fill=(110, 50, 170))
    g.text((w // 2, 1150), "Сборник головоломок-лабиринтов", font=font(54), fill=CREAM, anchor="mm")
    chips = ["Викторина", "Найди путь", "Лугомания", "Соединяй точки"]
    cols = [(255, 170, 60), TEAL, (110, 220, 90), (255, 107, 150)]
    y = 1200
    for i, name in enumerate(chips):
        x0 = 90 + (i % 2) * 450
        yy = y + (i // 2) * 120
        chip = rounded(420, 96, 48, cols[i] + (255,))
        img.alpha_composite(chip, (x0, yy))
        g = ImageDraw.Draw(img)
        g.text((x0 + 210, yy + 46), name, font=font(46), fill=(40, 20, 70), anchor="mm")
    g.text((w // 2, 1500), "4 мини-игры  •  14 уровней  •  для планшета", font=font(42, BODY), fill=CREAM, anchor="mm")
    g.text((w // 2, 1565), "Найди выход. Разгадай загадку. Не потеряй нить!", font=font(40), fill=GOLD, anchor="mm")
    save(img.convert("RGB"), BRAND, "poster_labyrinthino.png")


def make_result_art():
    d = os.path.join(SPR, "UI")
    size = 512
    rays = Image.new("RGBA", (1024, 1024), (0, 0, 0, 0))
    rg = ImageDraw.Draw(rays)
    for i in range(12):
        a = i * math.pi / 6
        rg.polygon([(512, 512), (512 + math.cos(a - .13) * 520, 512 + math.sin(a - .13) * 520),
                    (512 + math.cos(a + .13) * 520, 512 + math.sin(a + .13) * 520)], fill=(255, 226, 120, 42))
    save(rays.filter(ImageFilter.GaussianBlur(3)), d, "art_rays.png")
    img, g, s = icon_canvas(size)
    g.ellipse([96 * s, 96 * s, 416 * s, 416 * s], fill=(255, 207, 74, 255))
    g.ellipse([128 * s, 128 * s, 384 * s, 384 * s], fill=(255, 226, 120, 255))
    g.ellipse([170 * s, 170 * s, 342 * s, 342 * s], fill=(255, 207, 74, 255))
    pts = []
    for i in range(10):
        r = 78 if i % 2 == 0 else 32
        a = -math.pi / 2 + i * math.pi / 5
        pts.append((256 * s + math.cos(a) * r * s, 262 * s + math.sin(a) * r * s))
    g.polygon(pts, fill=(255, 255, 255, 255))
    save(down(img, s), d, "art_victory_medal.png")

    img, g, s = icon_canvas(size)
    g.ellipse([70 * s, 150 * s, 440 * s, 400 * s], fill=(120, 110, 170, 255))
    g.ellipse([120 * s, 100 * s, 300 * s, 260 * s], fill=(120, 110, 170, 255))
    g.ellipse([240 * s, 120 * s, 380 * s, 260 * s], fill=(120, 110, 170, 255))
    g.ellipse([180 * s, 250 * s, 210 * s, 290 * s], fill=(30, 20, 60, 255))
    g.ellipse([300 * s, 250 * s, 330 * s, 290 * s], fill=(30, 20, 60, 255))
    g.arc([190 * s, 310 * s, 320 * s, 380 * s], 200, 340, fill=(30, 20, 60, 255), width=9 * s)
    for x in (170, 250, 330):
        g.polygon([(x * s, 410 * s), ((x + 14) * s, 440 * s), ((x - 14) * s, 440 * s)], fill=(120, 190, 255, 255))
    save(down(img, s), d, "art_defeat_cloud.png")


def tone(freq, dur, vol=0.5, sr=22050, shape="sine", decay=6.0):
    n = int(sr * dur)
    out = []
    for i in range(n):
        t = i / sr
        env = math.exp(-decay * t) * min(1.0, i / 120)
        ph = 2 * math.pi * freq * t
        v = math.sin(ph) if shape == "sine" else (1.0 if math.sin(ph) > 0 else -1.0) * 0.5
        out.append(v * env * vol)
    return out


def write_wav(path, samples, sr=22050):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    with wave.open(path, "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(sr)
        w.writeframes(b"".join(struct.pack("<h", int(max(-1, min(1, x)) * 32000)) for x in samples))


def seq(notes, dur, **kw):
    out = []
    for f in notes:
        out += tone(f, dur, **kw)
    return out


def make_audio():
    sfx = os.path.join(ROOT, "Audio", "Sfx")
    write_wav(os.path.join(sfx, "sfx_click.wav"), tone(660, 0.09, 0.5, decay=30))
    write_wav(os.path.join(sfx, "sfx_tap.wav"), tone(520, 0.07, 0.5, decay=35) + tone(780, 0.07, 0.4, decay=35))
    write_wav(os.path.join(sfx, "sfx_correct.wav"), seq([523, 659, 784, 1046], 0.12, vol=0.5, decay=8))
    write_wav(os.path.join(sfx, "sfx_error.wav"), tone(180, 0.28, 0.6, shape="square", decay=7))
    write_wav(os.path.join(sfx, "sfx_victory.wav"), seq([523, 659, 784, 1046, 784, 1046, 1318], 0.16, vol=0.5, decay=5))
    write_wav(os.path.join(sfx, "sfx_defeat.wav"), seq([392, 349, 311, 262], 0.22, vol=0.5, decay=4))
    write_wav(os.path.join(sfx, "sfx_step.wav"), tone(440, 0.06, 0.45, decay=40))
    pent = [262, 311, 349, 392, 466, 523, 466, 392]
    bass = [131, 131, 117, 117, 156, 156, 175, 98]
    mus = []
    sr = 22050
    step = 0.375
    for bar in range(8):
        for i in range(8):
            n = int(sr * step)
            arp = tone(pent[(i + bar) % 8] * (2 if i % 4 == 3 else 1), step, 0.28, decay=4)
            bs = tone(bass[bar % 8], step, 0.22, decay=2) if i % 2 == 0 else [0.0] * n
            mus += [a + b for a, b in zip(arp, bs if len(bs) == n else bs[:n])]
    write_wav(os.path.join(ROOT, "Audio", "Music", "music_main_theme.wav"), mus)


if __name__ == "__main__":
    make_ui()
    make_icons()
    make_backgrounds()
    make_quiz_images()
    make_branding()
    make_audio()
    print("done")
