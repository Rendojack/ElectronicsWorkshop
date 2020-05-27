using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Controllers;
using UnityEngine;

public class GamemodeButton : MonoBehaviour
{
    private bool _locked = true;
    public enum GameMode
    {
        EditMode,
        PlayMode
    }

    private MeshRenderer _meshRenderer;
    private bool _mouseOver;
    private GameMode _gameMode = GameMode.PlayMode;

    [SerializeField] private Material _editModeMaterial;
    [SerializeField] private Material _playModeMaterial;
    [SerializeField] private Shader _highlightShader;
    [SerializeField] private Shader _defaultShader;
    [SerializeField] private Color _outlineShaderColor;

    private string _buttonHint = "Pakeisti žaidimo rėžimą";
    private bool _hintShowing = false;

    private void Start()
    {
        SubscribeEvents();
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        _meshRenderer.material = _editModeMaterial;
    }

    private void Update()
    {
        HighlightButton();
    }

    private void UnlockThis()
    {
        if (_locked)
        {
            _locked = false;
            GameEvents.current.FireEvent_HUDMessage("Atrakintas žaidimo rėžimo mygtukas!", HUDMessageType.Info);
        }
    }

    private void LockThis()
    {
        _locked = true;
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

    private void SubscribeEvents()
    {
        GameEvents.current.Event_OnMouseLeftClick += OnMouseLeftClick;
        GameEvents.current.Event_OnMousePoint += OnMousePoint;
        GameEvents.current.Event_OnGamemodeButtonUnlock += UnlockThis;
        GameEvents.current.Event_OnGamemodeButtonLock += LockThis;
    }

    private void OnMouseLeftClick(Transform point)
    {
        if (_locked)
        {
            return;
        }

        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        if (baseComponent != null && baseComponent.HasTag(Tag.GameModeButton))
        {
            SwitchGameMode();
        }
    }

    private void SwitchGameMode()
    {
        switch (_gameMode)
        {
            case GameMode.EditMode:
            {
                _gameMode = GameMode.PlayMode;
                _meshRenderer.material = _playModeMaterial;
                break;
            }

            case GameMode.PlayMode:
            {
                _gameMode = GameMode.EditMode;
                _meshRenderer.material = _editModeMaterial;
                break;
            }
        }

        GameEvents.current.FireEvent_GameModeSwitch(_gameMode);
    }

    private void OnMousePoint(Transform point)
    {
        if (_locked)
        {
            return;
        }

        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        _mouseOver = baseComponent != null && baseComponent.HasTag(Tag.GameModeButton);
    }
}
