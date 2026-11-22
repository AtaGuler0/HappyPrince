using System;
using UnityEngine;

namespace MutluPrens.Player
{
    /// <summary>
    /// Kırlangıç'ın vücut ısısı. Zamanla, soğuk bölgelerde, hızlı uçarken ve çarpınca azalır;
    /// sıcak bölgelerde artar. Sıfıra düşerse Kırlangıç donar (başarısızlık).
    /// Göç ertelendikten sonra azami ısı düşer ve soğuma hızlanır (yardımın bedeli).
    /// </summary>
    [RequireComponent(typeof(SwallowController))]
    public class SwallowWarmth : MonoBehaviour
    {
        public float maxWarmth = 100f;
        [Tooltip("Saniyede temel ısı kaybı.")] public float baseDrain = 0.45f;
        public float boostDrain = 0.8f;
        public float ceilingDrain = 6f;

        [Header("Göç ertelendikten sonra")]
        [Range(0.3f, 1f)] public float postponedCapFactor = 0.7f;
        public float postponedDrainMultiplier = 1.4f;

        public float Current { get; private set; }
        public bool Active { get; set; }
        public bool MigrationPostponed { get; private set; }
        public float Cap => MigrationPostponed ? maxWarmth * postponedCapFactor : maxWarmth;
        public float Normalized => Current / maxWarmth;
        public float CapNormalized => Cap / maxWarmth;
        public bool IsWarming => _warmTimer > 0f;
        public bool IsInCold => _coldTimer > 0f;

        public event Action Frozen;

        private SwallowController _controller;
        private bool _frozen;
        private float _warmTimer;
        private float _coldTimer;

        public static SwallowWarmth FromCollider(Collider c)
        {
            if (c == null) return null;
            var rb = c.attachedRigidbody;
            return rb != null ? rb.GetComponent<SwallowWarmth>() : null;
        }

        private void Awake()
        {
            _controller = GetComponent<SwallowController>();
            Current = maxWarmth;
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            if (_warmTimer > 0f) _warmTimer -= dt;
            if (_coldTimer > 0f) _coldTimer -= dt;
            if (!Active || _frozen) return;

            float drain = baseDrain;
            if (_controller.IsBoosting) drain += boostDrain;
            if (_controller.AboveCeiling) { drain += ceilingDrain; _coldTimer = 0.25f; }
            if (MigrationPostponed) drain *= postponedDrainMultiplier;
            Change(-drain * dt);
        }

        /// <summary>Sıcak (+) veya soğuk (-) bölgeler her fizik adımında çağırır; değer saniye başınadır.</summary>
        public void AddExposure(float ratePerSecond)
        {
            if (!Active || _frozen) return;
            if (ratePerSecond > 0f) _warmTimer = 0.25f;
            else
            {
                _coldTimer = 0.25f;
                if (MigrationPostponed) ratePerSecond *= postponedDrainMultiplier;
            }
            Change(ratePerSecond * Time.fixedDeltaTime);
        }

        public void ApplyHit(float amount) => Change(-amount);

        public void Refill()
        {
            Current = Cap;
            _frozen = false;
        }

        public void SetMigrationPostponed(bool postponed)
        {
            MigrationPostponed = postponed;
            Current = Mathf.Min(Current, Cap);
        }

        private void Change(float delta)
        {
            if (_frozen) return;
            Current = Mathf.Clamp(Current + delta, 0f, Cap);
            if (Current <= 0f && Active)
            {
                _frozen = true;
                Frozen?.Invoke();
            }
        }
    }
}
