using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts.Controllers;
using UnityEngine;
using ThreadPriority = UnityEngine.ThreadPriority;

namespace Assets.Scripts
{
    /// <summary>
    /// Special admin hot-keys to instantly enable certain game functions
    /// </summary>
    public class AdminHack : MonoBehaviour
    {
        private bool _buttonsLocked = true;
        private const int INVOKE_DELAY_MILLIS = 250;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) // The '1' key on the top of the alphanumeric keyboard
            {
                GameEvents.current.FireEvent_HUDMessage("Panaudotas slaptas mygtukų atrakinimas!", HUDMessageType.Info);

                if (_buttonsLocked)
                {
                    GameEvents.current.FireEvent_UnlockChapterButton();
                    GameEvents.current.FireEvent_UnlockAllChapters();
                    GameEvents.current.FireEvent_UnlockGamemodeButton();
                    GameEvents.current.FireEvent_UnlockValidationButton();

                    _buttonsLocked = false;
                }
            }

            if (Input.GetKeyDown(KeyCode.Alpha2)) // The '2' key on the top of the alphanumeric keyboard
            {
                GameEvents.current.FireEvent_HUDMessage("Panaudotas slaptas skaidrės atrakinimas!", HUDMessageType.Info);
                GameEvents.current.FireEvent_UnlockSlide();
            }
        }
    }
}
