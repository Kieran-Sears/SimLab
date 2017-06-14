using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Error : MonoBehaviour {

    public static Error instance { get; private set; }

    public GameObject errorPanel;
    public GameObject errorInputPanel;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        } else {
            instance = this;
        }
    }

    public void PrintError(string message) {
        errorPanel.GetComponentInChildren<Text>().text = message;
        errorPanel.gameObject.SetActive(true);
    }

    public void PrintDurationError(string message) {
        errorInputPanel.GetComponentInChildren<Text>().text = message;
        errorInputPanel.gameObject.SetActive(true);
        errorInputPanel.GetComponentInChildren<Button>().onClick.AddListener(GetValue);
        InputField duration = errorInputPanel.GetComponentInChildren<InputField>();
        duration.onValidateInput += delegate (string input, int charIndex, char addedChar) { return SimulationSetup.instance.SimulationDurationChangeValue(input, charIndex, addedChar); };
        duration.Select();
        duration.ActivateInputField();
        duration.onEndEdit.AddListener(fieldValue => { GetValue(); });
    }

    public void GetValue() {
 
        SimulationSetup.instance.simulationDuration.text = errorInputPanel.GetComponentInChildren<InputField>().text;
        SimulationSetup.instance.loadChosenVital(true, 0, SimulationSetup.instance.vitalName.text);
        errorInputPanel.gameObject.SetActive(false);
    }


}
