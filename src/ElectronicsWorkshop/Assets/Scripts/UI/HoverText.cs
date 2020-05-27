using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class HoverText : MonoBehaviour
{
    [SerializeField] private Tag _objectTag = Tag.Untagged;
    [SerializeField] private string _hoverText = string.Empty;

    private bool _textShowing = false;
    private bool _mouseOver = false;

    private void Start()
    {
        GameEvents.current.Event_OnMousePoint += OnMousePoint;
    }

    public void SetText(string text)
    {
        _hoverText = text;
    }

    private void OnMousePoint(Transform point)
    {
        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        if (baseComponent == null)
        {
            baseComponent = point.parent?.GetComponent<BaseComponent>();
        }

        _mouseOver = baseComponent != null && baseComponent.HasTag(_objectTag);
    }

    private void Update()
    {
        if (_mouseOver)
        {
            GameEvents.current.FireEvent_SetHUDText(_hoverText);
            _textShowing = true;
        }
        else
        {
            if (_textShowing)
            {
                GameEvents.current.FireEvent_RemoveHUDText();
                _textShowing = false;
            }
        }
    }
}
