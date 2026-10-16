using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Acoes do menu inicial. Mantida desde o tutorial (os botoes da cena
/// MainMenu apontam para estes metodos) e agora tambem usada pelo menu
/// construido por <see cref="MenuBuilder"/>.
/// </summary>
public class MenuActions : MonoBehaviour
{
    /// <summary>Comeca uma partida nova.</summary>
    public void IniciaJogo()
    {
        AudioManager.Instance.Click();
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameBootstrap.GameScene);
    }

    /// <summary>Volta para o menu inicial.</summary>
    public void Menu()
    {
        AudioManager.Instance.Click();
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameBootstrap.MenuScene);
    }

    /// <summary>Fecha o jogo (ignorado no WebGL, onde nao ha como sair).</summary>
    public void Sair()
    {
        AudioManager.Instance.Click();
        Application.Quit();
    }
}
