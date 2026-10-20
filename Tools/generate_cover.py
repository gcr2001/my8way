#!/usr/bin/env python3
"""
Gera a imagem de capa e o banner da pagina do itch.io, reaproveitando os
sprites e a paleta do jogo (nada de arte de terceiros).

Uso:
    python3 Tools/generate_cover.py

Saida:
    Build/itch/cover.png   630x500  -> campo "Cover image" do itch.io
    Build/itch/banner.png  960x540  -> campo "Banner" / imagem de destaque
"""

import os

from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
SPRITES = os.path.join(ROOT, "Assets", "Resources", "Sprites")
FONT_PATH = os.path.join(ROOT, "Assets", "TextMesh Pro", "Fonts", "LiberationSans.ttf")
OUT_DIR = os.path.join(ROOT, "Build", "itch")

DEEP = (10, 14, 26, 255)
CYAN = (61, 242, 255, 255)
GREEN = (57, 255, 136, 255)
GOLD = (255, 201, 61, 255)
WHITE = (240, 250, 255, 255)
VOID = (5, 7, 15, 255)


def sprite(name, size):
    path = os.path.join(SPRITES, name + ".png")
    image = Image.open(path).convert("RGBA")
    return image.resize((size, size), Image.NEAREST)


def tiled_floor(width, height, tile_px):
    tile = sprite("floor", tile_px)
    canvas = Image.new("RGBA", (width, height), DEEP)
    for y in range(0, height, tile_px):
        for x in range(0, width, tile_px):
            canvas.alpha_composite(tile, (x, y))
    return canvas


def glow(canvas, center, radius, color, strength=110):
    layer = Image.new("RGBA", canvas.size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(layer)
    draw.ellipse(
        [center[0] - radius, center[1] - radius, center[0] + radius, center[1] + radius],
        fill=(color[0], color[1], color[2], strength),
    )
    layer = layer.filter(ImageFilter.GaussianBlur(radius * 0.45))
    canvas.alpha_composite(layer)


def outlined_text(canvas, position, text, font, fill, outline=VOID, width=5, anchor="mm"):
    draw = ImageDraw.Draw(canvas)
    draw.text(position, text, font=font, fill=outline, anchor=anchor,
              stroke_width=width, stroke_fill=outline)
    draw.text(position, text, font=font, fill=fill, anchor=anchor)


def wall_strip(canvas, y, tile_px):
    tile = sprite("wall", tile_px)
    for x in range(0, canvas.width, tile_px):
        canvas.alpha_composite(tile, (x, y))


def build(width, height, title_size, subtitle_size, tile_px, mascot_px):
    canvas = tiled_floor(width, height, tile_px)

    wall_strip(canvas, 0, tile_px)
    wall_strip(canvas, height - tile_px, tile_px)

    glow(canvas, (int(width * 0.2), int(height * 0.58)), int(height * 0.3), GREEN, 95)
    glow(canvas, (int(width * 0.82), int(height * 0.62)), int(height * 0.24), GOLD, 85)
    glow(canvas, (int(width * 0.5), int(height * 0.3)), int(height * 0.3), CYAN, 70)

    # Personagens
    mascot = sprite("player", mascot_px)
    canvas.alpha_composite(mascot, (int(width * 0.2) - mascot_px // 2,
                                    int(height * 0.58) - mascot_px // 2))

    shard = sprite("shard", int(mascot_px * 0.62))
    canvas.alpha_composite(shard, (int(width * 0.82) - shard.width // 2,
                                   int(height * 0.62) - shard.height // 2))

    sentinel = sprite("sentinel", int(mascot_px * 0.55))
    canvas.alpha_composite(sentinel, (int(width * 0.43) - sentinel.width // 2,
                                      int(height * 0.76) - sentinel.height // 2))

    hunter = sprite("hunter", int(mascot_px * 0.55))
    canvas.alpha_composite(hunter, (int(width * 0.62) - hunter.width // 2,
                                    int(height * 0.74) - hunter.height // 2))

    hazard = sprite("hazard", int(mascot_px * 0.45))
    canvas.alpha_composite(hazard, (int(width * 0.52) - hazard.width // 2,
                                    int(height * 0.5) - hazard.height // 2))

    # Faixa escura atras do titulo para o texto sempre ter contraste
    band = Image.new("RGBA", (width, int(height * 0.3)), (5, 7, 15, 200))
    canvas.alpha_composite(band, (0, int(height * 0.1)))

    title_font = ImageFont.truetype(FONT_PATH, title_size)
    subtitle_font = ImageFont.truetype(FONT_PATH, subtitle_size)

    outlined_text(canvas, (width // 2, int(height * 0.19)), "MY 8 WAY", title_font, CYAN)
    outlined_text(canvas, (width // 2, int(height * 0.325)), "NEON CIRCUIT RUN",
                  subtitle_font, GREEN, width=4)

    return canvas.convert("RGB")


def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    cover = build(630, 500, 88, 34, 50, 150)
    cover_path = os.path.join(OUT_DIR, "cover.png")
    cover.save(cover_path)
    print("gerado %s (%dx%d)" % (os.path.relpath(cover_path, ROOT), cover.width, cover.height))

    banner = build(960, 540, 118, 44, 54, 180)
    banner_path = os.path.join(OUT_DIR, "banner.png")
    banner.save(banner_path)
    print("gerado %s (%dx%d)" % (os.path.relpath(banner_path, ROOT), banner.width, banner.height))


if __name__ == "__main__":
    main()
