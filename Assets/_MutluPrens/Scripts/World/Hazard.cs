using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>Kırlangıç çarparsa geri savrulur ve ısı kaybeder. Alt nesnelerdeki çarpıştırıcılar da sayılır.</summary>
    public class Hazard : MonoBehaviour
    {
        public string label = "Engel";
        public float knockback = 9f;
        public float warmthDamage = 25f;
    }
}
