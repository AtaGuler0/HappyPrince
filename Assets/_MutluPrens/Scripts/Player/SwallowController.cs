using System;
using MutluPrens.World;
using UnityEngine;

namespace MutluPrens.Player
{
    /// <summary>
    /// Kırlangıç'ın arcade tarzı uçuş kontrolü. Rigidbody hızı her fizik adımında doğrudan ayarlanır;
    /// rüzgâr ve çarpma savrulması ayrı bir "dış hız" olarak eklenir ve zamanla söner.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class SwallowController : MonoBehaviour
    {
        [Header("Hız")]
        public float cruiseSpeed = 11f;
        public float boostSpeed = 17f;
        public float reverseSpeed = 3f;
        public float acceleration = 10f;
        public float deceleration = 9f;

        [Header("Dönüş ve dikey hareket")]
        public float turnSpeed = 115f;
        public float verticalSpeed = 7f;
        public float verticalAcceleration = 14f;

        [Header("Rüzgâr / çarpma")]
        public float externalDamping = 1.6f;
        public float hitInvulnerability = 1.5f;

        [Header("Uçuş sınırı")]
        [Tooltip("Bu yüksekliğin üstünde ayaz rüzgâr Kırlangıç'ı aşağı iter.")]
        public float ceilingHeight = 24f;
        public float ceilingPush = 30f;

        public bool InputEnabled { get; set; }
        public IFlightInputSource InputSource { get; set; } = new KeyboardFlightInput();

        public float ForwardSpeed => _forwardSpeed;
        public float VerticalVelocity => _verticalSpeed;
        public float TurnInput => _input.Turn;
        public bool IsBoosting => InputEnabled && _input.Boost && _input.Throttle > 0f;
        public bool AboveCeiling { get; private set; }
        public float WindStrength { get; private set; }
        public float Yaw => _yaw;

        /// <summary>Tehlikeli bir nesneye çarpıldığında (dokunulmazlık süresi dışında) tetiklenir.</summary>
        public event Action<Hazard> HitHazard;

        private Rigidbody _rb;
        private FlightInput _input;
        private float _yaw;
        private float _forwardSpeed;
        private float _verticalSpeed;
        private Vector3 _externalVelocity;
        private Vector3 _windAccumulator;
        private float _invulnerableTimer;

        public static SwallowController FromCollider(Collider c)
        {
            if (c == null) return null;
            var rb = c.attachedRigidbody;
            return rb != null ? rb.GetComponent<SwallowController>() : null;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.freezeRotation = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            _rb.sleepThreshold = 0f; // tetikleyici "Stay" olayları hiç kesilmesin
            _yaw = transform.eulerAngles.y;
        }

        private void Update()
        {
            _input = InputEnabled && InputSource != null ? InputSource.ReadFlight() : default;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            var input = InputEnabled ? _input : default;

            _yaw += input.Turn * turnSpeed * dt;

            float targetForward = 0f;
            if (input.Throttle > 0f) targetForward = (input.Boost ? boostSpeed : cruiseSpeed) * input.Throttle;
            else if (input.Throttle < 0f) targetForward = -reverseSpeed;
            float rate = Mathf.Abs(targetForward) > Mathf.Abs(_forwardSpeed) ? acceleration : deceleration;
            _forwardSpeed = Mathf.MoveTowards(_forwardSpeed, targetForward, rate * dt);

            _verticalSpeed = Mathf.MoveTowards(_verticalSpeed, input.Vertical * verticalSpeed, verticalAcceleration * dt);

            // Yükseklik sınırı: tavanın üstünde aşağı doğru güçlü ayaz.
            float y = _rb.position.y;
            AboveCeiling = y > ceilingHeight;
            if (AboveCeiling)
                _windAccumulator += Vector3.down * ceilingPush * (1f + (y - ceilingHeight));

            WindStrength = _windAccumulator.magnitude;
            _externalVelocity += _windAccumulator * dt;
            _externalVelocity *= Mathf.Exp(-externalDamping * dt);
            _windAccumulator = Vector3.zero;

            var rotation = Quaternion.Euler(0f, _yaw, 0f);
            _rb.linearVelocity = rotation * Vector3.forward * _forwardSpeed
                                 + Vector3.up * _verticalSpeed
                                 + _externalVelocity;
            _rb.MoveRotation(rotation);

            if (_invulnerableTimer > 0f) _invulnerableTimer -= dt;
        }

        /// <summary>Rüzgâr bölgeleri her fizik adımında ivme (m/s²) ekler.</summary>
        public void AddWind(Vector3 acceleration) => _windAccumulator += acceleration;

        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _rb.position = position;
            _rb.rotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
            transform.SetPositionAndRotation(position, _rb.rotation);
            _yaw = rotation.eulerAngles.y;
            _forwardSpeed = 0f;
            _verticalSpeed = 0f;
            _externalVelocity = Vector3.zero;
            _windAccumulator = Vector3.zero;
            _rb.linearVelocity = Vector3.zero;
            _invulnerableTimer = 1f;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var hazard = collision.collider.GetComponentInParent<Hazard>();
            if (hazard == null) return;
            Vector3 normal = collision.contactCount > 0 ? collision.GetContact(0).normal : -transform.forward;
            ReceiveHit(hazard, normal);
        }

        private void OnTriggerEnter(Collider other)
        {
            var hazard = other.GetComponentInParent<Hazard>();
            if (hazard == null) return;
            Vector3 away = transform.position - other.ClosestPoint(transform.position);
            ReceiveHit(hazard, away.sqrMagnitude > 0.0001f ? away.normalized : -transform.forward);
        }

        private void ReceiveHit(Hazard hazard, Vector3 normal)
        {
            if (_invulnerableTimer > 0f || !InputEnabled) return;
            _invulnerableTimer = hitInvulnerability;
            normal.y = Mathf.Clamp(normal.y, -0.3f, 0.6f);
            _externalVelocity = normal.normalized * hazard.knockback + Vector3.up * 1.5f;
            _forwardSpeed *= 0.2f;
            HitHazard?.Invoke(hazard);
        }
    }
}
