using UnityEngine;

/// <summary>
/// Controla a trilha sonora e os efeitos sonoros. Vive em um objeto persistente
/// criado pelo GameBootstrap, entao sobrevive as trocas de cena (a musica nao
/// reinicia ao sair do menu para o jogo).
/// </summary>
public class AudioManager : MonoBehaviour
{
    private const string MutedKey = "my8way_muted";

    private static AudioManager instance;

    private AudioSource music;
    private AudioSource sfx;
    private AudioSource interfaceSfx;

    private AudioClip coinClip;
    private AudioClip hitClip;
    private AudioClip dashClip;
    private AudioClip waveClip;
    private AudioClip gameOverClip;
    private AudioClip victoryClip;
    private AudioClip clickClip;
    private AudioClip tickClip;
    private AudioClip spawnClip;

    private float musicVolume = 0.3f;

    public static AudioManager Instance
    {
        get { return instance != null ? instance : Ensure(); }
    }

    public bool Muted { get; private set; }

    /// <summary>Cria (uma unica vez) o objeto de audio persistente.</summary>
    public static AudioManager Ensure()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject host = new GameObject("AudioManager");
        DontDestroyOnLoad(host);
        instance = host.AddComponent<AudioManager>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Muted = PlayerPrefs.GetInt(MutedKey, 0) == 1;

        music = CreateSource("Music", true, musicVolume);
        sfx = CreateSource("Sfx", false, 0.55f);
        interfaceSfx = CreateSource("UI", false, 0.45f);

        // O som de moeda usa o asset de audio do projeto; o resto e sintetizado.
        coinClip = Resources.Load<AudioClip>("Audio/coin-recieved");
        if (coinClip == null)
        {
            coinClip = AudioSynth.Coin();
        }

        hitClip = AudioSynth.Hit();
        dashClip = AudioSynth.Dash();
        waveClip = AudioSynth.WaveClear();
        gameOverClip = AudioSynth.GameOver();
        victoryClip = AudioSynth.Victory();
        clickClip = AudioSynth.Click();
        tickClip = AudioSynth.Tick();
        spawnClip = AudioSynth.Spawn();

        music.clip = AudioSynth.Music();
        ApplyMute();
        music.Play();
    }

    private AudioSource CreateSource(string name, bool loop, float volume)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(transform, false);
        AudioSource source = child.AddComponent<AudioSource>();
        source.loop = loop;
        source.volume = volume;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.ignoreListenerPause = true;
        return source;
    }

    public void ToggleMute()
    {
        Muted = !Muted;
        PlayerPrefs.SetInt(MutedKey, Muted ? 1 : 0);
        PlayerPrefs.Save();
        ApplyMute();
        if (!Muted)
        {
            Click();
        }
    }

    /// <summary>Abaixa a musica enquanto o jogo esta pausado ou terminado.</summary>
    public void DuckMusic(bool ducked)
    {
        if (music != null)
        {
            music.volume = Muted ? 0f : (ducked ? musicVolume * 0.35f : musicVolume);
        }
    }

    public void Coin()
    {
        Play(sfx, coinClip, Random.Range(0.95f, 1.1f));
    }

    public void Hit()
    {
        Play(sfx, hitClip, Random.Range(0.92f, 1.02f));
    }

    public void DashSound()
    {
        Play(sfx, dashClip, Random.Range(0.98f, 1.06f));
    }

    public void WaveCleared()
    {
        Play(interfaceSfx, waveClip, 1f);
    }

    public void GameOverSound()
    {
        Play(interfaceSfx, gameOverClip, 1f);
    }

    public void VictorySound()
    {
        Play(interfaceSfx, victoryClip, 1f);
    }

    public void Click()
    {
        Play(interfaceSfx, clickClip, Random.Range(0.97f, 1.05f));
    }

    public void Tick()
    {
        Play(sfx, tickClip, 1f);
    }

    public void SpawnSound()
    {
        Play(sfx, spawnClip, 1f);
    }

    private void Play(AudioSource source, AudioClip clip, float pitch)
    {
        if (source == null || clip == null || Muted)
        {
            return;
        }

        source.pitch = pitch;
        source.PlayOneShot(clip);
    }

    private void ApplyMute()
    {
        if (music != null)
        {
            music.volume = Muted ? 0f : musicVolume;
        }
    }
}
