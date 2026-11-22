using MutluPrens.Player;
using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>
    /// Kırlangıç'ı belirli yönde iten rüzgâr bölgesi. İsteğe bağlı olarak dalgalar (bora) hâlinde eser
    /// ve içindeyken üşütür. Rüzgâr çizgisi parçacıkları bora gücüne göre yoğunlaşır.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class WindArea : MonoBehaviour
    {
        public Vector3 direction = Vector3.back;
        public float baseStrength = 4f;
        public float gustStrength = 14f;
        public bool gusty = true;
        public float gustInterval = 3.5f;
        public float gustDuration = 1.6f;
        public float timeOffset;
        [Tooltip("İçindeyken saniye başına ısı değişimi (negatif = üşütür).")]
        public float coldRate = -5f;
        public ParticleSystem streaks;

        public float GustFactor { get; private set; }
        public float CurrentStrength => baseStrength + GustFactor * gustStrength;

        private float _baseEmission = -1f;

        private void FixedUpdate()
        {
            if (!gusty) { GustFactor = 0f; return; }
            float t = Mathf.Repeat(Time.time + timeOffset, gustInterval);
            GustFactor = t < gustDuration ? Mathf.Sin(Mathf.PI * t / gustDuration) : 0f;
        }

        private void Update()
        {
            if (streaks == null) return;
            var emission = streaks.emission;
            if (_baseEmission < 0f) _baseEmission = emission.rateOverTimeMultiplier;
            float k = gusty ? Mathf.Lerp(0.2f, 1.6f, GustFactor) : 1f;
            emission.rateOverTimeMultiplier = _baseEmission * k;
        }

        private void OnTriggerStay(Collider other)
        {
            var swallow = SwallowController.FromCollider(other);
            if (swallow == null) return;
            swallow.AddWind(direction.normalized * CurrentStrength);
            if (coldRate != 0f)
            {
                var warmth = swallow.GetComponent<SwallowWarmth>();
                if (warmth != null) warmth.AddExposure(coldRate * (gusty ? 0.5f + GustFactor : 1f));
            }
        }
    }
}
