using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class LED : MonoBehaviour
    {
        private bool _textShowing = false;
        private bool _mouseOver = false;

        public bool Lit = false;
        [SerializeField] private Material _offMaterial;
        [SerializeField] private Material _onMaterial;

        private bool _blinkingEnabled = false;
        private bool _isBlinking = false;
        private const int BLINK_FREQ_MILLIS = 500;

        public string AnodeCoord { get; set; } // Coordinate
        public string CathodeCoord { get; set; } // Coordinate

        private void Start()
        {
            gameObject.name = GetThisName();
            GameEvents.current.Event_OnMousePoint += OnMousePoint;

            //TurnOn();
        }

        public void TurnOn()
        {
            if (!Lit)
            {
                Transform bulbObj = transform.GetChild(0);
                bulbObj.GetComponent<MeshRenderer>().material = _onMaterial;

                Transform lightObj = transform.GetChild(1);
                lightObj.gameObject.SetActive(true);

                Lit = true;
            }
        }

        public void TurnOff()
        {
            if (Lit)
            {
                Transform bulbObj = transform.GetChild(0);
                bulbObj.GetComponent<MeshRenderer>().material = _offMaterial;

                Transform lightObj = transform.GetChild(1);
                lightObj.gameObject.SetActive(false);

                Lit = false;
            }
        }

        private void OnMousePoint(Transform point)
        {
            BaseComponent baseComponent = point.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                baseComponent = point.parent?.GetComponent<BaseComponent>();
            }

            _mouseOver = baseComponent != null && baseComponent.HasTag(Tag.LED) &&
                         point.gameObject.name == GetThisName();
        }

        private string GetThisName()
        {
            // We need this as we use prefab for cloning.
            // Without ID, we end up on event listening collision,
            // because Tag.LED indication is not unique
            return $"LED_{GetInstanceID()}";
        }

        private void Update()
        {
            // Hover info text
            if (_mouseOver)
            {
                GameEvents.current.FireEvent_SetHUDText($"Šviesos diodas\nRaudonas 1.8V/20mA");
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

            // LED blinking
            if (_blinkingEnabled && !_isBlinking)
            {
                StartCoroutine(BlinkRoutine(BLINK_FREQ_MILLIS));
            }
        }

        private IEnumerator BlinkRoutine(int frequencyMillis)
        {
            _isBlinking = true;

            float blinkSeconds = (float) frequencyMillis / 1000;
            TurnOff();
            yield return new WaitForSeconds(blinkSeconds);
            TurnOn();
            yield return new WaitForSeconds(blinkSeconds);

            _isBlinking = false;
        }
    }
}
