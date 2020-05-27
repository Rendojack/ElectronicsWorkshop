using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class Resistor : MonoBehaviour
{
    public static string RESISTANCE_VAR_TPL = "%resistance%";
    public static int DEFAULT_RESISTANCE_OHM = 270;
    [SerializeField] private int _resistancesOhm = DEFAULT_RESISTANCE_OHM;

    private bool _textShowing = false;
    private bool _mouseOver = false;

    public string Coord1 { get; set; } // One leg connection coord
    public string Coord2 { get; set; } // Second leg connection coord

    public void SetResistance(int resistanceOhm)
    {
        _resistancesOhm = resistanceOhm;
    }

    public int GetResistance()
    {
        return _resistancesOhm;
    }

    private void Start()
    {
        gameObject.name = GetThisName();
        GameEvents.current.Event_OnMousePoint += OnMousePoint;
    }

    private void OnMousePoint(Transform point)
    {
        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        if (baseComponent == null)
        {
            baseComponent = point.parent?.GetComponent<BaseComponent>();
        }

        _mouseOver = baseComponent != null && baseComponent.HasTag(Tag.Resistor) &&
            point.gameObject.name == GetThisName();
    }

    private string GetThisName()
    {
        // We need this as we use prefab for cloning.
        // Without ID, we end up on event listening collision,
        // because Tag.Resistor indication is not unique
        return $"Resistor_{GetInstanceID()}";
    }

    private void Update()
    {
        if (_mouseOver)
        {
            GameEvents.current.FireEvent_SetHUDText($"Rezistorius {_resistancesOhm} Ω");
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
