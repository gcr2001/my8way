using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Acesso centralizado as acoes do asset "Project-wide Actions"
/// (Assets/Settings/InputSystem_Actions). Todas as acoes tem binding de teclado
/// e de gamepad/joystick, entao o jogo funciona com controle de Xbox sem
/// nenhuma configuracao extra.
/// </summary>
public static class InputHub
{
    public static InputAction Move { get { return Find("Move"); } }

    public static InputAction Dash { get { return Find("Dash") ?? Find("Jump"); } }

    public static InputAction Sprint { get { return Find("Sprint"); } }

    public static InputAction Pause { get { return Find("Pause"); } }

    /// <summary>Vetor de movimento normalizado em 8 direcoes.</summary>
    public static Vector2 MoveValue
    {
        get
        {
            InputAction action = Move;
            Vector2 value = action != null ? action.ReadValue<Vector2>() : Vector2.zero;
            if (value.sqrMagnitude > 1f)
            {
                value.Normalize();
            }

            return value;
        }
    }

    public static bool DashPressed
    {
        get
        {
            InputAction action = Dash;
            if (action != null && action.WasPressedThisFrame())
            {
                return true;
            }

            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
        }
    }

    public static bool SprintHeld
    {
        get
        {
            InputAction action = Sprint;
            if (action != null && action.IsPressed())
            {
                return true;
            }

            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.leftShiftKey.isPressed;
        }
    }

    public static bool PausePressed
    {
        get
        {
            InputAction action = Pause;
            if (action != null && action.WasPressedThisFrame())
            {
                return true;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null &&
                (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.startButton.wasPressedThisFrame;
        }
    }

    public static bool MutePressed
    {
        get
        {
            Keyboard keyboard = Keyboard.current;
            return keyboard != null && keyboard.mKey.wasPressedThisFrame;
        }
    }

    /// <summary>True quando um gamepad/joystick esta conectado.</summary>
    public static bool GamepadConnected
    {
        get { return Gamepad.current != null || Joystick.current != null; }
    }

    private static InputAction Find(string name)
    {
        InputActionAsset asset = InputSystem.actions;
        if (asset == null)
        {
            return null;
        }

        InputAction action = asset.FindAction(name, false);
        if (action != null && !action.enabled)
        {
            action.Enable();
        }

        return action;
    }
}
