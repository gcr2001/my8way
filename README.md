# MY 8 WAY — Neon Circuit Run

Jogo 2D de arcade feito na Unity 6 (URP 2D) para o **Projeto Introdução ao Unity**.
Você controla uma esfera de dados dentro de um circuito e precisa coletar todos os
fragmentos de cada setor **antes do cronômetro zerar** — e o cronômetro só volta a
subir quando você coleta.

> **Preencha antes de entregar**
> - Aluno: `SEU NOME COMPLETO`
> - Jogue no navegador (itch.io): `https://SEU-USUARIO.itch.io/my-8-way`
> - Código fonte (GitHub): `https://github.com/SEU-USUARIO/my8way`

---

## Como jogar

Objetivo: limpar os **3 setores**. Cada setor tem fragmentos de dados espalhados;
colete todos para avançar. A partida termina quando o tempo zera ou quando as
3 vidas acabam.

O tempo é o recurso central do jogo:

| Evento | Efeito no cronômetro |
| --- | --- |
| Início da partida | 45s |
| Coletar um fragmento | **+3,5s** |
| Limpar um setor | **+12s** |
| Tomar dano | **−5s** e −1 vida |
| Teto do cronômetro | 99s |

Pontuação: 100 × setor por fragmento, 500 por setor limpo, 1000 por concluir o
circuito e 10 por segundo restante no fim. O recorde fica salvo na máquina.

### Ameaças

- **Sentinela** (vermelha): patrulha um corredor em linha reta e inverte ao bater na parede.
- **Caçador** (roxo): patrulha até te enxergar e passa a perseguir; fica vermelho quando está no seu rastro.
- **Nó de energia** (laranja): parado, girando. Não bloqueia a passagem, mas atravessá-lo dói.

### Controles

| Ação | Teclado | Controle (Xbox / joystick) |
| --- | --- | --- |
| Mover (8 direções) | `WASD` ou setas | Analógico esquerdo / D-pad |
| Correr (gasta estamina) | `Shift` | `L3` |
| Investida (dash) | `Espaço` ou `Ctrl` | `A` / `RB` |
| Pausar | `Esc` ou `P` | `Start` |
| Ligar/desligar som | `M` | — |
| Navegar nos menus | Setas + `Enter` / mouse | Analógico + `A` |

Correr e dar dash consomem a barra de **estamina**; ela se recupera sozinha.

---

## Rubrica — onde cada item está implementado

| Item da rubrica | Implementação |
| --- | --- |
| **Mecânica de tempo** | `Assets/Scripts/Core/GameManager.cs`: cronômetro em contagem regressiva, tempo final mostrado no painel de fim de jogo e o tempo como recurso (fragmento devolve segundos, dano custa segundos, zerar = fim). |
| **Inimigos** | `Assets/Scripts/Enemies/`: `PatrolEnemy` (patrulha fixa), `HunterEnemy` (patrulha + perseguição com raio de detecção) e `HazardNode` (parado, causa dano). |
| **Visual** | Paleta única em `Assets/Scripts/Core/Palette.cs` e sprites próprios em `Assets/Resources/Sprites/`, gerados por `Tools/generate_sprites.py`. Sprites básicos do tutorial foram substituídos/recoloridos. |
| **Áudio** | `Assets/Scripts/Audio/AudioSynth.cs` sintetiza a trilha em loop (chiptune) e 9 efeitos distintos (coleta, dano, dash, setor limpo, fim de jogo, vitória, clique, tique do tempo, spawn). `AudioManager` mantém a música entre cenas. |
| **UI** | `Assets/Scripts/UI/HUDManager.cs`: pontos, recorde, cronômetro com barra, setor atual, fragmentos restantes, vidas (corações), barra de estamina, estado do dash, avisos e textos flutuantes. |
| **Controles** | `Assets/Scripts/Core/InputHub.cs` + ações `Dash` e `Pause` adicionadas ao asset de Project-wide Actions por `Tools/add_input_actions.py`. Gamepad/joystick funcionam no jogo e nos menus (`InputSystemUIInputModule`). |
| **Level design** | Tema "circuito neon" e 3 arenas autorais em `Assets/Scripts/Level/ArenaLayouts.cs`, montadas por `ArenaBuilder`. A dificuldade sobe por setor (mais inimigos, mais rápidos, corredores mais estreitos). |
| **Menu inicial** | `Assets/Scripts/UI/MenuBuilder.cs`: menu com o mesmo tema do jogo, recorde, "Como jogar" e créditos. `MainMenu` é a primeira cena do build. |
| **Sem dead-end / softlock** | Plantas validadas por `Tools/validate_layouts.py` (borda fechada, tudo alcançável); pausa sempre acessível; painéis de fim de jogo e de pausa sempre com *Reiniciar* e *Menu inicial*; o cronômetro impede partidas infinitas; o jogador é travado dentro da arena. |

---

## Estrutura do projeto

```
Assets/
  Editor/WebGLBuilder.cs        menu e método de build WebGL
  Prefabs/                      Player e Triangle (prefabs do tutorial)
  Resources/
    Audio/coin-recieved.mp3     efeito de coleta
    Sprites/*.png               sprites gerados por script
  Scenes/MainMenu.unity         cena inicial (índice 0 do build)
  Scenes/SampleScene.unity      cena de jogo
  Scripts/
    Audio/                      AudioManager, AudioSynth
    Core/                       GameBootstrap, GameManager, InputHub, ArtFactory,
                                Palette, PlayerLocator, CameraShake, FadeOut
    Enemies/                    EnemyBase, PatrolEnemy, HunterEnemy, HazardNode
    Level/                      ArenaBuilder, ArenaLayouts, Shard, Decor
    Player/                     PlayerMovement, PlayerHealth
    UI/                         HUDManager, MenuBuilder, MenuActions, UIFactory,
                                UIFloatText, MenuSpin
Tools/
  generate_sprites.py           gera os sprites (pixel art) e os .meta
  validate_layouts.py           valida as arenas (conectividade / softlock)
  add_input_actions.py          adiciona as ações Dash e Pause ao Input System
  check_compile.sh              compila os scripts fora do Unity
  build_webgl.sh                gera o build WebGL
```

### Nota de arquitetura

As cenas ficam enxutas de propósito: `GameBootstrap` roda via
`[RuntimeInitializeOnLoadMethod]` e monta a arena, o HUD e o menu por código.
Isso deixa o level design em texto (as plantas em `ArenaLayouts`), que pode ser
validado automaticamente antes de rodar o jogo. Os objetos colocados à mão no
tutorial (paredes e triângulos) são substituídos pela arena gerada.

---

## Rodando o projeto

Unity **6000.6.6f1** (a versão exata está em `ProjectSettings/ProjectVersion.txt`).

Abra o projeto e dê Play em `Assets/Scenes/MainMenu.unity`.

### Build WebGL

```bash
bash Tools/build_webgl.sh      # feche o editor do Unity antes
```

Ou no editor: menu **My 8 Way > Build WebGL**. A saída fica em `Build/WebGL`.

O build usa compressão **Gzip com Decompression Fallback**, que é a configuração
que carrega no itch.io sem precisar mexer em cabeçalhos do servidor. Para subir,
compacte o **conteúdo** da pasta `Build/WebGL` (com o `index.html` na raiz do zip),
marque "This file will be played in the browser" e use o viewport **1280 × 720**.

### Scripts auxiliares

```bash
python3 Tools/generate_sprites.py    # regera os sprites
python3 Tools/validate_layouts.py    # valida as arenas
bash Tools/check_compile.sh          # checa erros de compilação sem abrir o Unity
```

---

## Créditos

- **Programação, arte e level design:** autorais, feitos para este projeto.
- **Sprites:** pixel art gerada por `Tools/generate_sprites.py` (nenhum asset de terceiros).
- **Trilha sonora e efeitos:** sintetizados em tempo de execução por
  `Assets/Scripts/Audio/AudioSynth.cs` (ondas quadrada, triangular, serra e ruído).
- **Efeito de coleta** (`Assets/Resources/Audio/coin-recieved.mp3`): efeito retro de
  moeda baixado de banco de sons gratuito.
  `TODO: confirme a fonte e cole o link e o nome do autor aqui antes de entregar.`
- **Ferramentas:** Unity 6 (URP 2D), TextMesh Pro, Input System.
- **Base inicial:** tutorial "Introdução Rápida" da disciplina (movimentação,
  coletáveis e menu simples). O restante foi construído sobre essa base.
