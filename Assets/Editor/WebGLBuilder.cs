using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Gera o build WebGL do jogo (o formato exigido na entrega) sempre com as
/// mesmas configuracoes: compressao Gzip + Decompression Fallback, que e a
/// combinacao que funciona no itch.io sem configurar cabecalhos no servidor.
///
/// Pelo editor:  menu "My 8 Way" > "Build WebGL".
/// Pelo terminal: bash Tools/build_webgl.sh
/// </summary>
public static class WebGLBuilder
{
    private const string OutputDirectory = "Build/WebGL";

    private static readonly string[] Scenes =
    {
        "Assets/Scenes/MainMenu.unity",
        "Assets/Scenes/SampleScene.unity",
    };

    [MenuItem("My 8 Way/Build WebGL")]
    public static void BuildFromMenu()
    {
        Run(false);
    }

    /// <summary>Chamado pelo -executeMethod no modo batch.</summary>
    public static void BuildFromCommandLine()
    {
        Run(true);
    }

    private static void Run(bool batchMode)
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.template = "APPLICATION:Default";
        PlayerSettings.runInBackground = true;

        string path = Path.Combine(Directory.GetCurrentDirectory(), OutputDirectory);
        Directory.CreateDirectory(path);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = path,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build WebGL concluido em " + path + " (" +
                      (summary.totalSize / (1024 * 1024)) + " MB, " +
                      summary.totalTime.TotalSeconds.ToString("0") + "s)");
            if (batchMode)
            {
                EditorApplication.Exit(0);
            }

            return;
        }

        Debug.LogError("Build WebGL falhou: " + summary.result + " (" + summary.totalErrors + " erros)");
        if (batchMode)
        {
            EditorApplication.Exit(1);
        }
    }
}
