using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>Kinematik Rigidbody varsa fizikle uyumlu şekilde, yoksa doğrudan Transform üzerinden hareket ettirir.</summary>
    public abstract class KinematicMotion : MonoBehaviour
    {
        protected Rigidbody Body;
        protected Vector3 StartPosition;
        protected Quaternion StartRotation;

        protected virtual void Awake()
        {
            Body = GetComponent<Rigidbody>();
            if (Body != null)
            {
                Body.isKinematic = true;
                Body.interpolation = RigidbodyInterpolation.Interpolate;
            }
            StartPosition = transform.position;
            StartRotation = transform.rotation;
        }

        protected void Apply(Vector3 position, Quaternion rotation)
        {
            if (Body != null)
            {
                Body.MovePosition(position);
                Body.MoveRotation(rotation);
            }
            else
            {
                transform.SetPositionAndRotation(position, rotation);
            }
        }
    }
}
