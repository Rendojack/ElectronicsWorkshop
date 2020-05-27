using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class HoverHighlight : MonoBehaviour
{
    [SerializeField] private Shader _highlightShader;
    [SerializeField] private Shader _defaultShader;
    [SerializeField] private Color _outlineShaderColor;
    [SerializeField] private Tag _objectTag = Tag.Untagged;

    private bool _mouseOver = false;
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        GameEvents.current.Event_OnMousePoint += OnMousePoint;
    }

    private void Update()
    {
        HighlightObject();
    }

    private void HighlightObject()
    {
        if (_mouseOver)
        {
            _meshRenderer.material.shader = _highlightShader;
            _meshRenderer.material.SetColor("_OutlineColor", _outlineShaderColor);
        }
        else
        {
            _meshRenderer.material.shader = _defaultShader;
        }
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
}
