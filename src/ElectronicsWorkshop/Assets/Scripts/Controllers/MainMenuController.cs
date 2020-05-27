using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject _mainMenuObject;
        private bool _mainMenuOpen = false;

        private const int MAX_LINES = 25;
        private int _linesCountNow = 0;
        private void Start()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            GameEvents.current.Event_OnOpenMainMenu += OnOpenMainMenu;
            GameEvents.current.Event_OnCloseMainMenu += OnCloseMainMenu;
            GameEvents.current.Event_OnLogAction += LogAction;
        }

        private void LogAction(string text)
        {
            if (_linesCountNow == MAX_LINES)
            {
                _mainMenuObject.transform.Find("Text").GetComponent<Text>().text = string.Empty;
            }

            _mainMenuObject.transform.Find("Text").GetComponent<Text>().text += 
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " + text + Environment.NewLine;

            _linesCountNow++;
        }

        private void OnOpenMainMenu()
        {
            if (!_mainMenuOpen)
            {
                _mainMenuObject.SetActive(true);
                _mainMenuOpen = true;

                GameEvents.current.FireEvent_GoToGUIMode();
            }
        }

        private void OnCloseMainMenu()
        {
            if(_mainMenuOpen)
            {
                _mainMenuObject.SetActive(false);
                _mainMenuOpen = false;

                GameEvents.current.FireEvent_ReturnFromGUIMode();
            }
        }
    }
}
