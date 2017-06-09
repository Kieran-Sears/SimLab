using UnityEngine.EventSystems;
using UnityEngine;

public class CusorSensor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public bool mouseOver = false;

    public void OnPointerEnter(PointerEventData eventData) {
        mouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        mouseOver = false;
    }

    public void OnDisable() {
        mouseOver = false;
    }
}
