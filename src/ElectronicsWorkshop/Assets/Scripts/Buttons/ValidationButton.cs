using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Controllers;
using UnityEngine;

public class ValidationButton : MonoBehaviour
{
    private bool _locked = true;

    private MeshRenderer _meshRenderer;
    private bool _mouseOver;

    [SerializeField] private Shader _highlightShader;
    [SerializeField] private Shader _defaultShader;
    [SerializeField] private Color _outlineShaderColor;

    private string _buttonHint = "Užduoties validavimas";
    private bool _hintShowing = false;

    private void Start()
    {
        SubscribeEvents();
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        HighlightButton();
    }

    private void HighlightButton()
    {
        if (_locked)
        {
            return;
        }

        if (_mouseOver)
        {
            _meshRenderer.material.shader = _highlightShader;
            _meshRenderer.material.SetColor("_OutlineColor", _outlineShaderColor);

            GameEvents.current.FireEvent_SetHUDText(_buttonHint);
            _hintShowing = true;
        }
        else
        {
            _meshRenderer.material.shader = _defaultShader;

            if (_hintShowing)
            {
                GameEvents.current.FireEvent_RemoveHUDText();
                _hintShowing = false;
            }
        }
    }

    private void UnlockThis()
    {
        if (_locked)
        {
            _locked = false;
            GameEvents.current.FireEvent_HUDMessage("Atrakintas validavimo mygtukas!", HUDMessageType.Info);
        }
    }

    private void LockThis()
    {
        _locked = true;
    }

    private void SubscribeEvents()
    {
        GameEvents.current.Event_OnMouseLeftClick += OnMouseLeftClick;
        GameEvents.current.Event_OnMousePoint += OnMousePoint;
        GameEvents.current.Event_OnValidationButtonUnlock += UnlockThis;
        GameEvents.current.Event_OnValidationButtonLock += LockThis;
    }

    private void OnMousePoint(Transform point)
    {
        if (_locked)
        {
            return;
        }

        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        _mouseOver = baseComponent != null && baseComponent.HasTag(Tag.ValidationButton);
    }

    private void OnMouseLeftClick(Transform point)
    {
        if (_locked)
        {
            return;
        }

        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        if (baseComponent != null && baseComponent.HasTag(Tag.ValidationButton))
        {
            GameEvents.current.FireEvent_RequestChallengeValidation();
        }
    }
}
