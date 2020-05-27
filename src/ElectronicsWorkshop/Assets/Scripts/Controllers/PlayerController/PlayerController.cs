using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Events.Controllers
{
    public class PlayerController : BaseController
    {
        private bool _thisEnabled = true;
        private CharacterController _characterController;

        // Walking
        [SerializeField] private string _horizontalInputName = "Horizontal";
        [SerializeField] private string _verticalInputName = "Vertical";
        [SerializeField] private float _movementSpeed = 5f;

        // Jumping
        [SerializeField] private AnimationCurve _jumpFallOff = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
        [SerializeField] private float _jumpMultiplier = 5f;

        private bool _isJumping;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            GameEvents.current.Event_OnPlayerJump += Event_OnPlayerJump;
            GameEvents.current.Event_OnPlayerTryDragObject += Event_OnPlayerTryDragObject;

            GameEvents.current.Event_OnGoToGUIMode += GoToGUIMode;
            GameEvents.current.Event_OnReturnFromGUIMode += ReturnFromGUIMode;
        }

        private void GoToGUIMode()
        {
            _thisEnabled = false; // Disable player controller on GUI mode
        }

        private void ReturnFromGUIMode()
        {
            _thisEnabled = true; // Enable player controller when exiting GUI mode
        }

        private void Update()
        {
            if (!_thisEnabled)
            {
                return;
            }

            HandlePlayerMovement();
        }

        private Vector3 _lastForwardMovement;
        private Vector3 _lastRightMovement;

        private void HandlePlayerMovement()
        {
            float verticalInput = Input.GetAxis(_verticalInputName) * _movementSpeed;
            float horizontalInput = Input.GetAxis(_horizontalInputName) * _movementSpeed;

            Vector3 forwardMovement = transform.forward * verticalInput;
            Vector3 rightMovement = transform.right * horizontalInput;

            _characterController.SimpleMove(forwardMovement + rightMovement);

            //if (_lastRightMovement != rightMovement ||
            //    _lastForwardMovement != forwardMovement)
            //{
            //    GameEvents.current.FireEvent_PlaySound(SoundController.SoundType.Walk, true);
            //}
            //else
            //{
            //    GameEvents.current.FireEvent_StopLongSound();
            //}

            _lastForwardMovement = forwardMovement;
            _lastRightMovement = rightMovement;
        }

        private void Event_OnPlayerJump()
        {
            if (!_isJumping)
            {
                _isJumping = true;
                StartCoroutine(JumpRoutine());
            }
        }

        private void Event_OnPlayerTryDragObject()
        {
            
        }

        private IEnumerator JumpRoutine()
        {
            // Workaround for strange jitter when jumping next to walls
            _characterController.slopeLimit = 90.0f;

            float timeInAir = 0.0f;

            do
            {
                float jumpForce = _jumpFallOff.Evaluate(timeInAir);
                _characterController.Move(Vector3.up * jumpForce * _jumpMultiplier * Time.deltaTime);
                timeInAir += Time.deltaTime;

                yield return null;
            }
            while (!_characterController.isGrounded && _characterController.collisionFlags != CollisionFlags.Above);

            _isJumping = false;

            _characterController.slopeLimit = 45.0f;
        }

    }
}
