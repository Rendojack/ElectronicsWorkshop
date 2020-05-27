using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class PromptController : MonoBehaviour
{
    [SerializeField] private GameObject _prompt;
    [SerializeField] private Sprite _successIcon;
    [SerializeField] private Sprite _bellIcon;

    public enum PromptType
    {
        Success,
        Attention
    }

    private bool _isOpen = false;
    private void Start()
    {
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        GameEvents.current.Event_OnOpenPrompt += OnOpenPrompt;
        GameEvents.current.Event_OnClosePrompt += OnClosePrompt;
    }

    private void OnOpenPrompt(PromptType promptType, string text)
    {
        if (!_isOpen)
        {
            _prompt.gameObject.transform.Find("Text")
                .gameObject.GetComponent<Text>().text = Regex.Unescape(text);

            Sprite spriteToUse = null;
            switch (promptType)
            {
                case PromptType.Success:
                {
                    spriteToUse = _successIcon;
                    break;
                }

                case PromptType.Attention:
                {
                    GameEvents.current.FireEvent_PlaySound(SoundController.SoundType.Warning);
                    spriteToUse = _bellIcon;
                    break;
                }
            }

            if (spriteToUse == null)
            {
                return;
            }

            Image img = _prompt.gameObject.transform.Find("Icon").gameObject.GetComponent<Image>();
            img.sprite = spriteToUse;

            Vector2 size = new Vector2(100, 100); // Default size
            if (promptType == PromptType.Attention)
            {
                size = new Vector2(80, 80);
            }
            else if (promptType == PromptType.Success)
            {
                size = new Vector2(80, 100);
            }

            img.GetComponent<RectTransform>().sizeDelta = size;

            _prompt.SetActive(true);
            _isOpen = true;

            GameEvents.current.FireEvent_GoToGUIMode();
        }
    }

    private void OnClosePrompt()
    {
        if (_isOpen)
        {
            _prompt.SetActive(false);
            _isOpen = false;

            GameEvents.current.FireEvent_ReturnFromGUIMode();
        }
    }
}
