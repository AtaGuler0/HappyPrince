using MutluPrens.Core;
using MutluPrens.Player;
using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>Sokağı enlemesine kesen tetikleyici. Geçilince fener yanar, ısı dolar ve yeniden doğma noktası kaydedilir.</summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        public int index;
        public Transform spawnPoint;
        public Renderer lampRenderer;
        public Light lampLight;
        public Color offEmission = new(0.15f, 0.25f, 0.45f);
        public Color onEmission = new(3.5f, 2.2f, 0.9f);

        public bool IsLit { get; private set; }

        private Material _lampMaterial;
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            if (lampRenderer != null) _lampMaterial = lampRenderer.material;
            SetLit(false);
        }

        public void SetLit(bool lit)
        {
            IsLit = lit;
            if (_lampMaterial != null) _lampMaterial.SetColor(EmissionColor, lit ? onEmission : offEmission);
            if (lampLight != null)
            {
                lampLight.color = lit ? new Color(1f, 0.75f, 0.45f) : new Color(0.5f, 0.65f, 1f);
                lampLight.intensity = lit ? 6f : 1.5f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (SwallowController.FromCollider(other) != null && GameManager.Instance != null)
                GameManager.Instance.ReachCheckpoint(this);
        }
    }
}
