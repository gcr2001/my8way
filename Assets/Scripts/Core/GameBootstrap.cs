using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ponto de entrada do jogo. Roda automaticamente depois de qualquer carga de
/// cena (sem precisar de nenhum objeto montado a mao) e decide o que construir:
/// o menu inicial ou a arena da partida.
/// </summary>
public static class GameBootstrap
{
    public const string MenuScene = "MainMenu";
    public const string GameScene = "SampleScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;

        // A primeira cena ja esta carregada quando este metodo roda.
        BuildFor(SceneManager.GetActiveScene());
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BuildFor(scene);
    }

    private static void BuildFor(Scene scene)
    {
        Time.timeScale = 1f;
        Application.targetFrameRate = -1;

        AudioManager.Ensure();
        AudioManager.Instance.DuckMusic(false);

        if (scene.name == MenuScene)
        {
            MenuBuilder.Build();
            return;
        }

        bool gameplayScene = scene.name == GameScene ||
                             Object.FindAnyObjectByType<PlayerMovement>() != null;
        if (gameplayScene)
        {
            ArenaBuilder.Build();
        }
    }
}
