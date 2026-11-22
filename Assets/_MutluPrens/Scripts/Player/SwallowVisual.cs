using UnityEngine;

namespace MutluPrens.Player
{
    /// <summary>
    /// Gri kutu Kırlangıç modelinin basit animasyonu: kanat çırpma, dönüşte yatış,
    /// taşınan yakut ve üşüdükçe buz mavisine dönen gövde rengi.
    /// </summary>
    public class SwallowVisual : MonoBehaviour
    {
        public SwallowController controller;
        public SwallowWarmth warmth;
        public Transform modelRoot;
        public Transform leftWing;
        public Transform rightWing;
        public Renderer[] bodyRenderers;
        public GameObject carriedRuby;

        public Color warmColor = new(0.12f, 0.16f, 0.32f);
        public Color frozenColor = new(0.62f, 0.8f, 0.95f);

        /// <summary>Çocuğu serinletirken hızlı ve geniş kanat çırpma.</summary>
        public bool Fanning { get; set; }

        private Material[] _bodyMaterials;
        private float _flapPhase;
        private float _bank;
        private float _pitch;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            _bodyMaterials = new Material[bodyRenderers.Length];
            for (int i = 0; i < bodyRenderers.Length; i++)
                _bodyMaterials[i] = bodyRenderers[i].material; // örnek (instance) materyal
            SetCarrying(false);
        }

        public void SetCarrying(bool carrying)
        {
            if (carriedRuby != null) carriedRuby.SetActive(carrying);
        }

        private void LateUpdate()
        {
            if (controller == null) return;
            float dt = Time.deltaTime;

            float speed01 = Mathf.Clamp01(Mathf.Abs(controller.ForwardSpeed) / controller.boostSpeed);
            float flapRate = Mathf.Lerp(5f, 14f, speed01) + (controller.VerticalVelocity > 0.5f ? 5f : 0f);
            if (Fanning) flapRate = 24f;
            _flapPhase += flapRate * dt;
            float flapAmp = Fanning ? 55f : controller.VerticalVelocity < -2f ? 8f : Mathf.Lerp(35f, 22f, speed01);
            float flap = Mathf.Sin(_flapPhase) * flapAmp;
            if (leftWing) leftWing.localRotation = Quaternion.Euler(0f, 0f, flap);
            if (rightWing) rightWing.localRotation = Quaternion.Euler(0f, 0f, -flap);

            _bank = Mathf.Lerp(_bank, -controller.TurnInput * 32f, 1f - Mathf.Exp(-6f * dt));
            _pitch = Mathf.Lerp(_pitch, -Mathf.Clamp(controller.VerticalVelocity, -7f, 7f) * 2.5f, 1f - Mathf.Exp(-6f * dt));
            if (modelRoot) modelRoot.localRotation = Quaternion.Euler(_pitch, 0f, _bank);

            if (warmth != null && _bodyMaterials != null)
            {
                float cold = 1f - Mathf.Clamp01(warmth.Normalized);
                float t = Mathf.Clamp01(cold * 1.1f + (warmth.MigrationPostponed ? 0.15f : 0f));
                var c = Color.Lerp(warmColor, frozenColor, t);
                foreach (var m in _bodyMaterials) m.SetColor(BaseColor, c);
            }
        }
    }
}
