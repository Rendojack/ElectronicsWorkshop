using Nito.Collections;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Controllers
{
    public class HUDController : BaseController
    {
        [SerializeField] private GameObject Canvas;
        [SerializeField] private GameObject HUDMessagePrefab;

        [SerializeField] private Sprite HUDMessageInfoIcon;
        [SerializeField] private Sprite HUDMessageWarningIcon;
        [SerializeField] private Sprite HUDMessageErrorIcon;

        [SerializeField] private GameObject HUDEditMode;
        [SerializeField] private GameObject HUDPlayMode;

        [SerializeField] private GameObject HUDEntityPanel;
        [SerializeField] private GameObject HUDEntityDesc;

        [SerializeField] private GameObject _HUDText;
        [SerializeField] private GameObject _HUDCenteredMenu;
        [SerializeField] private GameObject _HUDPrompt;

        private const float HUD_MESSAGE_SHIFT_Y = 56f; // For each new HUD_MESSAGE, shift y pos

        private const float HUD_MESSAGE_INIT_X = 206f; // First HUD_MESSAGE x pos
        private const float HUD_MESSAGE_INIT_Y = 33f; // First HUD_MESSAGE y pos

        private const int HUD_MESSAGE_MAX_COUNT = 16;
        private const int HUD_MESSAGE_DESTROY_AFTER_MILLIS = 10_000;

        private static object _HUDMessageLock = new object();
        private readonly Deque<HUDMessage> _HUDMessages = new Deque<HUDMessage>();

        private int _lastHUDMessageIndex = 0;

        private ConcurrentQueue<Action> _pendingHUDMessages = new ConcurrentQueue<Action>();

        public void Start()
        {
            GameEvents.current.Event_OnHUDMessage += OnHUDMessage;
            GameEvents.current.Event_OnGameModeSwitch += OnGameModeSwitch;
            GameEvents.current.Event_OnSetHUDText += OnSetHUDText;
            GameEvents.current.Event_OnRemoveHUDText += OnRemoveHUDText;

            GameEvents.current.Event_OnOpenHUDCenteredMenu += OnOpenHUDCenteredMenu;
            GameEvents.current.Event_OnCloseHUDCenteredMenu += OnCloseHUDCenteredMenu;

            OnGameModeSwitch(GamemodeButton.GameMode.PlayMode);
        }

        public void OnOpenHUDCenteredMenu()
        {
            _HUDCenteredMenu.SetActive(true);
            GameEvents.current.FireEvent_GoToGUIMode();
        }

        public void OnCloseHUDCenteredMenu()
        {
            _HUDCenteredMenu.SetActive(false);
            GameEvents.current.FireEvent_ReturnFromGUIMode();
            GameEvents.current.FireEvent_HUDCenteredMenuClosed(GetFieldValues());
        }

        private List<string> GetFieldValues()
        {
            List<string> values = new List<string>();
            foreach (Transform child in _HUDCenteredMenu.transform)
            {
                if (child.tag == "Field")
                {
                    foreach (Transform child2 in child)
                    {
                        if (child2.tag == "FieldText")
                        {
                            values.Add(child2.gameObject.GetComponent<Text>().text);
                        }
                    }
                }
            }

            return values;
        }

        public void OnSetHUDText(string text)
        {
            _HUDText.gameObject.GetComponent<Text>().text = text;
            _HUDText.gameObject.SetActive(true);
        }

        public void OnRemoveHUDText()
        {
            _HUDText.gameObject.GetComponent<Text>().text = string.Empty;
            _HUDText.gameObject.SetActive(false);
        }

        public void OnGameModeSwitch(GamemodeButton.GameMode gameMode)
        {
            switch (gameMode)
            {
                case GamemodeButton.GameMode.PlayMode:
                {
                    HUDEditMode.SetActive(false);
                    HUDPlayMode.SetActive(true);

                    HUDEntityPanel.SetActive(false);
                    HUDEntityDesc.SetActive(false);
                    break;
                }
                case GamemodeButton.GameMode.EditMode:
                {
                    HUDEditMode.SetActive(true);
                    HUDPlayMode.SetActive(false);

                    HUDEntityPanel.SetActive(true);
                    HUDEntityDesc.SetActive(true);
                    break;
                }
            }
        }

        public void OnHUDMessage(string message, HUDMessageType type)
        {
            Sprite icon;
            switch (type)
            {
                case HUDMessageType.Error:
                {
                    GameEvents.current.FireEvent_PlaySound(SoundController.SoundType.Error);
                    icon = HUDMessageErrorIcon;
                    break;
                }
                case HUDMessageType.Warning:
                {
                    GameEvents.current.FireEvent_PlaySound(SoundController.SoundType.Warning);
                    icon = HUDMessageWarningIcon;
                    break;
                }
                case HUDMessageType.Info:
                default:
                {
                    GameEvents.current.FireEvent_PlaySound(SoundController.SoundType.Info);
                    icon = HUDMessageInfoIcon;
                    break;
                }
            }

            // Enqueue message for processing
            _pendingHUDMessages.Enqueue(() =>
            {
                CreateHUDMessage(message, icon);
            });
        }

        public void CreateHUDMessage(string message, Sprite icon)
        {
            lock (_HUDMessageLock)
            {
                if (_HUDMessages.Count == HUD_MESSAGE_MAX_COUNT)
                {
                    Destroy(_HUDMessages.RemoveFromFront().Self);


                    _lastHUDMessageIndex = HUD_MESSAGE_MAX_COUNT - 1;
                }
            }

            lock (_HUDMessageLock)
            {
                if (_HUDMessages.Any())
                {
                    GameObject existingMsg = _HUDMessages.First().Self;
                    if (existingMsg.GetComponentInChildren<Text>().text == message)
                    {
                        // Found identical message already existing. Do not duplicate, just identify that upcoming msg is the same
                        existingMsg.GetComponent<Shake>().ShakeIt();
                        return;
                    }
                }
            }

            GameObject HUDMessage = Instantiate(HUDMessagePrefab, new Vector3(0, 0, 0), Quaternion.identity,
                Canvas.transform);
            RectTransform rTransform = HUDMessage.GetComponent<RectTransform>();

            HUDMessage.GetComponentInChildren<Text>().text = message;
            rTransform.position = new Vector3(HUD_MESSAGE_INIT_X, HUD_MESSAGE_INIT_Y, rTransform.position.z);
            rTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom,
                HUD_MESSAGE_INIT_Y + (_lastHUDMessageIndex * HUD_MESSAGE_SHIFT_Y), 0);

            Transform HUDMessageType = HUDMessage.transform.Find("HUDMessageType");
            HUDMessageType.gameObject.GetComponent<Image>().sprite = icon;

            lock (_HUDMessageLock)
            {
                _HUDMessages.AddToFront(new HUDMessage() { Self = HUDMessage, TimeCreation = DateTime.Now });
            }

            if (_lastHUDMessageIndex + 1 < HUD_MESSAGE_MAX_COUNT)
            {
                _lastHUDMessageIndex++;
            }
        }

        public void Update()
        {
            // Reset next HUD message index if deque is empty
            lock (_HUDMessageLock)
            {
                if (!_HUDMessages.Any())
                {
                    _lastHUDMessageIndex = 0;
                }
            }

            // Try to build new HUD message if exist
            if (_pendingHUDMessages.TryDequeue(out Action Method))
            {
                Method.Invoke();
            }

            // Destroy old HUD messages
            DestroyHangingHUDMessages();
        }

        public void DestroyHangingHUDMessages()
        {
            HUDMessage HUDMessageToDestroy = null;
            lock (_HUDMessageLock)
            {
                foreach (HUDMessage HUDMessage in _HUDMessages)
                {
                    DateTime destroyTime = HUDMessage.TimeCreation.AddMilliseconds(HUD_MESSAGE_DESTROY_AFTER_MILLIS);
                    if (DateTime.Now >= destroyTime)
                    {
                        HUDMessageToDestroy = HUDMessage;
                        break;
                    }
                }

                if (HUDMessageToDestroy != null)
                {
                    _HUDMessages.RemoveFromBack();
                    DestroyImmediate(HUDMessageToDestroy.Self);
                }
            }

            // Destroy garbage
            foreach (GameObject obj in GameObject.FindGameObjectsWithTag("HUDMessage"))
            {
                if (obj.GetComponent<BaseComponent>().LiveTimer.ElapsedMilliseconds >=
                    HUD_MESSAGE_DESTROY_AFTER_MILLIS)
                {
                    DestroyImmediate(obj);
                }
            }
        }
    }
}
