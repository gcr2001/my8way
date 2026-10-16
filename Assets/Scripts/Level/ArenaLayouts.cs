using UnityEngine;

/// <summary>
/// Plantas das arenas, uma por onda. Cada arena e uma grade 21x11 de celulas de
/// 1 unidade. As plantas sao validadas por Tools/validate_layouts.py, que
/// garante borda fechada, spawn unico do jogador e que todo coletavel / inimigo
/// e alcancavel (nenhum dead-end ou softlock de level design).
///
/// Legenda:
///   '#' parede    '.' piso livre   'P' spawn do jogador
///   'o' fragmento 'e' sentinela (patrulha)
///   'h' cacador   'x' no de energia (perigo estatico)
/// </summary>
public static class ArenaLayouts
{
    public const char Wall = '#';
    public const char Floor = '.';
    public const char PlayerSpawn = 'P';
    public const char ShardSpawn = 'o';
    public const char PatrolSpawn = 'e';
    public const char HunterSpawn = 'h';
    public const char HazardSpawn = 'x';

    /// <summary>Setor 1 - arena aberta, ensina o ritmo do cronometro.</summary>
    private static readonly string[] Sector1 =
    {
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
    };

    /// <summary>Setor 2 - corredores estreitos, mais cacadores e perigos.</summary>
    private static readonly string[] Sector2 =
    {
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
    };

    /// <summary>Setor 3 - labirinto final, maxima pressao de tempo.</summary>
    private static readonly string[] Sector3 =
    {
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
    };

    private static readonly string[][] All = { Sector1, Sector2, Sector3 };

    public static int SectorCount
    {
        get { return All.Length; }
    }

    /// <summary>Planta da onda informada (1-based), limitada ao total existente.</summary>
    public static string[] ForWave(int wave)
    {
        return All[Mathf.Clamp(wave - 1, 0, All.Length - 1)];
    }

    /// <summary>Nome tematico do setor, usado no HUD.</summary>
    public static string NameForWave(int wave)
    {
        switch (Mathf.Clamp(wave - 1, 0, All.Length - 1))
        {
            case 0:
                return "NUCLEO EXTERNO";
            case 1:
                return "BARRAMENTO CENTRAL";
            default:
                return "FIREWALL";
        }
    }
}
