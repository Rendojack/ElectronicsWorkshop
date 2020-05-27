using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Feature;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Events.Controllers
{
    public class PlayerCameraController : BaseController
    {
        [SerializeField] private string _mouseXInputName = "Mouse X";
        [SerializeField] private string _mouseYInputName = "Mouse Y";
        [SerializeField] private float _mouseSensitivity = 150f;

        [SerializeField] private Transform _player;

        private bool _lockCamera = false;
        private float _xAxisClamp;

        private int _leftMouseButton = 0;
        private int _rightMouseButton = 1;

        private int _raycastLayerMask;
        private const int IGNORE_RAYCAST_LAYER_INDEX = 2;
        private bool _raycastPhysics = true;

        public void LockCamera(bool lockCamera)
        {
            _lockCamera = lockCamera;
        }

        private void Start()
        {
            InitRaycastMousePointerMask();
            SubscribeEvents();
        }

        private void Awake()
        {
            LockCursor();
            _xAxisClamp = -0.0f;
        }

        private void SubscribeEvents()
        {
           GameEvents.current.Event_OnGoToGUIMode += GoToGUIMode;
           GameEvents.current.Event_OnReturnFromGUIMode += ReturnFromGUIMode;
        }

        private void GoToGUIMode()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            _lockCamera = true;

            _raycastPhysics = false;
        }

        private void ReturnFromGUIMode()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            _lockCamera = false;

            _raycastPhysics = true;
        }

        private void LockCursor()
        {
            // Lock cursor to the center
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void InitRaycastMousePointerMask()
        {
            // Ignore player on raycast
            GameObject player = GameObject.Find("Player");
            int playerLayerIndex = player.layer;

            _raycastLayerMask = (1 << playerLayerIndex); // Cast rays against player
            _raycastLayerMask |= (1 << IGNORE_RAYCAST_LAYER_INDEX); // Cast rays against ignored index

            _raycastLayerMask = ~_raycastLayerMask; // Cast rays against all but supplied indices (inverted logic)
        }

        private void Update()
        {
            if (!_lockCamera)
                CameraRotation();

            RaycastMousePointerPhysics();
        }

        private void RaycastMousePointerPhysics()
        {
            if (!_raycastPhysics)
            {
                return;
            }

            // Find gameobject player is pointing at and fire event
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _raycastLayerMask))
            {
                GameEvents.current.FireEvent_MousePoint(hit.transform);

                if (Input.GetMouseButtonDown(_leftMouseButton))
                {
                    GameEvents.current.FireEvent_MouseLeftClick(hit.transform);
                }
                else if (Input.GetMouseButtonDown(_rightMouseButton))
                {
                    GameEvents.current.FireEvent_MouseRightClick(hit.transform);
                }
            }
        }

        private void CameraRotation()
        {
            float mouseX = Input.GetAxis(_mouseXInputName) * _mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis(_mouseYInputName) * _mouseSensitivity * Time.deltaTime;

            _xAxisClamp += mouseY;

            // Lock upmost position
            if (_xAxisClamp > 90.0f)
            {
                _xAxisClamp = 90.0f;
                mouseY = 0.0f;
                ClampXAxisRotationToValue(270.0f);
            }
            // Lock downmost position
            else if (_xAxisClamp < -90.0f)
            {
                _xAxisClamp = -90.0f;
                mouseY = 0.0f;
                ClampXAxisRotationToValue(90.0f);
            }

            transform.Rotate(Vector3.left * mouseY);
            _player.Rotate(Vector3.up * mouseX);
        }

        private void ClampXAxisRotationToValue(float value)
        {
            Vector3 eulerRotation = transform.eulerAngles;
            eulerRotation.x = value;
            transform.eulerAngles = eulerRotation;
        }
    }
}
