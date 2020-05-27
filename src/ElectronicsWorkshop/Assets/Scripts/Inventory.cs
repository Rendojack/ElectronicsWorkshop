using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Assets.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Feature
{
    public class Inventory : MonoBehaviour, IOrderedInit
    {
        public class InventoryItem
        {
            public string Name { get; set; }
            public GameObject Object { get; set; }
            public Sprite Sprite { get; set; }
            public string Desc { get; set; }
        }

        private bool _thisEnabled = false;

        [SerializeField] private GameObject _HUDEntityPanel;
        [SerializeField] private GameObject _HUDEntityDesc;

        private Dictionary<string, InventoryItem> _items = new Dictionary<string, InventoryItem>();
        private int _activeItemIndex;
        private InventoryItem _activeItem;

        private RectTransform _HUDScrollbar_RTrans; // Scrollbar background (constant)
        private RectTransform _HUDScrollbarFill_RTrans; // Scrollbar foreground (fill) 
        private float _HUDScrollbarStep; // Scroll step

        private void SubscribeEvents()
        {
            GameEvents.current.Event_OnMouseWheelScroll += OnMouseWheelScroll;
            GameEvents.current.Event_OnInventoryItemAdd += OnItemAdd;
            GameEvents.current.Event_OnGameModeSwitch += OnGameModeSwitch;
            GameEvents.current.Event_OnUpdateActiveInventoryItemDesc += OnUpdateActiveItemDescription;
        }

        private void OnGameModeSwitch(GamemodeButton.GameMode gameMode)
        {
            switch (gameMode)
            {
                case GamemodeButton.GameMode.EditMode:
                {
                    _thisEnabled = true;
                    break;
                }
                default:
                {
                    _thisEnabled = false;
                    break;
                }
            }
        }

        private void OnItemAdd(Sprite icon, GameObject item, string description)
        {
            _items.Add(item.name, new InventoryItem
            {
                Name = item.name,
                Object = item,
                Sprite = icon,
                Desc = Regex.Unescape(description)
            });

            UpdateInventory();
        }

        private void UpdateInventory()
        {
            if (_items.Any())
            {
                bool initialUpdate = _activeItem == null;
                if(initialUpdate)
                {
                    _activeItemIndex = 0;
                    _activeItem = _items.Values.ToList()[_activeItemIndex];
                }

                SetupHUDEntityPanel();
                UpdateActiveItemImage();
                UpdateActiveItemDesc();
            }
        }

        private void SetupHUDEntityPanel()
        {
            if (_HUDEntityPanel != null)
            {
                Transform HUDScrollbar = _HUDEntityPanel.transform.Find("HUDScrollbar");
                _HUDScrollbar_RTrans = HUDScrollbar.GetComponent<RectTransform>();

                _HUDScrollbarFill_RTrans = HUDScrollbar.Find("HUDScrollbarFill").GetComponent<RectTransform>();

                _HUDScrollbarStep = _HUDScrollbar_RTrans.sizeDelta.x / _items.Count;

                _HUDScrollbarFill_RTrans.sizeDelta = new Vector2(_activeItemIndex + 1 * _HUDScrollbarStep, _HUDScrollbar_RTrans.sizeDelta.y);
                UpdateScrollbarPosition();
            }
        }

        private void UpdateScrollbarPosition()
        {
            if (_HUDScrollbarFill_RTrans != null)
            {
                _HUDScrollbarFill_RTrans.anchoredPosition = new Vector3(_activeItemIndex * _HUDScrollbarStep, 0, 0);
            }
        }

        private void UpdateActiveItemImage()
        {
            if (_HUDEntityPanel != null && _activeItem != null)
            {
                _HUDEntityPanel.GetComponent<Image>().sprite = _activeItem.Sprite;
            }
        }

        private void UpdateActiveItemDesc()
        {
            if (_HUDEntityDesc != null && _activeItem != null)
            {
                _HUDEntityDesc.GetComponentInChildren<Text>().text = _activeItem.Desc;
            }
        }

        private void OnMouseWheelScroll(float distance, MouseWheelDirection direction)
        {
            if(!_thisEnabled) { return; }

            switch (direction)
            {
                case MouseWheelDirection.Forward:
                    {
                        if (_activeItemIndex < _items.Count - 1)
                        {
                            _activeItemIndex++;
                        }
                        else
                        {
                            _activeItemIndex = 0;
                        }

                        break;
                    }
                case MouseWheelDirection.Backward:
                    {
                        if (_activeItemIndex > 0)
                        {
                            _activeItemIndex--;
                        }
                        else
                        {
                            _activeItemIndex = _items.Count - 1;
                        }

                        break;
                    }
            }

            _activeItem = _items.Values.ToList()[_activeItemIndex];
            UpdateActiveItemImage();
            UpdateActiveItemDesc();
            UpdateScrollbarPosition();

            GameEvents.current.FireEvent_InventoryActiveItemChanged(_activeItem);
        }

        public void OnUpdateActiveItemDescription(string description)
        {
            _activeItem.Desc = description;
            UpdateActiveItemDesc();
        }

        public void InitOrdered()
        {
            SubscribeEvents();
        }

        public int GetInitOrder()
        {
            return 1;
        }
    }
}
