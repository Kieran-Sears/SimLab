using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Error : MonoBehaviour {

    public static Error instance { get; private set; }

    public GameObject errorPanel;
    public GameObject errorInputPanel;
    public GameObject lastFocusedObject;

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
        ErrorInput inputError = errorInputPanel.GetComponent<ErrorInput>();
        inputError.message.text = message;
        inputError.minutes.onValidateInput += delegate (string input, int charIndex, char addedChar) { return SimulationSetup.instance.SimulationDurationChangeValue(input, charIndex, addedChar); };
        inputError.seconds.onValidateInput += delegate (string input, int charIndex, char addedChar) { return SimulationSetup.instance.SimulationDurationChangeValue(input, charIndex, addedChar); };
        inputError.minutes.Select();
        inputError.minutes.ActivateInputField();
        inputError.minutes.onEndEdit.AddListener(fieldValue => { GetValue(); });
        inputError.seconds.onEndEdit.AddListener(fieldValue => { GetValue(); });
        errorInputPanel.gameObject.SetActive(true);
        errorInputPanel.GetComponentInChildren<Button>().onClick.AddListener(GetValue);
    }

    public void GetValue() {
        ErrorInput inputError = errorInputPanel.GetComponent<ErrorInput>();
        SimulationSetup.instance.simulationDurationMinutes.text = inputError.minutes.text;
        SimulationSetup.instance.simulationDurationSeconds.text = inputError.seconds.text;
        SimulationSetup.instance.loadChosenVital(true, 0, SimulationSetup.instance.vitalName.text);
        errorInputPanel.gameObject.SetActive(false);
    }


}
