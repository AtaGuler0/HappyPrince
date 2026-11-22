using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>Kamerayı izleyen kar yağışı. Göç ertelendikten sonra kar yoğunlaşır.</summary>
    public class SnowController : MonoBehaviour
    {
        public Transform follow;
        public ParticleSystem snow;
        public Vector3 offset = new(0f, 14f, 0f);

        private float _baseRate = -1f;

        private void Awake()
        {
            if (snow != null) _baseRate = snow.emission.rateOverTimeMultiplier;
        }

        public void SetIntensity(float multiplier)
        {
            if (snow == null) return;
            var e = snow.emission;
            e.rateOverTimeMultiplier = _baseRate * multiplier;
        }

        private void LateUpdate()
        {
            if (follow != null) transform.position = follow.position + offset;
        }
    }
}
