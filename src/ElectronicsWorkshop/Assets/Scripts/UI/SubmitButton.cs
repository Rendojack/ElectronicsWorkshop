using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.EventSystems;

public class SubmitButton : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string _onClickMethodCall;
    [SerializeField] private GameObject _gameEvents;
    public void OnPointerClick(PointerEventData eventData)
    {
        GameEvents gameEvents = _gameEvents.GetComponent<GameEvents>();
        gameEvents.SendMessage(_onClickMethodCall);
    }
}
