using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>Sabit hızla kendi ekseni etrafında döner (rüzgârgülü kanatları).</summary>
    public class Rotator : KinematicMotion
    {
        public Vector3 localAxis = Vector3.forward;
        public float degreesPerSecond = 60f;
        private float _angle;

        private void FixedUpdate()
        {
            _angle += degreesPerSecond * Time.fixedDeltaTime;
            Apply(StartPosition, StartRotation * Quaternion.AngleAxis(_angle, localAxis));
        }
    }
}
