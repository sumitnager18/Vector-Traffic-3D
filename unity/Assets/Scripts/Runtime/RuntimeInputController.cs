using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VectorTraffic3D.Runtime
{
    public sealed class RuntimeInputController : MonoBehaviour
    {
        public event Action<Vector2> PointerPressed;
        public event Action<Vector2, Vector2> PointerReleased;

        [SerializeField] private float swipeThresholdPixels = 30f;
        private Vector2 _start;
        private float _startTime;
        private bool _tracking;

        private void Update()
        {
            if (Touchscreen.current != null)
            {
                var touch = Touchscreen.current.primaryTouch;
                if (touch.press.wasPressedThisFrame)
                    Begin(touch.position.ReadValue());
                if (touch.press.wasReleasedThisFrame && _tracking)
                    End(touch.position.ReadValue());
            }

            if (Touchscreen.current == null && Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    Begin(Mouse.current.position.ReadValue());
                if (Mouse.current.leftButton.wasReleasedThisFrame && _tracking)
                    End(Mouse.current.position.ReadValue());
            }
        }

        private void Begin(Vector2 position)
        {
            _start = position;
            _startTime = Time.unscaledTime;
            _tracking = true;
            PointerPressed?.Invoke(position);
        }

        private void End(Vector2 position)
        {
            _tracking = false;
            PointerReleased?.Invoke(_start, position);
        }

        public bool IsSwipe(Vector2 delta) => delta.magnitude >= swipeThresholdPixels;
        public float PressDuration => Time.unscaledTime - _startTime;
    }
}
