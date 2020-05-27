using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Assets.Scripts;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public enum SoundType
    {
        Warning,
        Error,
        Info,
        Scroll,
        Success,
        Click,
        Use,
        Walk
    }

    [SerializeField] private AudioClip _warningSound;
    [SerializeField] private AudioClip _errorSound;
    [SerializeField] private AudioClip _infoSound;

    [SerializeField] private AudioClip _scrollSound;
    [SerializeField] private AudioClip _successSound;

    [SerializeField] private AudioClip _clickSound;
    [SerializeField] private AudioClip _useSound;

    [SerializeField] private AudioClip _walkSound;
    private void Start()
    {
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        GameEvents.current.Event_OnPlaySound += OnPlaySound;
        GameEvents.current.Event_OnStopLongSound += StopLongSound;
    }

    private void StopLongSound()
    {
        Transform child = transform.Find("LongAudioSource");
        AudioSource audio = child.GetComponent<AudioSource>();
        audio.Stop();
    }

    private void OnPlaySound(SoundType soundType, bool longSound)
    {
        AudioClip soundToPlay = null;
        switch (soundType)
        {
            case SoundType.Click:
            {
                soundToPlay = _clickSound;
                break;
            }

            case SoundType.Warning:
            case SoundType.Error:
            {
                soundToPlay = _errorSound;
                break;
            }

            case SoundType.Info:
            {
                soundToPlay = _infoSound;
                break;
            }

            case SoundType.Scroll:
            {
                soundToPlay = _scrollSound;
                break;
            }

            case SoundType.Success:
            {
                soundToPlay = _successSound;
                break;
            }

            case SoundType.Use:
            {
                soundToPlay = _useSound;
                break;
            }

            case SoundType.Walk:
            {
                soundToPlay = _walkSound;
                break;
            }
        }

        if (soundToPlay)
        {
            if (longSound)
            {
                Transform child = transform.Find("LongAudioSource");
                AudioSource audio = child.GetComponent<AudioSource>();

                if (!audio.isPlaying)
                {
                    audio.clip = soundToPlay;
                    audio.Play();
                }
            }
            else
            {
                AudioSource audio = gameObject.GetComponent<AudioSource>();
                audio.PlayOneShot(soundToPlay);
            }
        }
    }
}
