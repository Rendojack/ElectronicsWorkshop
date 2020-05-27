using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public enum MouseWheelDirection
    {
        Forward,
        Backward
    }

    public class InputController : BaseController
    {
        private bool _thisEnabled = true;
        private Dictionary<KeyCode, Action> _keys = new Dictionary<KeyCode, Action>();

        private void Start()
        {
            BindKeys();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            GameEvents.current.Event_OnGoToGUIMode += GoToGUIMode;
            GameEvents.current.Event_OnReturnFromGUIMode += ReturnFromGUIMode;
        }

        private void GoToGUIMode()
        {
            _thisEnabled = false;
        }

        private void ReturnFromGUIMode()
        {
            _thisEnabled = true;
        }

        private void BindKeys()
        {    
            _keys.Add(KeyCode.Space, GameEvents.current.FireEvent_PlayerJump);
            _keys.Add(KeyCode.Mouse0, GameEvents.current.FireEvent_PlayerTryDragObject);
            _keys.Add(KeyCode.Mouse1, GameEvents.current.FireEvent_PlayerThrowObject);
            _keys.Add(KeyCode.Q, GameEvents.current.FireEvent_OpenMainMenu);
            _keys.Add(KeyCode.Tab, () =>
            {
                GameEvents.current.FireEvent_MouseWheelScroll(10.00f, MouseWheelDirection.Forward);
            });
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (!_thisEnabled)
            {
                return;
            }

            // Binded keyboard/mouse keys
            foreach (KeyValuePair<KeyCode, Action> kvp in _keys)
            {
                KeyCode keyDown = kvp.Key;
                if (Input.GetKeyDown(keyDown))
                {
                    GameEvents.current.FireEvent_PlaySound(SoundController.SoundType.Click);
                    Action action = kvp.Value;
                    action?.Invoke();
                }
            }

            // Ctrl + z (undo)
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    GameEvents.current.FireEvent_DestroyLastBreadboardGameobject();
                }
            }

            // Mouse wheel
            float distance;
            if ((distance = Input.GetAxis("Mouse ScrollWheel")) != 0f)
            {
                GameEvents.current.FireEvent_PlaySound(SoundController.SoundType.Scroll);
                GameEvents.current.FireEvent_MouseWheelScroll(distance,
                    distance > 0f ? MouseWheelDirection.Forward : MouseWheelDirection.Backward);
            }
        }
    }
}
