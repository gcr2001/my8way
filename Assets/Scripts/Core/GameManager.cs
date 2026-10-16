using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Estados possiveis de uma partida.</summary>
public enum GameState
{
    Running,
    Paused,
    GameOver,
    Victory
}

/// <summary>
/// Cerebro da partida: cronometro, pontuacao, vidas, ondas e transicoes de
/// estado. O tempo e o recurso central do jogo - ele sempre corre para baixo,
/// coletar fragmentos devolve segundos e tomar dano custa segundos.
/// </summary>
public class GameManager : MonoBehaviour
{
    public const int TotalWaves = 3;
    public const int StartLives = 3;
    public const float StartTime = 45f;
    public const float TimePerShard = 3.5f;
    public const float TimePerWave = 12f;
    public const float DamageTimePenalty = 5f;
    public const float MaxTime = 99f;

    private const string HighScoreKey = "my8way_highscore";

    private float tickTimer;
    private bool announcedLowTime;

    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; }

    public int Score { get; private set; }

    public int Lives { get; private set; }

    public int Wave { get; private set; }

    public int ShardsLeft { get; private set; }

    public int ShardsInWave { get; private set; }

    public float TimeLeft { get; private set; }

    /// <summary>Tempo total de jogo da partida (mostrado no fim).</summary>
    public float RunTime { get; private set; }

    public int HighScore { get; private set; }

    public bool NewRecord { get; private set; }

    /// <summary>Pontuacao final: inclui o bonus do tempo que sobrou.</summary>
    public int FinalScore
    {
        get { return Score + (Mathf.FloorToInt(Mathf.Max(0f, TimeLeft)) * 10); }
    }

    public event Action StatsChanged;

    public event Action<GameState> StateChanged;

    /// <summary>Mensagem curta para o HUD (texto + cor).</summary>
    public event Action<string, Color> Announced;

    /// <summary>Texto flutuante no mundo (posicao + texto + cor).</summary>
    public event Action<Vector3, string, Color> Popup;

    public static GameManager Ensure()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject host = new GameObject("GameManager");
        return host.AddComponent<GameManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
        HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        Score = 0;
        Lives = StartLives;
        Wave = 1;
        TimeLeft = StartTime;
        RunTime = 0f;
        State = GameState.Running;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (InputHub.MutePressed)
        {
            AudioManager.Instance.ToggleMute();
        }

        if (InputHub.PausePressed)
        {
            if (State == GameState.Running)
            {
                Pause();
            }
            else if (State == GameState.Paused)
            {
                Resume();
            }
        }

        if (State != GameState.Running)
        {
            return;
        }

        float delta = Time.deltaTime;
        RunTime += delta;
        TimeLeft -= delta;

        if (TimeLeft <= 10f && !announcedLowTime)
        {
            announcedLowTime = true;
            Announce("TEMPO CRITICO!", Palette.Pink);
        }

        if (TimeLeft <= 10f && TimeLeft > 0f)
        {
            tickTimer -= delta;
            if (tickTimer <= 0f)
            {
                tickTimer = 1f;
                AudioManager.Instance.Tick();
            }
        }

        if (TimeLeft <= 0f)
        {
            TimeLeft = 0f;
            EndRun(GameState.GameOver, "O TEMPO ACABOU");
        }

        StatsChanged?.Invoke();
    }

    /// <summary>Informa quantos fragmentos a onda atual tem.</summary>
    public void RegisterWave(int wave, int shardCount)
    {
        Wave = wave;
        ShardsInWave = shardCount;
        ShardsLeft = shardCount;
        announcedLowTime = TimeLeft <= 10f;
        StatsChanged?.Invoke();
    }

    public void CollectShard(Vector3 worldPosition)
    {
        if (State != GameState.Running || ShardsLeft <= 0)
        {
            return;
        }

        int points = 100 * Wave;
        Score += points;
        TimeLeft = Mathf.Min(MaxTime, TimeLeft + TimePerShard);
        ShardsLeft = Mathf.Max(0, ShardsLeft - 1);

        AudioManager.Instance.Coin();
        Popup?.Invoke(worldPosition, "+" + TimePerShard.ToString("0.0") + "s", Palette.Gold);
        StatsChanged?.Invoke();

        if (ShardsLeft <= 0)
        {
            CompleteWave();
        }
    }

    /// <summary>Dano no jogador: custa uma vida e segundos do cronometro.</summary>
    public void DamagePlayer(Vector3 worldPosition)
    {
        if (State != GameState.Running)
        {
            return;
        }

        Lives--;
        TimeLeft = Mathf.Max(0f, TimeLeft - DamageTimePenalty);
        AudioManager.Instance.Hit();
        Popup?.Invoke(worldPosition, "-" + DamageTimePenalty.ToString("0") + "s", Palette.Pink);
        StatsChanged?.Invoke();

        if (Lives <= 0)
        {
            Lives = 0;
            EndRun(GameState.GameOver, "SISTEMA COMPROMETIDO");
        }
        else
        {
            Announce("DANO! " + Lives + (Lives == 1 ? " VIDA" : " VIDAS"), Palette.Pink);
        }
    }

    private void CompleteWave()
    {
        if (Wave >= TotalWaves)
        {
            Score += 1000;
            AudioManager.Instance.VictorySound();
            EndRun(GameState.Victory, "CIRCUITO COMPLETO");
            return;
        }

        Score += 500;
        TimeLeft = Mathf.Min(MaxTime, TimeLeft + TimePerWave);
        AudioManager.Instance.WaveCleared();
        Announce("SETOR " + Wave + " LIMPO  +" + TimePerWave.ToString("0") + "s", Palette.Cyan);
        StatsChanged?.Invoke();

        ArenaBuilder arena = ArenaBuilder.Current;
        if (arena != null)
        {
            // Pequeno atraso para o aviso de setor limpo aparecer antes da troca.
            arena.ScheduleWave(Wave + 1, 1f);
        }
    }

    public void Pause()
    {
        if (State != GameState.Running)
        {
            return;
        }

        SetState(GameState.Paused);
        Time.timeScale = 0f;
        AudioManager.Instance.DuckMusic(true);
        AudioManager.Instance.Click();
    }

    public void Resume()
    {
        if (State != GameState.Paused)
        {
            return;
        }

        SetState(GameState.Running);
        Time.timeScale = 1f;
        AudioManager.Instance.DuckMusic(false);
        AudioManager.Instance.Click();
    }

    private void EndRun(GameState finalState, string headline)
    {
        if (finalState == GameState.GameOver)
        {
            AudioManager.Instance.GameOverSound();
        }

        int total = FinalScore;
        NewRecord = total > HighScore;
        if (NewRecord)
        {
            HighScore = total;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }

        Time.timeScale = 0f;
        AudioManager.Instance.DuckMusic(true);
        SetState(finalState);
        Announce(headline, finalState == GameState.Victory ? Palette.Green : Palette.Pink);
        StatsChanged?.Invoke();
    }

    private void SetState(GameState next)
    {
        State = next;
        StateChanged?.Invoke(next);
    }

    public void Announce(string message, Color color)
    {
        Announced?.Invoke(message, color);
    }

    public void ShowPopup(Vector3 worldPosition, string message, Color color)
    {
        Popup?.Invoke(worldPosition, message, color);
    }

    /// <summary>Reinicia a partida do zero (anti-softlock).</summary>
    public void Restart()
    {
        AudioManager.Instance.Click();
        Time.timeScale = 1f;
        AudioManager.Instance.DuckMusic(false);
        SceneManager.LoadScene(GameBootstrap.GameScene);
    }

    /// <summary>Volta ao menu inicial (anti-softlock).</summary>
    public void ToMenu()
    {
        AudioManager.Instance.Click();
        Time.timeScale = 1f;
        AudioManager.Instance.DuckMusic(false);
        SceneManager.LoadScene(GameBootstrap.MenuScene);
    }
}
