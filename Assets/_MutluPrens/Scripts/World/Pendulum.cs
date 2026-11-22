using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>Sarkaç gibi ileri geri sallanır (sallanan tabelalar).</summary>
    public class Pendulum : KinematicMotion
    {
        public Vector3 localAxis = Vector3.forward;
        public float amplitude = 35f;
        public float period = 3f;
        [Range(0f, 1f)] public float phase;

        private void FixedUpdate()
        {
            float a = amplitude * Mathf.Sin((Time.time / period + phase) * Mathf.PI * 2f);
            Apply(StartPosition, StartRotation * Quaternion.AngleAxis(a, localAxis));
        }
    }
}
