using UnityEngine;

namespace MutluPrens.Player
{
    /// <summary>Ana menüde kamerayı Mutlu Prens'in sütunu etrafında yavaşça döndürür.</summary>
    public class MenuCamera : MonoBehaviour
    {
        public Vector3 pivot = new(0f, 13f, 0f);
        public float radius = 24f;
        public float height = 6f;
        public float degreesPerSecond = 4f;
        public float startAngle = 200f;
        public Vector3 lookOffset = new(0f, 2.5f, 0f);

        private float _angle;

        private void OnEnable() => _angle = startAngle;

        private void LateUpdate()
        {
            _angle += degreesPerSecond * Time.unscaledDeltaTime;
            float a = _angle * Mathf.Deg2Rad;
            var pos = pivot + new Vector3(Mathf.Sin(a) * radius, height, Mathf.Cos(a) * radius);
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(pivot + lookOffset - pos);
        }
    }
}
