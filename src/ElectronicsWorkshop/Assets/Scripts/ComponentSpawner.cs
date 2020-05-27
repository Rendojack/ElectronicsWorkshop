using Assets.Scripts.Core.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Feature
{
    public enum ComponentType
    {
        Unknown,
        LED,
        Resistor
    }

    public class ComponentSpawner: MonoBehaviour, IOrderedInit
    {
        private bool _thisEnabled = false;

        [SerializeField] private GameObject _breadboard;
        [SerializeField] private List<GameObject> _spawnablePrefabs = new List<GameObject>();
        [SerializeField] private List<Sprite> _spawnablePrefabSprites = new List<Sprite>();
        [SerializeField] private List<string> _spawnablePrefabDescs = new List<string>();

        private GameObject _activeSpawnablePrefab;
        private const int _activeSpawnablePrefabHolesNeeded = 2; // For now we are only working with 2 terminal components

        private FixedSizeQueue<Transform> _holes = new FixedSizeQueue<Transform>(2);

        private bool _cuttingToolActive = false;
        private bool _rightClickMenuOpen = false;

        private int _resistorOhmConfigured = Resistor.DEFAULT_RESISTANCE_OHM;
        private Inventory.InventoryItem _activeInventoryItem;

        private void Start()
        {
            SubscribeEvents();
        }

        private void PopulateInventory()
        {
            for (int i = 0; i < _spawnablePrefabs.Count; i++)
            {
                GameEvents.current.FireEvent_InventoryItemAdd(
                    _spawnablePrefabSprites[i],
                    _spawnablePrefabs[i], 
                    _spawnablePrefabDescs[i]);
            }
        }

        private void SubscribeEvents()
        {
            GameEvents.current.Event_OnMouseLeftClick += OnMouseLeftClick;
            GameEvents.current.Event_OnMouseRightClick += OnMouseRightClick;

            GameEvents.current.Event_OnInventoryActiveItemChanged += OnInventoryActiveItemChanged;
            GameEvents.current.Event_OnGameModeSwitch += OnGameModeSwitch;

            GameEvents.current.Event_OnCloseHUDCenteredMenu += OnHUDCenteredMenuClose;
            GameEvents.current.Event_OnHUDCenteredMenuClosed += OnHUDCenteredMenuClosed;
        }

        private void OnHUDCenteredMenuClosed(List<string> values)
        {
            BaseComponent baseComponent = _activeInventoryItem.Object.GetComponent<BaseComponent>();
            if (baseComponent.HasTag(Tag.Resistor))
            {
                if (!int.TryParse(values.First(), out _resistorOhmConfigured))
                {
                    return; // Invalid OHM entered
                }

                // Update old resistance value
                int resistanceDescIndex = _activeInventoryItem.Desc.IndexOf("Varža");
                string updatedDesc = _activeInventoryItem.Desc.Substring(0, resistanceDescIndex);
                updatedDesc += $"Varža {_resistorOhmConfigured} Ω";

                // Fire event to update description on GUI
                GameEvents.current.FireEvent_UpdateActiveInventoryItemDesc(updatedDesc);
            }
        }

        private void OnHUDCenteredMenuClose()
        {
            _rightClickMenuOpen = false;
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

        private void OnInventoryActiveItemChanged(Inventory.InventoryItem item)
        {
            _activeInventoryItem = item;

            if (!_thisEnabled) { return; }
            BaseComponent baseComponent = item.Object.GetComponent<BaseComponent>();
            if (baseComponent.HasTag(Tag.Spawnable))
            {
                _activeSpawnablePrefab = item.Object;
            }
            else
            {
                _activeSpawnablePrefab = null;
            }

            if(baseComponent.HasTag(Tag.CuttingTool))
            {
                _cuttingToolActive = true;
            }
            else
            {
                _cuttingToolActive = false;
            }

            if (baseComponent.HasTag(Tag.Resistor))
            {
                string updatedDesc = item.Desc.Replace(Resistor.RESISTANCE_VAR_TPL, _resistorOhmConfigured.ToString());
                GameEvents.current.FireEvent_UpdateActiveInventoryItemDesc(updatedDesc);
            }
        }

        private void OnMouseRightClick(Transform point)
        {
            if (!_thisEnabled || _activeSpawnablePrefab == null)
            {
                return;
            }

            BaseComponent baseComponent = _activeSpawnablePrefab.GetComponent<BaseComponent>();

            if (baseComponent != null && baseComponent.HasTag(Tag.HasOptionMenu))
            {
                if(!_rightClickMenuOpen)
                {
                    GameEvents.current.FireEvent_OpenHUDCenteredMenu();
                    _rightClickMenuOpen = true;
                }
            }
        }

        private void OnMouseLeftClick(Transform point)
        {
            if (!_thisEnabled) { return; }
            BaseComponent baseComponent = point.GetComponent<BaseComponent>();

            if (baseComponent == null)
            {
                return;
            }

            if (_cuttingToolActive && baseComponent.HasTag(Tag.Spawnable))
            {
                GameEvents.current.FireEvent_ComponentDestroyBreadboard(point.gameObject);
                DestroyImmediate(point.gameObject);
                GameEvents.current.FireEvent_RemoveHUDText();
                //GameEvents.current.FireEvent_HUDMessage("Component destroyed!", Controllers.HUDMessageType.Info);
                return;
            }

            if(_activeSpawnablePrefab == null)
            {
                return; // Nothing to spawn
            }

            if (baseComponent != null && baseComponent.HasTag(Tag.Hole))
            {
                GameEvents.current.FireEvent_MarkHoleAsWiring(point);
                _holes.Enqueue(point);
            }

            TrySpawnOnBreadboard();
        }

        private void TrySpawnOnBreadboard()
        {
            if (_holes.Count == _activeSpawnablePrefabHolesNeeded)
            {
                // Breadboard holes
                Transform H_1_place = _holes.Dequeue();
                Transform H_2_place = _holes.Dequeue();

                GameEvents.current.FireEvent_UnmarkHoleAsWiring(H_1_place);
                GameEvents.current.FireEvent_UnmarkHoleAsWiring(H_2_place);

                Breadboard breadboard = _breadboard.GetComponent<Breadboard>();

                if(breadboard.HolesOccupied(H_1_place, H_2_place, out string errorText))
                {
                    GameEvents.current.FireEvent_HUDMessage(errorText, Controllers.HUDMessageType.Error);
                    return;
                }

                if (breadboard.HolesNearbyInline(H_1_place, H_2_place))
                {
                    // Init prefab
                    GameObject prefab = Instantiate(_activeSpawnablePrefab);

                    PlaceComponentOnBreadboard(prefab, H_1_place, H_2_place);

                    ComponentType type = ComponentType.Unknown;
                    foreach (Tag tag in prefab.GetComponent<BaseComponent>().Tags)
                    {
                        switch (tag)
                        {
                            case Tag.LED:
                                {
                                    type = ComponentType.LED;
                                    prefab.GetComponent<LED>().CathodeCoord = // Breadboard first click is always Cathode (-)
                                        H_1_place.name.Replace("H_", ""); // Name is like H_G1, etc., we only need postfix

                                    prefab.GetComponent<LED>().AnodeCoord = // Second click is always Anode (+)
                                        H_2_place.name.Replace("H_", ""); // Name is like H_J2, etc., we only need postfix
                                    break;
                                }
                            case Tag.Resistor:
                                {
                                    type = ComponentType.Resistor;
                                    prefab.GetComponent<Resistor>().SetResistance(_resistorOhmConfigured);

                                    prefab.GetComponent<Resistor>().Coord1 = // No polarity component, coord1 might be coord2,
                                                                             // It doesn't matter
                                        H_1_place.name.Replace("H_", ""); // Name is like H_G1, etc., we only need postfix

                                    prefab.GetComponent<Resistor>().Coord2 = // No polarity component, coord1 might be coord2,
                                                                             // It doesn't matter
                                        H_2_place.name.Replace("H_", ""); // Name is like H_G1, etc., we only need postfix

                                    break;
                                }
                        }
                    }

                    // Inform about occupied breaboard holes
                    GameEvents.current.FireEvent_ComponentSpawnBreadboard(H_1_place, H_2_place, type, prefab);
                    //GameEvents.current.FireEvent_HUDMessage("Component spawned!", Controllers.HUDMessageType.Info);
                }
                else
                {
                    GameEvents.current.FireEvent_HUDMessage("Draudžiamos mazgų pozicijos!", Controllers.HUDMessageType.Error);
                }
            }
        }

        private void PlaceComponentOnBreadboard(GameObject prefab, Transform H_1_place, Transform H_2_place)
        {
            float yShift = 0.01f; // Hide placement guide

            // Prefab guide to hole position
            prefab.transform.position = new Vector3(H_1_place.position.x, H_1_place.position.y - yShift, H_1_place.position.z);

            // Fix prefab position by certain rotations
            bool H_horizontalInline = H_1_place.position.z == H_2_place.position.z; // H_1 and H_2 are inline (horizontally)
            if (H_horizontalInline)
            {
                bool H_1_onRight = H_1_place.position.x < H_2_place.position.x; // H_1 is on right compared to H_2
                if (H_1_onRight)
                {
                    // Rotate -180 degrees
                    Vector3 rotation = prefab.transform.rotation.eulerAngles;
                    rotation = new Vector3(rotation.x, -180, rotation.z);
                    prefab.transform.rotation = Quaternion.Euler(rotation);
                }
            }
            else
            {
                bool H_1_lower = H_1_place.position.z > H_2_place.position.z; // H_1 is lower than H_2 (vertically)
                if (H_1_lower)
                {
                    // Rotate -90 degrees
                    Vector3 rotation = prefab.transform.rotation.eulerAngles;
                    rotation = new Vector3(rotation.x, -90, rotation.z);
                    prefab.transform.rotation = Quaternion.Euler(rotation);
                }
                else
                {
                    // Rotate 90 degrees
                    Vector3 rotation = prefab.transform.rotation.eulerAngles;
                    rotation = new Vector3(rotation.x, 90, rotation.z);
                    prefab.transform.rotation = Quaternion.Euler(rotation);
                }
            }
        }

        public void InitOrdered()
        {
            if (_spawnablePrefabs.Any())
            {
                PopulateInventory();
            }
        }

        public int GetInitOrder()
        {
            return 2;
        }
    }
}
