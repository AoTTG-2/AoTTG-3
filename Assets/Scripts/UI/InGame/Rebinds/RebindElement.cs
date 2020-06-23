﻿using Assets.Scripts.UI.Input;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.InGame.Rebinds
{
    public class RebindElement : MonoBehaviour
    {
        public Text Label;
        public Button InputKey;

        public KeyCode Key { get; set; }

        private bool IsRebinding { get; set; }
        private const int MouseButtons = 7;

        private void Awake()
        {
            InputKey.onClick.AddListener(EnableRebinding);
        }

        private void EnableRebinding()
        {
            IsRebinding = true;
            InputKey.GetComponentInChildren<Text>().text = "PRESS ANY KEY!";
        }

        void OnGUI()
        {
            if (!IsRebinding) return;

            if (Event.current.isKey && Event.current.type == EventType.KeyDown)
            {
                SetInputKeycode(Event.current.keyCode);
                return;
            }

            // LeftShift is not recognized via events
            if (UnityEngine.Input.GetKey(KeyCode.LeftShift))
            {
                SetInputKeycode(KeyCode.LeftShift);
                return;
            }

            // RightShift is not recognized via events
            if (UnityEngine.Input.GetKey(KeyCode.RightShift))
            {
                SetInputKeycode(KeyCode.RightShift);
                return;
            }

            if (UnityEngine.Input.mouseScrollDelta.y > 0)
            {
                SetInputKeycode(InputManager.ScrollUp);
                return;
            }

            if (UnityEngine.Input.mouseScrollDelta.y < 0)
            {
                SetInputKeycode(InputManager.ScrollDown);
                return;
            }

            if (Event.current.isMouse)
            {
                for (var i = 0; i < MouseButtons; i++)
                {
                    if (!UnityEngine.Input.GetMouseButtonDown(i)) continue;
                    var mouseKeycode = KeyCode.Mouse0 + i;
                    SetInputKeycode(mouseKeycode);
                    break;
                }
            }
        }

        public void SetInputKeycode(KeyCode keyCode)
        {
            Key = keyCode;
            var keyString = keyCode.ToString();
            if (keyCode == InputManager.ScrollUp)
            {
                keyString = "Scroll Up";
            } else if (keyCode == InputManager.ScrollDown)
            {
                keyString = "Scroll Down";
            }

            InputKey.GetComponentInChildren<Text>().text = keyString;
            IsRebinding = false;
        }
    }
}
