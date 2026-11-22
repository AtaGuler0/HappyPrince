using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>
    /// Mutlu Prens heykeli. Kılıç kabzasındaki yakut alınabilir; yakut uzaklaştıkça Prens'in yakut gözlerinin
    /// ışığı söner ve teslimden sonra heykelin yaldızı donuklaşır (yardımın Prens'e bedeli).
    /// </summary>
    public class PrinceStatue : MonoBehaviour
    {
        [Header("Parçalar")]
        public Renderer[] eyeRenderers;
        public Light eyeLight;
        public GameObject swordRuby;
        public Light rubyLight;
        public GameObject emptySocket;
        public Renderer[] goldRenderers;
        [Tooltip("Masaldaki gibi: bulutsuz gökten Kırlangıç'ın üstüne düşen damlalar Prens'in gözyaşlarıdır.")]
        public ParticleSystem[] tears;

        [Header("Noktalar")]
        public Transform rubyPoint;
        public Transform approachPoint;

        [Header("Renkler")]
        public Color eyeEmission = new(4f, 0.25f, 0.3f);
        public Color eyeLitBase = new(0.8f, 0.05f, 0.08f);
        public Color eyeDarkBase = new(0.05f, 0.05f, 0.06f);
        public Color goldColor = new(0.85f, 0.65f, 0.2f);
        public Color dullColor = new(0.42f, 0.4f, 0.36f);

        public float Sight { get; private set; } = 1f;

        private Material[] _eyeMaterials;
        private Material[] _goldMaterials;
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            _eyeMaterials = new Material[eyeRenderers.Length];
            for (int i = 0; i < eyeRenderers.Length; i++) _eyeMaterials[i] = eyeRenderers[i].material;
            _goldMaterials = new Material[goldRenderers.Length];
            for (int i = 0; i < goldRenderers.Length; i++) _goldMaterials[i] = goldRenderers[i].material;
            if (emptySocket) emptySocket.SetActive(false);
            SetSight(1f);
            SetDulled(0f);
        }

        private void Update()
        {
            if (rubyLight != null && swordRuby != null && swordRuby.activeSelf)
                rubyLight.intensity = 2.5f + Mathf.Sin(Time.time * 3f) * 1.2f;
        }

        /// <summary>1 = gözler parlak kırmızı, 0 = tamamen kararmış.</summary>
        public void SetSight(float t)
        {
            Sight = Mathf.Clamp01(t);
            foreach (var m in _eyeMaterials)
            {
                m.SetColor(BaseColor, Color.Lerp(eyeDarkBase, eyeLitBase, Sight));
                m.SetColor(EmissionColor, eyeEmission * Sight);
            }
            if (eyeLight != null) eyeLight.intensity = 1.5f * Sight;
        }

        public void SetDulled(float t)
        {
            var c = Color.Lerp(goldColor, dullColor, Mathf.Clamp01(t));
            foreach (var m in _goldMaterials) m.SetColor(BaseColor, c);
        }

        public void SetCrying(bool crying)
        {
            if (tears == null) return;
            foreach (var t in tears)
            {
                if (t == null) continue;
                if (crying && !t.isPlaying) t.Play();
                else if (!crying && t.isPlaying) t.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        public void RemoveRuby()
        {
            if (swordRuby) swordRuby.SetActive(false);
            if (emptySocket) emptySocket.SetActive(true);
        }
    }
}
