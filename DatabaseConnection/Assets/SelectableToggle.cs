using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SelectableToggle : MonoBehaviour {

    public Toggle graphToggle;
    public Toggle editToggle;

    public void Start() {
        editToggle.onValueChanged.AddListener(ChangeColour);
    }

    private void ChangeColour(bool isOn) {
        if (isOn) {
            editToggle.GetComponentInChildren<Image>().color = editToggle.colors.pressedColor;
        } else {
            editToggle.GetComponentInChildren<Image>().color = editToggle.colors.normalColor;
        }
    }
}
