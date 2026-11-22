using MutluPrens.Player;
using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>İçindeyken ısı kazandırır (+) veya kaybettirir (-). Fırın penceresi, fenerler vb.</summary>
    [RequireComponent(typeof(Collider))]
    public class WarmthArea : MonoBehaviour
    {
        [Tooltip("Saniye başına ısı değişimi. Pozitif = ısıtır, negatif = üşütür.")]
        public float ratePerSecond = 20f;

        private void OnTriggerStay(Collider other)
        {
            var warmth = SwallowWarmth.FromCollider(other);
            if (warmth != null) warmth.AddExposure(ratePerSecond);
        }
    }
}
