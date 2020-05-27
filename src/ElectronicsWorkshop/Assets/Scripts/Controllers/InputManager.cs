using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class InputManager : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private Text _textCursor;
    private Text _text;
    private bool _textContainPlaceholder = true;

    private Text _title;
    private Image _fieldImage;

    private Stopwatch _backspacingTimer = new Stopwatch();
    private int BACKSPACING_START_AFTER_MS = 500;

    private bool _focus = false;
    private const int MAX_LETTERS_COUNT = 25;

    private void Start()
    {
        _fieldImage = GetComponent<Image>();

        SubscribeEvents();
        SetInitialValues();
    }

    public string GetText()
    {
        return _text.text;
    }

    private void SetInitialValues()
    {
        GameObject textObj = transform.GetChild(0).gameObject;
        GameObject titleObj = transform.GetChild(1).gameObject;
        GameObject textCursorObj = transform.GetChild(2).gameObject;

        _text = textObj.GetComponent<Text>();
        _title = titleObj.GetComponent<Text>();
        _textCursor = textCursorObj.GetComponent<Text>();

        SetPlaceholder();
    }

    private void RemovePlaceholder()
    {
        _text.color = Color.black;
        _text.text = string.Empty;

        _textContainPlaceholder = false;
    }

    private void SetPlaceholder()
    {
        _text.color = Color.grey;
        _textContainPlaceholder = true;
    }

    private void SubscribeEvents()
    {
        GameEvents.current.Event_OnMouseLeftClick += OnLeftMouseClick;
        GameEvents.current.Event_OnClearAllFieldFocus += OnClearFieldFocus;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetActiveField();
    }

    private void OnLeftMouseClick(Transform point)
    {
        SetActiveField();
    }

    private void OnClearFieldFocus()
    {
        SetInactiveField();
    }

    private void Update()
    {
        if (!_focus)
        {
            return;
        }

        // Single char delete
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            DeleteChar();

            _backspacingTimer.Reset();
            _backspacingTimer.Start();
        }
        // Continuous char delete
        else if (Input.GetKey(KeyCode.Backspace) && 
                 _backspacingTimer.ElapsedMilliseconds >= BACKSPACING_START_AFTER_MS)
        {
            DeleteChar();
        }
        // Any other char entered
        else if (Input.anyKey)
        {
            AddText(Input.inputString);
        }
    }

    private void DeleteChar()
    {
        if (_text.text.Any())
        {
            _text.text = _text.text.Substring(0, _text.text.Length - 1);
        }
    }

    private void AddText(string text)
    {
        if (_text.text.Length < MAX_LETTERS_COUNT)
        {
            if (_text.text.Length + text.Length > MAX_LETTERS_COUNT) // Cannot add whole string
            {
                text = (text.Substring(0, MAX_LETTERS_COUNT - _text.text.Length));
            }

            string extraText = GetNumbers(text);
            _text.text += extraText;
        }
    }

    private string GetNumbers(string input)
    {
        return new string(input.Where(c => char.IsDigit(c)).ToArray());
    }

    private void SetActiveField()
    {
        GameEvents.current.FireEvent_ClearAllFieldFocus();

        if (!_focus)
        {
            _focus = true;
            _fieldImage.color = new Color32(253, 251, 228, 255); // Light yellowish
            gameObject.AddComponent<Outline>();
        }
    }

    private void SetInactiveField()
    {
        if (_focus)
        {
            _focus = false;
            _fieldImage.color = Color.white;
            Destroy(gameObject.GetComponent<Outline>());
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        return;
    }
}
