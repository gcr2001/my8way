#!/usr/bin/env python3
"""
Gerador dos sprites de "My 8 Way - Neon Circuit".

Todos os sprites do jogo sao desenhados por este script (pixel art 32x32),
garantindo uma paleta unica e coerente e nenhum asset de terceiros.

Uso:
    python3 Tools/generate_sprites.py

Saida:
    Assets/Resources/Sprites/*.png  (+ arquivos .meta com os ajustes de import)
"""

import hashlib
import math
import os

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT_DIR = os.path.join(ROOT, "Assets", "Resources", "Sprites")

# ---------------------------------------------------------------- paleta ----
VOID = (5, 7, 15, 255)
DEEP = (16, 22, 44, 255)
WALL_DARK = (24, 33, 64, 255)
WALL_MID = (36, 50, 92, 255)
CYAN = (61, 242, 255, 255)
CYAN_DIM = (26, 109, 140, 255)
GREEN = (57, 255, 136, 255)
GREEN_DARK = (14, 122, 69, 255)
GOLD = (255, 201, 61, 255)
GOLD_DARK = (168, 112, 20, 255)
PINK = (255, 77, 109, 255)
PINK_DARK = (138, 28, 50, 255)
PURPLE = (192, 77, 255, 255)
PURPLE_DARK = (94, 28, 138, 255)
ORANGE = (255, 138, 61, 255)
ORANGE_DARK = (140, 64, 20, 255)
WHITE = (240, 250, 255, 255)
CLEAR = (0, 0, 0, 0)


# ------------------------------------------------------------- utilidades ----
def new_img(size):
    return Image.new("RGBA", (size, size), CLEAR)


def lerp(a, b, t):
    t = max(0.0, min(1.0, t))
    return tuple(int(round(a[i] + (b[i] - a[i]) * t)) for i in range(4))


def squircle_mask(size, radius, power=3.2):
    """Mascara de 'quadrado arredondado' (squircle)."""
    c = (size - 1) / 2.0
    mask = [[False] * size for _ in range(size)]
    for y in range(size):
        for x in range(size):
            dx = abs(x - c) / radius
            dy = abs(y - c) / radius
            mask[y][x] = (dx ** power + dy ** power) <= 1.0
    return mask


def circle_mask(size, radius):
    return squircle_mask(size, radius, 2.0)


def diamond_mask(size, radius):
    return squircle_mask(size, radius, 1.0)


def star_mask(size, outer, inner, points=8, phase=0.0):
    c = (size - 1) / 2.0
    mask = [[False] * size for _ in range(size)]
    for y in range(size):
        for x in range(size):
            dx = x - c
            dy = y - c
            d = math.hypot(dx, dy)
            if d < 0.0001:
                mask[y][x] = True
                continue
            ang = math.atan2(dy, dx) + phase
            wave = abs(math.cos(ang * points / 2.0))
            limit = inner + (outer - inner) * wave
            mask[y][x] = d <= limit
    return mask


def erode(mask):
    size = len(mask)
    out = [[False] * size for _ in range(size)]
    for y in range(size):
        for x in range(size):
            if not mask[y][x]:
                continue
            ok = True
            for ny, nx in ((y - 1, x), (y + 1, x), (y, x - 1), (y, x + 1)):
                if not (0 <= ny < size and 0 <= nx < size) or not mask[ny][nx]:
                    ok = False
                    break
            out[y][x] = ok
    return out


def fill(img, mask, color_fn):
    px = img.load()
    size = len(mask)
    for y in range(size):
        for x in range(size):
            if mask[y][x]:
                c = color_fn(x, y) if callable(color_fn) else color_fn
                if c is not None:
                    px[x, y] = c


def outline(img, mask, color):
    """Pinta a borda de 1px da mascara."""
    fill(img, [[mask[y][x] and not erode(mask)[y][x] for x in range(len(mask))]
               for y in range(len(mask))], color)


def body_shade(size, top, bottom):
    def fn(x, y):
        return lerp(top, bottom, y / float(size - 1))
    return fn


def rect(img, x0, y0, x1, y1, color):
    px = img.load()
    for y in range(int(y0), int(y1) + 1):
        for x in range(int(x0), int(x1) + 1):
            if 0 <= x < img.width and 0 <= y < img.height:
                px[x, y] = color


# ----------------------------------------------------------------- sprites ---
def make_player(size=32):
    """Esfera de dados do jogador: nucleo verde neon com anel ciano."""
    img = new_img(size)
    outer = circle_mask(size, 15.0)
    inner = circle_mask(size, 12.0)
    core = circle_mask(size, 6.5)

    fill(img, outer, CYAN_DIM)
    fill(img, inner, body_shade(size, GREEN, GREEN_DARK))
    outline(img, outer, lerp(CYAN, VOID, 0.35))
    fill(img, core, body_shade(size, WHITE, GREEN))

    # olhos (identidade de personagem) e brilho especular
    rect(img, 11, 12, 12, 14, VOID)
    rect(img, 19, 12, 20, 14, VOID)
    rect(img, 10, 7, 13, 8, lerp(WHITE, CLEAR, 0.25))
    return img


def make_shard(size=32):
    """Fragmento de dados coletavel."""
    img = new_img(size)
    big = diamond_mask(size, 14.0)
    mid = diamond_mask(size, 10.5)
    small = diamond_mask(size, 5.0)

    fill(img, big, GOLD_DARK)
    fill(img, mid, body_shade(size, GOLD, GOLD_DARK))
    fill(img, small, WHITE)
    outline(img, big, lerp(GOLD, VOID, 0.45))
    rect(img, 15, 4, 16, 10, lerp(WHITE, GOLD, 0.3))
    return img


def make_wall(size=32):
    """Bloco de parede: placa de circuito com trilhas cianas."""
    img = new_img(size)
    rect(img, 0, 0, size - 1, size - 1, WALL_DARK)
    rect(img, 2, 2, size - 3, size - 3, WALL_MID)

    # borda superior iluminada / inferior em sombra
    rect(img, 0, 0, size - 1, 1, lerp(CYAN, WALL_DARK, 0.55))
    rect(img, 0, size - 2, size - 1, size - 1, VOID)

    # trilhas de circuito
    rect(img, 6, 8, 25, 9, CYAN_DIM)
    rect(img, 6, 8, 7, 22, CYAN_DIM)
    rect(img, 16, 14, 25, 15, CYAN_DIM)
    rect(img, 24, 14, 25, 22, CYAN_DIM)
    rect(img, 10, 20, 22, 21, CYAN_DIM)

    # nos / vias
    for cx, cy in ((6, 8), (25, 15), (10, 21), (22, 20)):
        rect(img, cx - 1, cy - 1, cx + 1, cy + 1, CYAN)

    # rebites nos cantos
    for cx, cy in ((3, 3), (size - 4, 3), (3, size - 5), (size - 4, size - 5)):
        rect(img, cx, cy, cx + 1, cy + 1, lerp(CYAN, WALL_DARK, 0.3))
    return img


def make_floor(size=32):
    """Piso do circuito, bem escuro, com grid discreto."""
    img = new_img(size)
    rect(img, 0, 0, size - 1, size - 1, DEEP)
    rect(img, 0, 0, size - 1, 0, lerp(DEEP, CYAN, 0.10))
    rect(img, 0, 0, 0, size - 1, lerp(DEEP, CYAN, 0.10))
    for i in range(4, size, 8):
        for j in range(4, size, 8):
            rect(img, i, j, i, j, lerp(DEEP, CYAN, 0.22))
    return img


def make_sentinel(size=32):
    """Inimigo de patrulha: chassi quadrado com barra de varredura."""
    img = new_img(size)
    chassis = squircle_mask(size, 14.0, 4.5)
    inner = squircle_mask(size, 11.0, 4.5)

    fill(img, chassis, PINK_DARK)
    fill(img, inner, body_shade(size, PINK, PINK_DARK))
    outline(img, chassis, VOID)

    rect(img, 8, 14, 23, 17, VOID)
    rect(img, 13, 15, 18, 16, WHITE)
    rect(img, 9, 8, 22, 9, lerp(WHITE, PINK, 0.45))
    rect(img, 6, 20, 25, 21, PINK_DARK)
    return img


def make_hunter(size=32):
    """Inimigo cacador: casco hexagonal com visor triangular."""
    img = new_img(size)
    hull = squircle_mask(size, 14.5, 1.6)
    inner = squircle_mask(size, 11.0, 1.6)

    fill(img, hull, PURPLE_DARK)
    fill(img, inner, body_shade(size, PURPLE, PURPLE_DARK))
    outline(img, hull, VOID)

    # visor triangular apontando para cima
    px = img.load()
    for y in range(9, 20):
        half = (y - 9) // 2
        for x in range(15 - half, 17 + half):
            if 0 <= x < size:
                px[x, y] = VOID if y < 18 else lerp(CYAN, VOID, 0.2)
    rect(img, 14, 15, 17, 17, CYAN)
    rect(img, 11, 6, 20, 7, lerp(WHITE, PURPLE, 0.4))
    return img


def make_hazard(size=32):
    """No de energia estatico: espinhos laranja."""
    img = new_img(size)
    spikes = star_mask(size, 15.0, 6.0, points=8)
    core = circle_mask(size, 7.0)
    pit = circle_mask(size, 3.5)

    fill(img, spikes, body_shade(size, ORANGE, ORANGE_DARK))
    outline(img, spikes, VOID)
    fill(img, core, ORANGE_DARK)
    fill(img, pit, WHITE)
    outline(img, core, VOID)
    return img


def make_heart(size=16):
    """Icone de vida."""
    img = new_img(size)
    px = img.load()
    pattern = [
        "..##..##..",
        ".########.",
        ".########.",
        ".########.",
        "..######..",
        "...####...",
        "....##....",
    ]
    ox, oy = 3, 4
    for j, row in enumerate(pattern):
        for i, ch in enumerate(row):
            if ch == "#":
                shade = lerp(PINK, PINK_DARK, j / float(len(pattern) - 1))
                px[ox + i, oy + j] = shade
    # contorno simples
    for j, row in enumerate(pattern):
        for i, ch in enumerate(row):
            if ch != "#":
                continue
            for dj, di in ((-1, 0), (1, 0), (0, -1), (0, 1)):
                nj, ni = j + dj, i + di
                if not (0 <= nj < len(pattern) and 0 <= ni < len(row)) or pattern[nj][ni] != "#":
                    x, y = ox + i + di, oy + j + dj
                    if 0 <= x < size and 0 <= y < size:
                        px[x, y] = VOID
    return img


def make_glow(size=64):
    """Halo radial usado em efeitos e no fundo do menu."""
    img = new_img(size)
    px = img.load()
    c = (size - 1) / 2.0
    for y in range(size):
        for x in range(size):
            d = math.hypot(x - c, y - c) / c
            if d >= 1.0:
                continue
            a = (1.0 - d) ** 2.4
            px[x, y] = (WHITE[0], WHITE[1], WHITE[2], int(255 * a))
    return img


def make_pixel(size=8):
    """Quadrado branco solido: base para barras, bordas e paineis."""
    img = new_img(size)
    rect(img, 0, 0, size - 1, size - 1, (255, 255, 255, 255))
    return img


# -------------------------------------------------------------------- meta ---
META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
TextureImporter:
  internalIDToNameTable: []
  externalObjects: {{}}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 0
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {{x: 0.5, y: 0.5}}
  spritePixelsToUnits: {ppu}
  spriteBorder: {{x: 0, y: 0, z: 0, w: 0}}
  spriteGenerateFallbackPhysicsShape: 1
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationMethod: 0
  spriteTessellationDetail: -1
  spriteGeometrySubdivision: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData: 
    physicsShape: []
    bones: []
    spriteID: 5e97eb03825dee720800000000000000
    internalID: 0
    vertices: []
    indices: 
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {{}}
  mipmapLimitGroupName: 
  pSDRemoveMatte: 0
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""


def stable_guid(name):
    """GUID deterministico para o import ficar estavel entre execucoes."""
    return hashlib.md5(("my8way/sprite/" + name).encode("utf-8")).hexdigest()


SPRITES = [
    ("player", make_player, 32),
    ("shard", make_shard, 32),
    ("wall", make_wall, 32),
    ("floor", make_floor, 32),
    ("sentinel", make_sentinel, 32),
    ("hunter", make_hunter, 32),
    ("hazard", make_hazard, 32),
    ("heart", make_heart, 32),
    ("glow", make_glow, 32),
    ("pixel", make_pixel, 32),
]


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    for name, fn, ppu in SPRITES:
        img = fn()
        path = os.path.join(OUT_DIR, name + ".png")
        img.save(path)
        with open(path + ".meta", "w", encoding="utf-8") as fh:
            fh.write(META_TEMPLATE.format(guid=stable_guid(name), ppu=ppu))
        print("gerado {0} ({1}x{2})".format(os.path.relpath(path, ROOT), img.width, img.height))


if __name__ == "__main__":
    main()
