using UnityEngine;

namespace MutluPrens.Player
{
    /// <summary>Kırlangıç'ı arkadan izleyen, duvarlara girmeyen basit takip kamerası.</summary>
    public class ChaseCamera : MonoBehaviour
    {
        public Transform target;
        public float distance = 7f;
        public float height = 2.4f;
        public float lookAhead = 4f;
        public float positionSmoothTime = 0.12f;
        public float yawFollowSharpness = 6f;
        public float collisionRadius = 0.35f;

        private float _yaw;
        private Vector3 _velocity;
        private float _shakeTimer;
        private float _shakeStrength;

        // "Ignore Raycast" (2) katmanındaki Kırlangıç hariç her şey.
        private const int Mask = ~(1 << 2);

        public void SnapToTarget()
        {
            if (target == null) return;
            _yaw = target.eulerAngles.y;
            transform.position = ComputeDesired(out _);
            transform.rotation = Quaternion.LookRotation(LookPoint() - transform.position);
            _velocity = Vector3.zero;
        }

        /// <summary>Ayarlar menüsünden kapatılabilir.</summary>
        public static bool ShakeEnabled = true;

        private float _blendTimer;
        private float _blendDuration;
        private bool _fixedActive;
        private Vector3 _fixedPos, _fixedLook;

        /// <summary>Kısa bir an için kamerayı sabit bir bakış noktasına alır (ör. çocuğu serinletirken pencereden içeri).</summary>
        public void SetFixedView(bool active, Vector3 position = default, Vector3 lookAt = default)
        {
            _fixedActive = active;
            _fixedPos = position;
            _fixedLook = lookAt;
            if (!active) BeginBlend(0.8f);
        }

        /// <summary>Kameranın mevcut konumundan takip konumuna yumuşak geçiş (menüden oyuna).</summary>
        public void BeginBlend(float duration)
        {
            _blendDuration = Mathf.Max(0.01f, duration);
            _blendTimer = _blendDuration;
            if (target != null) _yaw = target.eulerAngles.y;
        }

        public void Shake(float duration, float strength)
        {
            if (!ShakeEnabled) return;
            _shakeTimer = duration;
            _shakeStrength = strength;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            float dt = Time.deltaTime;
            _yaw = Mathf.LerpAngle(_yaw, target.eulerAngles.y, 1f - Mathf.Exp(-yawFollowSharpness * dt));

            if (_fixedActive)
            {
                transform.position = Vector3.SmoothDamp(transform.position, _fixedPos, ref _velocity, 0.45f);
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(_fixedLook - transform.position), 1f - Mathf.Exp(-4f * dt));
                return;
            }

            Vector3 desired = ComputeDesired(out bool blocked);
            float smooth = positionSmoothTime;
            if (_blendTimer > 0f)
            {
                _blendTimer -= dt;
                smooth = Mathf.Lerp(positionSmoothTime, 0.9f, Mathf.Clamp01(_blendTimer / _blendDuration));
                blocked = false;
            }
            Vector3 pos = Vector3.SmoothDamp(transform.position, desired, ref _velocity, smooth);
            if (blocked)
            {
                // Engel varsa içeri doğru hemen yaklaş; duvarın içinden görüntü olmasın.
                Vector3 pivot = Pivot();
                if ((pos - pivot).sqrMagnitude > (desired - pivot).sqrMagnitude) pos = desired;
            }

            if (_shakeTimer > 0f)
            {
                _shakeTimer -= dt;
                pos += Random.insideUnitSphere * _shakeStrength * Mathf.Clamp01(_shakeTimer * 3f);
            }

            transform.position = pos;
            var look = Quaternion.LookRotation(LookPoint() - pos);
            transform.rotation = _blendTimer > 0f
                ? Quaternion.Slerp(transform.rotation, look, 1f - Mathf.Exp(-5f * dt))
                : look;
        }

        private Vector3 Pivot() => target.position + Vector3.up * 0.4f;

        private Vector3 LookPoint() =>
            target.position + Quaternion.Euler(0f, _yaw, 0f) * Vector3.forward * lookAhead + Vector3.up * 0.6f;

        private Vector3 ComputeDesired(out bool blocked)
        {
            Vector3 pivot = Pivot();
            Vector3 desired = target.position + Quaternion.Euler(0f, _yaw, 0f) * new Vector3(0f, height, -distance);
            Vector3 dir = desired - pivot;
            float len = dir.magnitude;
            blocked = false;
            if (len > 0.01f && Physics.SphereCast(pivot, collisionRadius, dir / len, out var hit, len, Mask,
                    QueryTriggerInteraction.Ignore))
            {
                blocked = true;
                desired = pivot + dir / len * Mathf.Max(0.6f, hit.distance - 0.1f);
            }
            return desired;
        }
    }
}
