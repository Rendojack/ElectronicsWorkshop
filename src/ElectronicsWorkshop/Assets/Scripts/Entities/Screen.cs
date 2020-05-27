using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Assets.Scripts;
using Assets.Scripts.Controllers;
using UnityEngine;

public class Screen : MonoBehaviour
{
    private bool _thisEnabled = true;
    private GamemodeButton.GameMode _currGamemode = GamemodeButton.GameMode.PlayMode;

    [SerializeField] private int _startLockSlidesFromIndex = -1;
    private int _currLockedSlideIndex = -1;

    [SerializeField] private Texture[] _slides;
    private int _currSlideIndex = 0;
    private int _biggestVisitedSlideIndex; // Prevents from accessing chapters that were never visited before (linear learning)

    [SerializeField] private int[] _chapterIndices;
    private int _currChapterIndex = -1; // Not initialized
    private int _biggestVisitedChapterIndex = -1; // Not initialized

    private MeshRenderer _meshRenderer;

    [SerializeField] private int _unlockChapterButtonSlideIndex = -1; // On which slide unlocks content
    [SerializeField] private int _unlockGamemodeButtonSlideIndex = -1; // On which slide unlocks content
    [SerializeField] private int _unlockValidationButtonSlideIndex = -1; // On which slide unlocks content
    private const int UNLOCK_HUD_MSG_DELAY_MS = 3_000;

    [SerializeField] private int[] _challengeIndices;
    private bool[] _shouldValidateChallenge;

    private List<int> _skipSlidesIndices = new List<int>(); // Indices to skip

    private void Start()
    {
        SubscribeEvents();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material.SetTexture("_MainTex", _slides[0]);

        if (_chapterIndices != null && _chapterIndices.Any())
        {
            _currChapterIndex = _chapterIndices.First(); // Start from the first chapter
        }

        if (_startLockSlidesFromIndex > 0) // If initialized
        {
            _currLockedSlideIndex = _startLockSlidesFromIndex;
        }

        _shouldValidateChallenge = new bool[_challengeIndices.Length];
    }

    private void SubscribeEvents()
    {
        GameEvents.current.Event_OnMouseLeftClick += OnMouseLeftClick;
        GameEvents.current.Event_OnMouseRightClick += OnMouseRightClick;
        GameEvents.current.Event_OnChapterButtonClick += OnChapterButtonClick;
        GameEvents.current.Event_OnSlideUnlock += OnUnlockSlide;
        GameEvents.current.Event_OnUnlockAllChapters += OnUnlockAllChapters;

        GameEvents.current.Event_OnGameModeSwitch += OnGamemodeChanged;
        GameEvents.current.Event_OnChallengeValidated += OnChallengeValidated;
        GameEvents.current.Event_OnRequestChallengeValidation += Current_Event_OnRequestChallengeValidation;
    }

    private void Current_Event_OnRequestChallengeValidation()
    {
        InitChallengeValidation();
    }

    private void OnGamemodeChanged(GamemodeButton.GameMode gamemode)
    {
        _currGamemode = gamemode;
    }

    private void InitChallengeValidation()
    {
        if (_challengeIndices.Contains(_currSlideIndex))
        {
            int _challengeIndex = new List<int>(_challengeIndices)
                .FindIndex(i => i == _currSlideIndex);

            if (_shouldValidateChallenge[_challengeIndex])
            {
                GameEvents.current.FireEvent_ValidateChallenge(_challengeIndex + 1);
            }
        }
    }
    
    private void OnChallengeValidated(int challengeIndex, bool isValid)
    {
        if (isValid)
        {
            _skipSlidesIndices.Add(_currSlideIndex); // Do not show completed tasks anymore

            UnlockNextSlide();
            NextSlide();

            int pointsWon = challengeIndex * 5;
            GameEvents.current.FireEvent_HUDMessage($"Laimėjote {pointsWon} {(pointsWon > 5 ? "taškų" : "taškus")}!", HUDMessageType.Info);
            if (challengeIndex == 3)
            {
                GameEvents.current.FireEvent_HUDMessage($"Laimėjote slaptą prizą!", HUDMessageType.Info);
            }

            string prompt = $"Sveikiname! {challengeIndex} užduotis atlikta!";
            if (challengeIndex < 3)
            {
                prompt += $"\\nEikime prie sekančios užduoties...";
            }

            GameEvents.current.FireEvent_OpenPrompt(PromptController.PromptType.Success, prompt);
            GameEvents.current.FireEvent_PlaySound(SoundController.SoundType.Success);
        }
    }

    private void OnUnlockAllChapters()
    {
        if (!_thisEnabled)
        {
            return;
        }

        if (_biggestVisitedChapterIndex != _chapterIndices.Last())
        {
            if (_currChapterIndex == -1) // Not initialized
            {
                _currChapterIndex = _chapterIndices.First();
            }

            _biggestVisitedSlideIndex = _slides.Length - 1;

            _biggestVisitedChapterIndex = _chapterIndices.Last();
        }
    }

    private void OnUnlockSlide()
    {
        if (!_thisEnabled)
        {
            return;
        }

        if (_currLockedSlideIndex < _slides.Length)
        {
            UnlockNextSlide();
        }
    }

    private void UnlockNextSlide()
    {
        _currLockedSlideIndex++;
        GameEvents.current.FireEvent_HUDMessage("Atrakinta nauja skaidrė!", HUDMessageType.Info);
    }

    private void OnChapterButtonClick()
    {
        if (!_thisEnabled)
        {
            return;
        }

        if (_currChapterIndex == -1)
        {
            return; // Not initialized
        }

        int index = 0;
        foreach (int chapterIndex in _chapterIndices)
        {
            if (_currChapterIndex == chapterIndex) // Find current chapter and pick next or first value (reset)
            {
                int nextChapterIndex = -1;
                if (index + 1 < _chapterIndices.Length && // Contains next value...
                    _biggestVisitedSlideIndex >= (nextChapterIndex = _chapterIndices[index + 1])) // And slide was visited before
                {
                    _currChapterIndex = nextChapterIndex; // Pick next chapter
                }
                else
                {
                    _currChapterIndex = _chapterIndices.First(); // Reset index to the first one
                }

                SetSlide(_currChapterIndex);
                return;
            }

            index++;
        }
    }

    private void UpdateCurrChapterIndex()
    {
        if (_currChapterIndex == -1)
        {
            return; // Not initialized
        }

        if (_biggestVisitedChapterIndex == -1)
        {
            _biggestVisitedChapterIndex = _currChapterIndex; // Initialize
        }

        int lastChapterIndex = _chapterIndices.First();
        int i = -1;
        foreach (int chapterIndex in _chapterIndices)
        {
            i++;
            if (_currSlideIndex == chapterIndex) // Starting boundary of new chapter
            {
                _currChapterIndex = _currSlideIndex;
                break;
            }

            if (_currSlideIndex > chapterIndex) // Might be inside next chapter, let's see...
            {
                lastChapterIndex = chapterIndex;
                continue;
            }

            if (_currSlideIndex < chapterIndex) // We were wrong. This is the right chapter to choose
            {
                _currChapterIndex = lastChapterIndex;
                break;
            }
        }

        if (_currChapterIndex > _biggestVisitedChapterIndex)
        {
            // Chapter changed!
            _biggestVisitedChapterIndex = _currChapterIndex;
            GameEvents.current.FireEvent_HUDMessage($"Atrakintas {i + 1} skyrius!", HUDMessageType.Info);
        }
    }

    private void SetSlide(int index)
    {
        _currSlideIndex = index;
        if (index > _biggestVisitedSlideIndex)
        {
            _biggestVisitedSlideIndex = index;
        }

        _meshRenderer.material.SetTexture("_MainTex", _slides[_currSlideIndex]);
        UpdateCurrChapterIndex();
    }

    private void NextSlide()
    {
        if (_currSlideIndex == _unlockChapterButtonSlideIndex)
        {
            Task.Run(() =>
            {
                Thread.Sleep(UNLOCK_HUD_MSG_DELAY_MS);
                GameEvents.current.FireEvent_UnlockChapterButton();
            });
        }

        if (_currSlideIndex == _unlockGamemodeButtonSlideIndex)
        {
            Task.Run(() =>
            {
                Thread.Sleep(UNLOCK_HUD_MSG_DELAY_MS);
                GameEvents.current.FireEvent_UnlockGamemodeButton();
            });
        }

        if (_currSlideIndex == _unlockValidationButtonSlideIndex)
        {
            Task.Run(() =>
            {
                Thread.Sleep(UNLOCK_HUD_MSG_DELAY_MS);
                GameEvents.current.FireEvent_UnlockValidationButton();
            });
        }

        if (_currSlideIndex + 1 == _currLockedSlideIndex)
        {
            GameEvents.current.FireEvent_HUDMessage("Atlikite užduotį norėdami pereiti į kitą skaidrę!", HUDMessageType.Warning);
            return;
        }

        do
        {
            _currSlideIndex++;

            if (_currSlideIndex + 1 == _slides.Length)
            {
                break;
            }
        }
        while (_skipSlidesIndices.Contains(_currSlideIndex));

        if (_currSlideIndex > _biggestVisitedSlideIndex)
        {
            _biggestVisitedSlideIndex = _currSlideIndex;
        }

        if (_challengeIndices.Contains(_currSlideIndex))
        {
            int _challengeIndex = new List<int>(_challengeIndices)
                .FindIndex(i => i == _currSlideIndex);

            _shouldValidateChallenge[_challengeIndex] = true;
        }

        _meshRenderer.material.SetTexture("_MainTex", _slides[_currSlideIndex]);

        UpdateCurrChapterIndex();
    }

    private void ReturnSlide()
    {
        do
        {
            _currSlideIndex--;

            if (_currSlideIndex == 0)
            {
                break;
            }
        }
        while (_skipSlidesIndices.Contains(_currSlideIndex));

        _meshRenderer.material.SetTexture("_MainTex", _slides[_currSlideIndex]);
        UpdateCurrChapterIndex();
    }

    private bool ShouldProcessMouseClick()
    {
        if (!_thisEnabled || _currGamemode == GamemodeButton.GameMode.EditMode)
        {
            return false;
        }

        return true;
    }

    private void OnMouseLeftClick(Transform point)
    {
        if (!ShouldProcessMouseClick())
        {
            return;
        }

        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        if (baseComponent != null && baseComponent.HasTag(Tag.Screen))
        {
            if (_currSlideIndex < _slides.Length - 1)
            {
                NextSlide();
            }
        }
    }

    private void OnMouseRightClick(Transform point)
    {
        if (!ShouldProcessMouseClick())
        {
            return;
        }

        BaseComponent baseComponent = point.GetComponent<BaseComponent>();
        if (baseComponent != null && baseComponent.HasTag(Tag.Screen))
        {
            if (_currSlideIndex > 0)
            {
                ReturnSlide();
            }
        }
    }
}
