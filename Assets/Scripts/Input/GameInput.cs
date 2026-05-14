using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace SunnysideIsland.Input
{
    public static class GameInput
    {
        public static bool AnyKeyDown()
        {
            return Keyboard.current?.anyKey.wasPressedThisFrame == true || IsAnyMouseButtonPressedThisFrame();
        }

        public static bool GetKey(KeyCode keyCode)
        {
            return TryGetKey(keyCode, out var key) && key.isPressed;
        }

        public static bool GetKeyDown(KeyCode keyCode)
        {
            return TryGetKey(keyCode, out var key) && key.wasPressedThisFrame;
        }

        public static bool GetMouseButtonDown(int button)
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return false;
            }

            return button switch
            {
                0 => mouse.leftButton.wasPressedThisFrame,
                1 => mouse.rightButton.wasPressedThisFrame,
                2 => mouse.middleButton.wasPressedThisFrame,
                _ => false
            };
        }

        public static float GetAxisRaw(string axisName)
        {
            if (string.Equals(axisName, "Horizontal", StringComparison.OrdinalIgnoreCase))
            {
                return GetHorizontalAxis();
            }

            if (string.Equals(axisName, "Vertical", StringComparison.OrdinalIgnoreCase))
            {
                return GetVerticalAxis();
            }

            return 0f;
        }

        public static bool GetButtonDown(string buttonName)
        {
            if (Enum.TryParse(buttonName, true, out KeyCode keyCode))
            {
                return GetKeyDown(keyCode);
            }

            return buttonName switch
            {
                "Submit" => GetKeyDown(KeyCode.Return) || GetKeyDown(KeyCode.Space),
                "Cancel" => GetKeyDown(KeyCode.Escape),
                "Jump" => GetKeyDown(KeyCode.Space),
                "Fire1" => GetMouseButtonDown(0),
                _ => false
            };
        }

        private static float GetHorizontalAxis()
        {
            float value = 0f;

            if (GetKey(KeyCode.A) || GetKey(KeyCode.LeftArrow))
            {
                value -= 1f;
            }

            if (GetKey(KeyCode.D) || GetKey(KeyCode.RightArrow))
            {
                value += 1f;
            }

            var gamepadValue = Gamepad.current?.leftStick.x.ReadValue() ?? 0f;
            return Mathf.Abs(gamepadValue) > Mathf.Abs(value) ? Mathf.Sign(gamepadValue) : Mathf.Clamp(value, -1f, 1f);
        }

        private static float GetVerticalAxis()
        {
            float value = 0f;

            if (GetKey(KeyCode.S) || GetKey(KeyCode.DownArrow))
            {
                value -= 1f;
            }

            if (GetKey(KeyCode.W) || GetKey(KeyCode.UpArrow))
            {
                value += 1f;
            }

            var gamepadValue = Gamepad.current?.leftStick.y.ReadValue() ?? 0f;
            return Mathf.Abs(gamepadValue) > Mathf.Abs(value) ? Mathf.Sign(gamepadValue) : Mathf.Clamp(value, -1f, 1f);
        }

        private static bool IsAnyMouseButtonPressedThisFrame()
        {
            var mouse = Mouse.current;
            return mouse != null
                   && (mouse.leftButton.wasPressedThisFrame
                       || mouse.rightButton.wasPressedThisFrame
                       || mouse.middleButton.wasPressedThisFrame);
        }

        private static bool TryGetKey(KeyCode keyCode, out KeyControl key)
        {
            var keyboard = Keyboard.current;
            key = null;

            if (keyboard == null)
            {
                return false;
            }

            key = keyCode switch
            {
                KeyCode.A => keyboard.aKey,
                KeyCode.B => keyboard.bKey,
                KeyCode.C => keyboard.cKey,
                KeyCode.D => keyboard.dKey,
                KeyCode.E => keyboard.eKey,
                KeyCode.F => keyboard.fKey,
                KeyCode.G => keyboard.gKey,
                KeyCode.H => keyboard.hKey,
                KeyCode.I => keyboard.iKey,
                KeyCode.J => keyboard.jKey,
                KeyCode.K => keyboard.kKey,
                KeyCode.L => keyboard.lKey,
                KeyCode.M => keyboard.mKey,
                KeyCode.N => keyboard.nKey,
                KeyCode.O => keyboard.oKey,
                KeyCode.P => keyboard.pKey,
                KeyCode.Q => keyboard.qKey,
                KeyCode.R => keyboard.rKey,
                KeyCode.S => keyboard.sKey,
                KeyCode.T => keyboard.tKey,
                KeyCode.U => keyboard.uKey,
                KeyCode.V => keyboard.vKey,
                KeyCode.W => keyboard.wKey,
                KeyCode.X => keyboard.xKey,
                KeyCode.Y => keyboard.yKey,
                KeyCode.Z => keyboard.zKey,
                KeyCode.Alpha0 => keyboard.digit0Key,
                KeyCode.Alpha1 => keyboard.digit1Key,
                KeyCode.Alpha2 => keyboard.digit2Key,
                KeyCode.Alpha3 => keyboard.digit3Key,
                KeyCode.Alpha4 => keyboard.digit4Key,
                KeyCode.Alpha5 => keyboard.digit5Key,
                KeyCode.Alpha6 => keyboard.digit6Key,
                KeyCode.Alpha7 => keyboard.digit7Key,
                KeyCode.Alpha8 => keyboard.digit8Key,
                KeyCode.Alpha9 => keyboard.digit9Key,
                KeyCode.BackQuote => keyboard.backquoteKey,
                KeyCode.DownArrow => keyboard.downArrowKey,
                KeyCode.Escape => keyboard.escapeKey,
                KeyCode.LeftArrow => keyboard.leftArrowKey,
                KeyCode.LeftShift => keyboard.leftShiftKey,
                KeyCode.Return => keyboard.enterKey,
                KeyCode.RightArrow => keyboard.rightArrowKey,
                KeyCode.RightShift => keyboard.rightShiftKey,
                KeyCode.Space => keyboard.spaceKey,
                KeyCode.UpArrow => keyboard.upArrowKey,
                _ => null
            };

            return key != null;
        }
    }
}
