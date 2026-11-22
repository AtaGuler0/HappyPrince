using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>
    /// Meydanın güneyinde dönen kırlangıç sürüsü. Kırlangıç kalmaya karar verince sürü güneye göç edip kaybolur.
    /// </summary>
    public class FlockOfSwallows : MonoBehaviour
    {
        public float radius = 9f;
        public float angularSpeed = 50f;
        public float leaveSpeed = 14f;
        public Vector3 leaveDirection = new(0f, 0.25f, -1f);

        public bool HasLeft { get; private set; }

        private Transform[] _birds;
        private float[] _offsets;
        private float[] _heights;
        private bool _leaving;
        private float _leaveTimer;

        private void Awake()
        {
            int n = transform.childCount;
            _birds = new Transform[n];
            _offsets = new float[n];
            _heights = new float[n];
            for (int i = 0; i < n; i++)
            {
                _birds[i] = transform.GetChild(i);
                _offsets[i] = i * (360f / n) + Random.Range(-10f, 10f);
                _heights[i] = Random.Range(-2f, 2f);
            }
        }

        public void Leave()
        {
            if (_leaving) return;
            _leaving = true;
            _leaveTimer = 0f;
        }

        private void Update()
        {
            float t = Time.time;
            if (!_leaving)
            {
                for (int i = 0; i < _birds.Length; i++)
                {
                    float a = (t * angularSpeed + _offsets[i]) * Mathf.Deg2Rad;
                    var local = new Vector3(Mathf.Cos(a) * radius, _heights[i] + Mathf.Sin(t * 2f + i) * 0.6f, Mathf.Sin(a) * radius);
                    var tangent = new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a));
                    _birds[i].position = transform.position + local;
                    _birds[i].rotation = Quaternion.LookRotation(tangent);
                }
                return;
            }

            if (HasLeft) return;
            _leaveTimer += Time.deltaTime;
            var dir = leaveDirection.normalized;
            for (int i = 0; i < _birds.Length; i++)
            {
                var b = _birds[i];
                var spread = new Vector3(Mathf.Sin(i * 1.7f), 0f, 0f) * 0.4f;
                var step = (dir + spread).normalized;
                b.position += step * leaveSpeed * Time.deltaTime;
                b.rotation = Quaternion.Slerp(b.rotation, Quaternion.LookRotation(step), Time.deltaTime * 3f);
            }
            if (_leaveTimer > 14f)
            {
                HasLeft = true;
                foreach (var b in _birds) b.gameObject.SetActive(false);
            }
        }
    }
}
