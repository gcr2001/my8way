#!/usr/bin/env python3
"""
Adiciona as acoes "Dash" e "Pause" ao asset de Project-wide Actions, com
bindings de teclado, gamepad (Xbox) e joystick.

O script e idempotente: rodar de novo nao duplica nada.

Uso:
    python3 Tools/add_input_actions.py
"""

import json
import os
import uuid

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ASSET = os.path.join(ROOT, "Assets", "Settings", "InputSystem_Actions.inputactions")

NEW_ACTIONS = {
    "Dash": [
        ("<Keyboard>/space", "Keyboard&Mouse"),
        ("<Keyboard>/leftCtrl", "Keyboard&Mouse"),
        ("<Gamepad>/buttonSouth", "Gamepad"),
        ("<Gamepad>/rightShoulder", "Gamepad"),
        ("<Joystick>/trigger", "Joystick"),
    ],
    "Pause": [
        ("<Keyboard>/escape", "Keyboard&Mouse"),
        ("<Keyboard>/p", "Keyboard&Mouse"),
        ("<Gamepad>/start", "Gamepad"),
        ("<Joystick>/{Menu}", "Joystick"),
    ],
}


def main():
    with open(ASSET, encoding="utf-8") as handle:
        data = json.load(handle)

    player_map = None
    for entry in data["maps"]:
        if entry["name"] == "Player":
            player_map = entry
            break

    if player_map is None:
        raise SystemExit("mapa 'Player' nao encontrado em %s" % ASSET)

    existing = {action["name"] for action in player_map["actions"]}
    changed = False

    for name, bindings in NEW_ACTIONS.items():
        if name in existing:
            print("acao '%s' ja existe, nada a fazer" % name)
            continue

        player_map["actions"].append({
            "name": name,
            "type": "Button",
            "id": str(uuid.uuid4()),
            "expectedControlType": "Button",
            "processors": "",
            "interactions": "",
            "initialStateCheck": False,
        })

        for path, groups in bindings:
            player_map["bindings"].append({
                "name": "",
                "id": str(uuid.uuid4()),
                "path": path,
                "interactions": "",
                "processors": "",
                "groups": groups,
                "action": name,
                "isComposite": False,
                "isPartOfComposite": False,
            })

        changed = True
        print("acao '%s' adicionada com %d bindings" % (name, len(bindings)))

    if changed:
        with open(ASSET, "w", encoding="utf-8") as handle:
            json.dump(data, handle, indent=4)
            handle.write("\n")
        print("asset atualizado: %s" % os.path.relpath(ASSET, ROOT))


if __name__ == "__main__":
    main()
