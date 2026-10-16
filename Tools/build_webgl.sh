#!/usr/bin/env bash
# Gera o build WebGL do jogo em Build/WebGL.
#
# IMPORTANTE: feche o editor do Unity antes de rodar - o Unity nao permite dois
# processos com o mesmo projeto aberto.
#
# Uso:
#   bash Tools/build_webgl.sh

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_VERSION="$(sed -n 's/^m_EditorVersion: //p' "$ROOT/ProjectSettings/ProjectVersion.txt")"
UNITY="/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity"
LOG="$ROOT/Build/webgl-build.log"

if [ ! -x "$UNITY" ]; then
  echo "Unity $UNITY_VERSION nao encontrado em $UNITY" >&2
  exit 1
fi

mkdir -p "$ROOT/Build"
echo "Compilando WebGL com Unity $UNITY_VERSION (log: $LOG)"

set +e
"$UNITY" \
  -batchmode \
  -nographics \
  -projectPath "$ROOT" \
  -buildTarget WebGL \
  -executeMethod WebGLBuilder.BuildFromCommandLine \
  -logFile "$LOG"
STATUS=$?
set -e

if [ "$STATUS" -ne 0 ]; then
  echo "FALHOU (codigo $STATUS). Ultimas linhas do log:" >&2
  grep -nE "error CS|BuildFailedException|Build WebGL falhou|Aborting" "$LOG" | tail -20 >&2 || tail -40 "$LOG" >&2
  exit "$STATUS"
fi

echo "OK: build gerado em $ROOT/Build/WebGL"
echo "Para entregar no itch.io, compacte o conteudo da pasta (index.html na raiz do zip)."
