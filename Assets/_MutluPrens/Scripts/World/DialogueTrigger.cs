using MutluPrens.Core;
using MutluPrens.Player;
using UnityEngine;

namespace MutluPrens.World
{
    /// <summary>Kırlangıç girdiğinde (uygun aşamadaysa) altyazı repliklerini oynatır.</summary>
    [RequireComponent(typeof(Collider))]
    public class DialogueTrigger : MonoBehaviour
    {
        public string lineSetId;
        public GamePhase[] phases = { GamePhase.Deliver };
        public bool repeatable;
        public float cooldown = 10f;

        private bool _played;
        private float _lastTime = -999f;

        private void OnTriggerEnter(Collider other)
        {
            var gm = GameManager.Instance;
            if (gm == null || SwallowController.FromCollider(other) == null) return;
            if (System.Array.IndexOf(phases, gm.Phase) < 0) return;
            if (_played && (!repeatable || Time.time - _lastTime < cooldown)) return;
            _played = true;
            _lastTime = Time.time;
            gm.hud.QueueLines(StoryText.Get(lineSetId), false);
        }
    }
}
