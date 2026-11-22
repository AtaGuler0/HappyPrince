using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>Başlangıç noktası etrafında bir doğru boyunca gidip gelir (kepenk).</summary>
    public class Oscillator : KinematicMotion
    {
        public Vector3 offset = new(0f, 7f, 0f);
        public float period = 5f;
        [Range(0f, 1f)] public float phase;

        private void FixedUpdate()
        {
            float s = Mathf.Sin((Time.time / period + phase) * Mathf.PI * 2f);
            Apply(StartPosition + offset * s, StartRotation);
        }
    }
}
