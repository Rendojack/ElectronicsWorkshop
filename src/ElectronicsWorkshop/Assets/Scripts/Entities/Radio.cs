using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Controllers;
using UnityEngine;

public class Radio : MonoBehaviour
{
    private HoverText _hoverText;
    private AudioSource _audio;
    private Shake _shake;

    [SerializeField] private List<AudioClip> _songs = new List<AudioClip>();
    [SerializeField] private List<string> _songNames = new List<string>();

    private int _currentAudioIndex = 0;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
        _shake = GetComponent<Shake>();
        _shake.enabled = false;

        _hoverText = GetComponent<HoverText>();
        _hoverText.SetText("Įjungti muziką");

        if (_songs.Any())
        {
            _audio.clip = _songs[0];
        }

        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        GameEvents.current.Event_OnMouseLeftClick += MouseLeftClick;
        GameEvents.current.Event_OnMouseRightClick += MouseRightClick;
    }

    private void MouseLeftClick(Transform point)
    {
        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        if (baseComponent != null && baseComponent.HasTag(Tag.Radio))
        {
            if (_audio.isPlaying)
            {
                _audio.Stop();
                _shake.enabled = false;
                _hoverText.SetText("Įjungti muziką");
            }
            else
            {
                PlaySong();
                _hoverText.SetText("Išjungti muziką/\nPerjungti dainą");
            }
        }
    }

    private void MouseRightClick(Transform point)
    {
        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        if (baseComponent != null && baseComponent.HasTag(Tag.Radio))
        {
            if (!_songs.Any() || !_audio.isPlaying)
            {
                return;
            }

            if (_currentAudioIndex + 1 < _songs.Count)
            {
                _currentAudioIndex++;
            }
            else
            {
                _currentAudioIndex = 0;
            }

            _audio.Stop();
            _audio.clip = _songs[_currentAudioIndex];
            PlaySong();
        }
    }

    private void PlaySong()
    {
        _audio.Play();
        _shake.enabled = true;
        GameEvents.current.FireEvent_HUDMessage("Groja daina: " + _songNames[_currentAudioIndex], HUDMessageType.Info);
    }
}
