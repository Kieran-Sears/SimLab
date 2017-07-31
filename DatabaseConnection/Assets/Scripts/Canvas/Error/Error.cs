using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Error : MonoBehaviour {

    public static Error instance { get; private set; }

    public GameObject informPanel;
    public GameObject boolPanel;

    public Text informMessageText;
    public Button informOkButton;

    public Text boolMessageText;
    public Button boolRightButton;
    public Button boolLeftButton;
    public Button boolCancelButton;
    public Dropdown boolDropdown;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        } else {
            instance = this;
        }
    }

    public void DeactivateErrorInformPanel() {
        instance.informMessageText.text = "";
        instance.informOkButton.onClick.RemoveAllListeners();
        instance.informPanel.SetActive(false);
    }

    public void DeactivateErrorBoolPanel() {
        instance.boolCancelButton.gameObject.SetActive(false);
        instance.boolMessageText.text = "";
        instance.boolLeftButton.onClick.RemoveAllListeners();
        instance.boolRightButton.onClick.RemoveAllListeners();
        boolCancelButton.onClick.RemoveAllListeners();
    }

}
