using UnityEngine;

namespace MutluPrens.World
{
    public enum GuideMode { None, Outbound, Return }

    /// <summary>
    /// Prens'ten terzi kadının evine uzanan rota. Oyuncunun rotadaki ilerlemesini, kalan mesafeyi ve
    /// pusula için "ileri bakış" noktasını hesaplar; yol ışıklarını ve hedef ışık sütununu yönetir.
    /// </summary>
    public class RouteGuide : MonoBehaviour
    {
        [Tooltip("Gidiş sırasıyla: Prens -> terzi kadının evi.")]
        public Transform[] waypoints;
        public GameObject guideLightsRoot;
        public Renderer[] guideLights;
        public Material outboundMaterial;
        public Material returnMaterial;
        public Transform beacon;

        public float TotalLength { get; private set; }
        public GuideMode Mode { get; private set; } = GuideMode.None;

        private Vector3[] _points;
        private float[] _cumulative;

        private void Awake()
        {
            _points = new Vector3[waypoints.Length];
            _cumulative = new float[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
            {
                _points[i] = waypoints[i].position;
                if (i > 0) _cumulative[i] = _cumulative[i - 1] + Vector3.Distance(_points[i - 1], _points[i]);
            }
            TotalLength = _cumulative[_cumulative.Length - 1];
            SetMode(GuideMode.None);
            SetBeacon(null);
        }

        public Vector3 GetPoint(int i) => _points[i];
        public int Count => _points.Length;

        public void SetMode(GuideMode mode)
        {
            Mode = mode;
            if (guideLightsRoot) guideLightsRoot.SetActive(mode != GuideMode.None);
            var mat = mode == GuideMode.Return ? returnMaterial : outboundMaterial;
            if (mat != null && guideLights != null)
                foreach (var r in guideLights) if (r) r.sharedMaterial = mat;
        }

        public void SetBeacon(Vector3? position)
        {
            if (beacon == null) return;
            beacon.gameObject.SetActive(position.HasValue);
            if (position.HasValue) beacon.position = new Vector3(position.Value.x, 0f, position.Value.z);
        }

        /// <summary>Konumun rota üzerindeki izdüşümünün başlangıçtan uzaklığı (metre).</summary>
        public float Project(Vector3 p)
        {
            float best = float.MaxValue, bestS = 0f;
            for (int i = 0; i < _points.Length - 1; i++)
            {
                Vector3 a = _points[i], ab = _points[i + 1] - a;
                float len2 = ab.sqrMagnitude;
                float t = len2 > 0f ? Mathf.Clamp01(Vector3.Dot(p - a, ab) / len2) : 0f;
                float d = (p - (a + ab * t)).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    bestS = _cumulative[i] + t * Mathf.Sqrt(len2);
                }
            }
            return bestS;
        }

        public Vector3 PointAt(float s)
        {
            s = Mathf.Clamp(s, 0f, TotalLength);
            for (int i = 0; i < _points.Length - 1; i++)
            {
                if (s <= _cumulative[i + 1])
                {
                    float seg = _cumulative[i + 1] - _cumulative[i];
                    float t = seg > 0f ? (s - _cumulative[i]) / seg : 0f;
                    return Vector3.Lerp(_points[i], _points[i + 1], t);
                }
            }
            return _points[_points.Length - 1];
        }

        public float Progress01(Vector3 p) => TotalLength > 0f ? Project(p) / TotalLength : 0f;

        public float RemainingDistance(Vector3 p, bool outbound)
        {
            float s = Project(p);
            return outbound ? TotalLength - s : s;
        }

        public Vector3 SteerPoint(Vector3 p, bool outbound, float lookAhead)
        {
            float s = Project(p);
            return PointAt(outbound ? s + lookAhead : s - lookAhead);
        }
    }
}
