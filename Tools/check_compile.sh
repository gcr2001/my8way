#!/usr/bin/env bash
# Compila os scripts do projeto fora do Unity, usando o Roslyn do SDK do .NET e
# as mesmas referencias que o Unity gera em Assembly-CSharp.csproj.
#
# Serve como verificacao rapida de erros de compilacao sem precisar abrir o
# editor. Nao substitui o build final do Unity.
#
# Uso:
#   bash Tools/check_compile.sh

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

if [ ! -f Assembly-CSharp.csproj ]; then
  echo "Assembly-CSharp.csproj nao encontrado. Abra o projeto no Unity uma vez." >&2
  exit 1
fi

CSC="$(ls -d /usr/local/share/dotnet/sdk/*/Roslyn/bincore/csc.dll 2>/dev/null | tail -1 || true)"
if [ -z "$CSC" ]; then
  echo "csc.dll do Roslyn nao encontrado no SDK do .NET." >&2
  exit 1
fi

RSP="$(mktemp -t my8way-rsp)"
trap 'rm -f "$RSP"' EXIT

python3 - "$RSP" <<'PY'
import os
import re
import sys

rsp_path = sys.argv[1]
project = os.getcwd()

with open("Assembly-CSharp.csproj", encoding="utf-8") as handle:
    csproj = handle.read()

refs = re.findall(r"<HintPath>(.*?)</HintPath>", csproj)

# Pastas com .asmdef formam assemblies separados e nao entram no Assembly-CSharp.
excluded = set()
for folder, _dirs, files in os.walk("Assets"):
    if any(f.endswith(".asmdef") for f in files):
        excluded.add(os.path.abspath(folder))

sources = []
for folder, _dirs, files in os.walk("Assets"):
    absolute = os.path.abspath(folder)
    if any(absolute == skip or absolute.startswith(skip + os.sep) for skip in excluded):
        continue
    for name in files:
        if name.endswith(".cs"):
            sources.append(os.path.join(folder, name))

lines = [
    "-target:library",
    "-out:" + os.path.join("Temp", "CompileCheck.dll"),
    "-langversion:9.0",
    "-nostdlib+",
    "-noconfig",
    "-nologo",
    "-warn:0",
    "-define:UNITY_2023_1_OR_NEWER;UNITY_6000_0_OR_NEWER;ENABLE_INPUT_SYSTEM;UNITY_EDITOR",
]
lines += ['-r:"%s"' % r for r in refs]
lines += ['"%s"' % s for s in sorted(sources)]

with open(rsp_path, "w", encoding="utf-8") as handle:
    handle.write("\n".join(lines))

print("arquivos compilados: %d | referencias: %d" % (len(sources), len(refs)))
PY

mkdir -p Temp
dotnet exec "$CSC" "@$RSP"
echo "OK: nenhum erro de compilacao."
