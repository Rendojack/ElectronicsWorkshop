using Assets.Scripts.Canvas.Crosshair;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Canvas.Crosshair
{
    public enum CrosshairType
    {
        Default,
        ObjectRotate,
        ObjectPickup
    }

    public class CrosshairController : BaseController
    {
        [SerializeField] private Sprite _defaultCrosshairSprite;
        [SerializeField] private Sprite _objectPickupCrosshairSprite;
        [SerializeField] private Sprite _objectRotateCrosshairSprite;

        private Image _crosshairImage;

        private void Awake()
        {
            _crosshairImage = GetComponent<Image>();
            _crosshairImage.sprite = _defaultCrosshairSprite;
        }

        private void Start()
        {
            GameEvents.current.Event_OnPlayerCanPickObject += Event_OnPlayerCanPickObject;
            GameEvents.current.Event_OnPlayerCantPickObject += Event_OnPlayerCantPickObject;

            GameEvents.current.Event_OnPlayerDragObject += Event_OnPlayerDragObject;
            GameEvents.current.Event_OnPlayerDropbject += Event_OnPlayerDropObject;
        }
        
        private void Event_OnPlayerCanPickObject()
        {
            SetCrosshair(CrosshairType.ObjectPickup);
        }

        private void Event_OnPlayerCantPickObject()
        {
            SetDefaultCrosshair();
        }

        private void Event_OnPlayerDragObject()
        {
            SetCrosshair(CrosshairType.ObjectPickup);
        }

        private void Event_OnPlayerDropObject()
        {
            SetDefaultCrosshair();
        }

        private void SetCrosshair(CrosshairType type)
        {
            switch (type)
            {
                case CrosshairType.Default:
                    _crosshairImage.sprite = _defaultCrosshairSprite;
                    break;
                case CrosshairType.ObjectPickup:
                    _crosshairImage.sprite = _objectPickupCrosshairSprite;
                    break;
                case CrosshairType.ObjectRotate:
                    _crosshairImage.sprite = _objectRotateCrosshairSprite;
                    break;
            }
        }

        public void SetDefaultCrosshair()
        {
            _crosshairImage.sprite = _defaultCrosshairSprite;
        }
    }
}
