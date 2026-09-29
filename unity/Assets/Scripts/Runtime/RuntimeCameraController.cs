using UnityEngine;

namespace VectorTraffic3D.Runtime
{
    public sealed class RuntimeCameraController : MonoBehaviour
    {
        [SerializeField] private float followSharpness = 7f;
        [SerializeField] private float height = 10f;
        [SerializeField] private float distance = 8f;
        [SerializeField] private float angle = 52f;

        private Transform _target;
        private Camera _camera;

        public void Initialize(Camera camera, Transform target)
        {
            _camera = camera;
            _target = target;
            ApplyImmediate();
        }

        private void LateUpdate()
        {
            if (_target == null || _camera == null) return;

            var desired = _target.position + Quaternion.Euler(angle, 0f, 0f) *
                new Vector3(0f, 0f, -distance) + Vector3.up * height;
            var blend = 1f - Mathf.Exp(-followSharpness * Time.unscaledDeltaTime);
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, desired, blend);
            _camera.transform.LookAt(_target.position);
        }

        private void ApplyImmediate()
        {
            if (_target == null || _camera == null) return;
            _camera.transform.position = _target.position +
                Quaternion.Euler(angle, 0f, 0f) * new Vector3(0f, 0f, -distance) +
                Vector3.up * height;
            _camera.transform.LookAt(_target.position);
        }
    }
}
