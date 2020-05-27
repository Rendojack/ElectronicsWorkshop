using Assets.Scripts.Feature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Events;
using UnityEngine;

namespace Assets.Scripts
{
    public partial class GameEvents : MonoBehaviour
    {
        public static GameEvents current;

        private void Awake()
        {
            current = this;
        }

        public event Action<int> Event_OnDoorwayTriggerEnter;
        public void FireEvent_DoorwayTriggerEnter(int id)
        {
            Event_OnDoorwayTriggerEnter?.Invoke(id);
        }

        public event Action<int> Event_OnDoorwayTriggerExit;
        public void FireEvent_DoorwayTriggerExit(int id)
        {
            Event_OnDoorwayTriggerExit?.Invoke(id);
        }

        public event Action Event_OnOpenMainMenu;
        public void FireEvent_OpenMainMenu()
        {
            Event_OnOpenMainMenu?.Invoke();
        }

        public event Action Event_OnCloseMainMenu;
        public void FireEvent_CloseMainMenu()
        {
            Event_OnCloseMainMenu?.Invoke();
        }

        public event Action Event_OnClosePrompt;
        public void FireEvent_ClosePrompt()
        {
            Event_OnClosePrompt?.Invoke();
        }

        public event Action<PromptController.PromptType, string> Event_OnOpenPrompt;
        public void FireEvent_OpenPrompt(PromptController.PromptType promptType, string text)
        {
            Event_OnOpenPrompt?.Invoke(promptType, text);
        }

        public event Action Event_OnPlayerJump;
        public void FireEvent_PlayerJump()
        {
            Event_OnPlayerJump?.Invoke();
        }

        public event Action Event_OnPlayerDragObject;
        public void FireEvent_PlayerDragObject()
        {
            Event_OnPlayerDragObject?.Invoke();
        }

        public event Action Event_OnPlayerDropbject;
        public void FireEvent_PlayerDropObject()
        {
            Event_OnPlayerDropbject?.Invoke();
        }

        public event Action Event_OnPlayerCanPickObject;
        public void FireEvent_PlayerCanPickObject()
        {
            Event_OnPlayerCanPickObject?.Invoke();
        }

        public event Action Event_OnPlayerCantPickObject;
        public void FireEvent_PlayerCantPickObject()
        {
            Event_OnPlayerCantPickObject?.Invoke();
        }

        public event Action Event_OnPlayerThrowObject;
        public void FireEvent_PlayerThrowObject()
        {
            Event_OnPlayerThrowObject?.Invoke();
        }

        public event Action<float, MouseWheelDirection> Event_OnMouseWheelScroll;
        public void FireEvent_MouseWheelScroll(float distance, MouseWheelDirection direction)
        {
            Event_OnMouseWheelScroll?.Invoke(distance, direction);
        }

        public event Action Event_OnPlayerTryDragObject;
        public void FireEvent_PlayerTryDragObject()
        {
            Event_OnPlayerTryDragObject?.Invoke();
        }

        public event Action<Transform> Event_OnMousePoint;
        public void FireEvent_MousePoint(Transform point)
        {
            Event_OnMousePoint?.Invoke(point);
        }

        public event Action<Transform> Event_OnMouseLeftClick;
        public void FireEvent_MouseLeftClick(Transform point)
        {
            FireEvent_PlaySound(SoundController.SoundType.Click);
            Event_OnMouseLeftClick?.Invoke(point);
        }

        public event Action Event_OnClearAllFieldFocus;
        public void FireEvent_ClearAllFieldFocus()
        {
            Event_OnClearAllFieldFocus?.Invoke();
        }

        public event Action Event_OnChapterButtonClick;
        public void FireEvent_ChapterButtonClick()
        {
            Event_OnChapterButtonClick?.Invoke();
        }

        public event Action Event_OnSlideUnlock;
        public void FireEvent_UnlockSlide()
        {
            Event_OnSlideUnlock?.Invoke();
        }

        public event Action<Transform> Event_OnMouseRightClick;
        public void FireEvent_MouseRightClick(Transform point)
        {
            FireEvent_PlaySound(SoundController.SoundType.Click);
            Event_OnMouseRightClick?.Invoke(point);
        }

        public event Action<BaseComponent> Event_OnBaseComponentCreation;
        public void FireEvent_BaseComponentCreation(BaseComponent baseComponent)
        {
            Event_OnBaseComponentCreation?.Invoke(baseComponent);
        }

        public event Action<string, Controllers.HUDMessageType> Event_OnHUDMessage;
        public void FireEvent_HUDMessage(string message, Controllers.HUDMessageType type)
        {
            Event_OnHUDMessage?.Invoke(message, type);
        }

        public event Action<Transform, Transform, ComponentType, GameObject> Event_OnComponentSpawnBreadboard;
        public void FireEvent_ComponentSpawnBreadboard(Transform H_1, Transform H_2, ComponentType type, GameObject component)
        {
            Event_OnComponentSpawnBreadboard?.Invoke(H_1, H_2, type, component);
        }

        public event Action<GameObject> Event_OnComponentDestroyBreadboard;
        public void FireEvent_ComponentDestroyBreadboard(GameObject component)
        {
            Event_OnComponentDestroyBreadboard?.Invoke(component);
        }

        public event Action<Sprite, GameObject, string> Event_OnInventoryItemAdd;
        public void FireEvent_InventoryItemAdd(Sprite icon, GameObject item, string description)
        {
            Event_OnInventoryItemAdd?.Invoke(icon, item, description);
        }

        public event Action<GameObject> Event_OnInventoryItemRemove;
        public void FireEvent_InventoryItemRemove(GameObject item)
        {
            Event_OnInventoryItemRemove?.Invoke(item);
        }

        public event Action<Inventory.InventoryItem> Event_OnInventoryActiveItemChanged;
        public void FireEvent_InventoryActiveItemChanged(Inventory.InventoryItem item)
        {
            Event_OnInventoryActiveItemChanged?.Invoke(item);
        }

        public event Action<string> Event_OnUpdateActiveInventoryItemDesc;
        public void FireEvent_UpdateActiveInventoryItemDesc(string desc)
        {
            Event_OnUpdateActiveInventoryItemDesc?.Invoke(desc);
        }

        public event Action<GamemodeButton.GameMode> Event_OnGameModeSwitch;
        public void FireEvent_GameModeSwitch(GamemodeButton.GameMode gameMode)
        {
            Event_OnGameModeSwitch?.Invoke(gameMode);
        }

        public event Action<string> Event_OnSetHUDText;
        public void FireEvent_SetHUDText(string text)
        {
            Event_OnSetHUDText?.Invoke(text);
        }

        public event Action Event_OnOpenHUDCenteredMenu;
        public void FireEvent_OpenHUDCenteredMenu()
        {
            Event_OnOpenHUDCenteredMenu?.Invoke();
        }

        public event Action Event_OnCloseHUDCenteredMenu;
        public void FireEvent_CloseHUDCenteredMenu()
        {
            Event_OnCloseHUDCenteredMenu?.Invoke();
        }

        public event Action Event_OnRemoveHUDText;
        public void FireEvent_RemoveHUDText()
        {
            Event_OnRemoveHUDText?.Invoke();
        }

        public event Action Event_OnGamemodeButtonUnlock;
        public void FireEvent_UnlockGamemodeButton()
        {
            Event_OnGamemodeButtonUnlock?.Invoke();
        }

        public event Action Event_OnGamemodeButtonLock;
        public void FireEvent_LockGamemodeButton()
        {
            Event_OnGamemodeButtonLock?.Invoke();
        }

        public event Action Event_OnChapterButtonUnlock;
        public void FireEvent_UnlockChapterButton()
        {
            Event_OnChapterButtonUnlock?.Invoke();
        }

        public event Action Event_OnValidationButtonUnlock;
        public void FireEvent_UnlockValidationButton()
        {
            Event_OnValidationButtonUnlock?.Invoke();
        }

        public event Action Event_OnValidationButtonLock;
        public void FireEvent_LockValidationButton()
        {
            Event_OnValidationButtonLock?.Invoke();
        }

        public event Action Event_OnChapterButtonLock;
        public void FireEvent_LockChapterButton()
        {
            Event_OnChapterButtonLock?.Invoke();
        }

        public event Action Event_OnUnlockAllChapters;
        public void FireEvent_UnlockAllChapters()
        {
            Event_OnUnlockAllChapters?.Invoke();
        }

        public event Action Event_OnLockScreen;
        public void FireEvent_LockScreen()
        {
            Event_OnLockScreen?.Invoke();
        }

        public event Action Event_OnGoToGUIMode;
        public void FireEvent_GoToGUIMode()
        {
            Event_OnGoToGUIMode?.Invoke();
        }

        public event Action Event_OnReturnFromGUIMode;
        public void FireEvent_ReturnFromGUIMode()
        {
            Event_OnReturnFromGUIMode?.Invoke();
        }

        public event Action<List<string>> Event_OnHUDCenteredMenuClosed;
        public void FireEvent_HUDCenteredMenuClosed(List<string> fieldValues)
        {
            Event_OnHUDCenteredMenuClosed?.Invoke(fieldValues);
        }

        public event Action<Transform> Event_OnMarkHoleAsWiring;
        public void FireEvent_MarkHoleAsWiring(Transform hole)
        {
            Event_OnMarkHoleAsWiring?.Invoke(hole);
        }

        public event Action<Transform> Event_OnUnmarkHoleAsWiring;
        public void FireEvent_UnmarkHoleAsWiring(Transform hole)
        {
            Event_OnUnmarkHoleAsWiring?.Invoke(hole);
        }

        public event Action<int> Event_OnValidateChallenge;
        public void FireEvent_ValidateChallenge(int challengeIndex)
        {
            Event_OnValidateChallenge?.Invoke(challengeIndex);
        }

        public event Action<int, bool> Event_OnChallengeValidated;
        public void FireEvent_ChallengeValidated(int challengeIndex, bool isValid)
        {
            Event_OnChallengeValidated?.Invoke(challengeIndex, isValid);
        }

        public event Action Event_OnRequestChallengeValidation;
        public void FireEvent_RequestChallengeValidation()
        {
            Event_OnRequestChallengeValidation?.Invoke();
        }

        public event Action<SoundController.SoundType, bool> Event_OnPlaySound;
        public void FireEvent_PlaySound(SoundController.SoundType soundType, bool longSound = false)
        {
            Event_OnPlaySound?.Invoke(soundType, longSound);
        }

        public event Action Event_OnStopLongSound;
        public void FireEvent_StopLongSound()
        {
            Event_OnStopLongSound?.Invoke();
        }

        public event Action Event_OnDestroyLastBreadboardGameobject;
        public void FireEvent_DestroyLastBreadboardGameobject()
        {
            Event_OnDestroyLastBreadboardGameobject?.Invoke();
        }

        public event Action<string> Event_OnLogAction;
        public void FireEvent_LogAction(string text)
        {
            Event_OnLogAction?.Invoke(text);
        }

        public event Action<GameObject> Event_OnWireCreated;
        public void FireEvent_WireCreated(GameObject W)
        {
            Event_OnWireCreated?.Invoke(W);
        }

        public event Action<string> Event_OnHelpPrompt;
        public void FireEvent_SendHelpPrompt(string helpText)
        {
            Event_OnHelpPrompt?.Invoke(helpText);
        }
    }
}
