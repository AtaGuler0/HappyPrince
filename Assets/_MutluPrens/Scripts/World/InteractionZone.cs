using MutluPrens.Core;
using MutluPrens.Player;
using UnityEngine;

namespace MutluPrens.World
{
    public enum InteractionKind
    {
        Ruby,     // Prens'in kılıcındaki yakut
        Delivery, // Terzi kadının penceresi
        Prince,   // Dönüşte Prens'in yanı
    }

    /// <summary>E tuşuyla etkileşim yapılabilecek alan. Hangi aşamada geçerli olduğuna GameManager karar verir.</summary>
    [RequireComponent(typeof(Collider))]
    public class InteractionZone : MonoBehaviour
    {
        public InteractionKind kind;

        private void OnTriggerEnter(Collider other)
        {
            if (SwallowController.FromCollider(other) != null && GameManager.Instance != null)
                GameManager.Instance.EnterZone(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (SwallowController.FromCollider(other) != null && GameManager.Instance != null)
                GameManager.Instance.ExitZone(this);
        }
    }
}
