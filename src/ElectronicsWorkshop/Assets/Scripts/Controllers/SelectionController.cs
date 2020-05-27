using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Events.Controllers
{
    public class SelectionController : BaseController
    {
        [SerializeField] private Tag _selectableTag = Tag.Selectable;
        [SerializeField] private Shader _selectionShader;
        [SerializeField] private Color _selectionShaderColor;

        private const string _shaderOutlineColorPropertyName = "_OutlineColor";

        private Transform _lastTargetObject;
        private Renderer _lastTargetObjectRenderer;
        private Shader _lastTargetObjectDefaultShader;

        private void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit))
            {
                Transform currentTargetObject = hit.transform;
                BaseComponent baseComponent = currentTargetObject.GetComponent<BaseComponent>();
          
                bool isSelectableObject = baseComponent == null ? false : baseComponent.HasTag(_selectableTag);
                if (isSelectableObject && _lastTargetObject == null)
                {
                    _lastTargetObject = currentTargetObject;
                    _lastTargetObjectRenderer = _lastTargetObject.GetComponent<Renderer>();

                    _lastTargetObjectDefaultShader = _lastTargetObjectRenderer.material.shader;
                    _lastTargetObjectRenderer.material.shader = _selectionShader;
                    _lastTargetObjectRenderer.material.SetColor(_shaderOutlineColorPropertyName, _selectionShaderColor);
                }

                if(_lastTargetObject != null && _lastTargetObject != currentTargetObject)
                {
                    ClearLastTargetObject();
                }
            }
        }

        private void ClearLastTargetObject()
        {
            if (_lastTargetObjectRenderer != null)
            {
                _lastTargetObjectRenderer.material.shader = _lastTargetObjectDefaultShader;
            }

            _lastTargetObjectRenderer = null;
            _lastTargetObjectDefaultShader = null;
            _lastTargetObject = null;
        }
    }
}
