using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>
    /// Terzi kadının evi: pencereden bırakılan yakut masada, yüksüğün yanında parlar. Kırlangıç kanatlarıyla
    /// serinlettikçe hasta çocuğun soluk mavi rengi ısınır ve oda aydınlanır.
    /// </summary>
    public class SeamstressHouse : MonoBehaviour
    {
        public Transform approachPoint;
        public Transform returnSpawnPoint;
        public GameObject rubyOnTable;
        public Renderer childRenderer;
        public Light candleLight;
        public Light roomGlow;
        public Color sickColor = new(0.55f, 0.65f, 0.8f);
        public Color healthyColor = new(0.95f, 0.72f, 0.55f);

        public bool RubyPlaced { get; private set; }
        public float FanProgress { get; private set; }

        private Material _childMaterial;
        private float _candleBase;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        private void Awake()
        {
            if (childRenderer != null)
            {
                _childMaterial = childRenderer.material;
                _childMaterial.SetColor(BaseColor, sickColor);
            }
            if (rubyOnTable) rubyOnTable.SetActive(false);
            if (candleLight) _candleBase = candleLight.intensity;
            if (roomGlow) roomGlow.intensity = 0f;
        }

        private void Update()
        {
            if (candleLight != null)
                candleLight.intensity = _candleBase * (0.85f + 0.15f * Mathf.PerlinNoise(Time.time * 4f, 0.3f));
        }

        public void PlaceRuby()
        {
            if (RubyPlaced) return;
            RubyPlaced = true;
            if (rubyOnTable) rubyOnTable.SetActive(true);
            if (roomGlow) roomGlow.intensity = 1f;
        }

        /// <summary>0 = ateşler içinde, 1 = serinlemiş ve uykuya dalmış.</summary>
        public void SetFanProgress(float t)
        {
            FanProgress = Mathf.Clamp01(t);
            if (_childMaterial != null) _childMaterial.SetColor(BaseColor, Color.Lerp(sickColor, healthyColor, FanProgress));
            if (roomGlow) roomGlow.intensity = Mathf.Lerp(1f, 4f, FanProgress);
        }
    }
}
