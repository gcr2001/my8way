#!/usr/bin/env python3
"""
Valida as plantas de arena usadas por ArenaLayouts.cs.

Garante que: a borda eh fechada, existe exatamente um ponto de spawn do
jogador, todas as celulas livres (e todos os marcadores de coletavel /
inimigo / perigo) sao alcancaveis a partir do spawn e nenhum inimigo nasce
preso. Isso evita dead-end / softlock no level design.

Uso:
    python3 Tools/validate_layouts.py
"""

import sys
from collections import deque

LAYOUTS = {
    "WAVE 1": [
        "#####################",
        "#P......o.....o.....#",
        "#...##.....##.......#",
        "#.e.##..o..##...x...#",
        "#...........#.......#",
        "#..###..h..###...o..#",
        "#...........#.......#",
        "#...##..o..##...x...#",
        "#.e.##.....##.......#",
        "#......o............#",
        "#####################",
    ],
    "WAVE 2": [
        "#####################",
        "#P..o...#...#...o...#",
        "#.###.e.#.x.#.e.###.#",
        "#...#...#...#...#...#",
        "#.o.#.####.##.#.#.o.#",
        "#...x..h...h..x.....#",
        "#.o.#.####.##.#.#.o.#",
        "#...#...#...#...#...#",
        "#.###.e.#.x.#...###.#",
        "#..o....#.x.#....o..#",
        "#####################",
    ],
    "WAVE 3": [
        "#####################",
        "#P.o..x...e...x..o..#",
        "#.#####.#####.#####.#",
        "#.o...#.#h..#.#...o.#",
        "#.###.#.#.#.#.#.###.#",
        "#.x..e..o.#.o..h..x.#",
        "#.###.#.#.#.#.#.###.#",
        "#.o...#..h#.#.#...o.#",
        "#.#####.#####.#####.#",
        "#..o..x...e...x..o..#",
        "#####################",
    ],
}

WALKABLE = set(".Poehx")
DIRS = ((1, 0), (-1, 0), (0, 1), (0, -1))


def check(name, rows):
    height = len(rows)
    width = len(rows[0])
    print("=== %s  (%dx%d)" % (name, width, height))
    ok = True

    for index, row in enumerate(rows):
        if len(row) != width:
            print("  ERRO: linha %d tem %d chars (esperado %d)" % (index, len(row), width))
            ok = False

    counts = {}
    for row in rows:
        for char in row:
            counts[char] = counts.get(char, 0) + 1
    print("  coletaveis=%d patrulhas=%d cacadores=%d perigos=%d"
          % (counts.get("o", 0), counts.get("e", 0), counts.get("h", 0), counts.get("x", 0)))

    for x in range(width):
        if rows[0][x] != "#" or rows[height - 1][x] != "#":
            print("  ERRO: borda horizontal aberta em x=%d" % x)
            ok = False
    for y in range(height):
        if rows[y][0] != "#" or rows[y][width - 1] != "#":
            print("  ERRO: borda vertical aberta em y=%d" % y)
            ok = False

    starts = [(x, y) for y in range(height) for x in range(width) if rows[y][x] == "P"]
    if len(starts) != 1:
        print("  ERRO: precisa de exatamente um 'P', encontrei %d" % len(starts))
        return False

    seen = {starts[0]}
    queue = deque(starts)
    while queue:
        x, y = queue.popleft()
        for dx, dy in DIRS:
            nx, ny = x + dx, y + dy
            if 0 <= nx < width and 0 <= ny < height:
                if (nx, ny) not in seen and rows[ny][nx] in WALKABLE:
                    seen.add((nx, ny))
                    queue.append((nx, ny))

    walkable = {(x, y) for y in range(height) for x in range(width) if rows[y][x] in WALKABLE}
    unreachable = walkable - seen
    if unreachable:
        print("  ERRO: %d celulas livres inalcancaveis: %s"
              % (len(unreachable), sorted(unreachable)[:12]))
        ok = False
        blocked = [(x, y, rows[y][x]) for (x, y) in unreachable if rows[y][x] in "oehx"]
        if blocked:
            print("  ERRO CRITICO: marcadores inalcancaveis: %s" % blocked)

    for y in range(1, height - 1):
        for x in range(1, width - 1):
            if rows[y][x] in "eh":
                free = sum(1 for dx, dy in DIRS if rows[y + dy][x + dx] in WALKABLE)
                if free == 0:
                    print("  ERRO: inimigo sem saida em (%d,%d)" % (x, y))
                    ok = False

    print("  -> %s" % ("OK" if ok else "FALHOU"))
    return ok


def main():
    all_ok = True
    for name, rows in LAYOUTS.items():
        all_ok = check(name, rows) and all_ok
    print("\nRESULTADO: %s" % ("todas as arenas OK" if all_ok else "corrigir as arenas"))
    return 0 if all_ok else 1


if __name__ == "__main__":
    sys.exit(main())
