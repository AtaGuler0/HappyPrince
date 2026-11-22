using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MutluPrens.UI
{
    /// <summary>Menü düğmesi görünümü: seçilince yazı altın rengine döner ve solunda küçük bir işaret belirir.</summary>
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler
    {
        public Text label;
        public GameObject marker;
        public Color normalColor = new(0.86f, 0.88f, 0.94f, 0.85f);
        public Color selectedColor = new(1f, 0.83f, 0.52f, 1f);

        private void OnEnable() => Apply(EventSystem.current != null && EventSystem.current.currentSelectedGameObject == gameObject);

        public void OnSelect(BaseEventData eventData) => Apply(true);
        public void OnDeselect(BaseEventData eventData) => Apply(false);

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(gameObject);
        }

        private void Apply(bool selected)
        {
            if (label) label.color = selected ? selectedColor : normalColor;
            if (marker) marker.SetActive(selected);
        }
    }
}
