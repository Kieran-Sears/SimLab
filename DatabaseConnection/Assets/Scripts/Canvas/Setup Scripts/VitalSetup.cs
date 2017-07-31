using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class VitalSetup : MonoBehaviour {

    public TabManager tabManager;

    public InputField vitalName;
    public InputField vitalMax;
    public InputField vitalMin;
    public InputField vitalUnit;

    public GameObject vitalNameDuplicateWarning;

    void Start() {
        vitalName.onValidateInput += delegate (string input, int charIndex, char addedChar) { return VitalNameChangeValue(input, charIndex, addedChar); };
    }

    public void ClearAttributes() {
        vitalName.text = "";
        vitalMax.text = "";
        vitalMin.text = "";
        vitalUnit.text = "";
    }

    public void SelectMaxVitalValue() {
        Error.instance.informPanel.SetActive(false);
        vitalMax.Select();
        vitalMax.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    public void AddVital() {

        if (float.Parse(vitalMax.text) <= float.Parse(vitalMin.text)) {
            Error.instance.informMessageText.text = "Max value is less or equal to min value for vital";
            Error.instance.informPanel.SetActive(true);
            Error.instance.informOkButton.onClick.AddListener(SelectMaxVitalValue);
            return;
        }

        Transform vitalTrans = tabManager.transform.FindChild(vitalName.text);

        if (vitalTrans != null) {
            Destroy(vitalTrans.gameObject);
        }

        int vitalIndex = -1;

        for (int i = 0; i < ConditionSetup.Instance.vitals.Count; i++) {
            if (ConditionSetup.Instance.vitals[i].name == vitalName.text) {
                vitalIndex = i;
            }
        }

        if (vitalIndex != -1) {
            ConditionSetup.Instance.vitals.RemoveAt(vitalIndex);
            Destroy(ConditionSetup.Instance.vitalsChosen.transform.FindChild(vitalName.text).gameObject);
        }


        Vital vital = new Vital();
        vital.nodeID = ConditionSetup.Instance.vitals.Count;
        vital.name = vitalName.text;
        vital.units = vitalUnit.text;
        vital.max = float.Parse(vitalMax.text);
        vital.min = float.Parse(vitalMin.text);

        ConditionSetup.Instance.newVitalPanel.SetActive(false);
        WindowManager.instance.drug.SetActive(false);
        WindowManager.instance.condition.SetActive(true);

        GameObject toggleObject = Instantiate(ConditionSetup.Instance.togglePrefab);
        toggleObject.transform.SetParent(ConditionSetup.Instance.vitalsChosen.transform);
        toggleObject.transform.SetAsFirstSibling();
        toggleObject.transform.localScale = Vector3.one;
        toggleObject.transform.localPosition = Vector3.zero;
        toggleObject.transform.GetChild(1).GetComponent<Text>().text = vital.name;

        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.name = vital.name;
        toggle.isOn = false;
        toggle.onValueChanged.AddListener((bool value) => ConditionSetup.Instance.LoadChosenVital(value, toggleObject.transform.GetSiblingIndex(), vital.name));
        
        ConditionSetup.Instance.vitals.Insert(0, vital);

        ExportManager.Instance.SaveVital(vital, "/Resources/Vitals/"+vital.name+".xml");
       // ConditionSetup.instance.LoadChosenVital(true, 0, vital.name);

        // add checking here for is duplicate exists. If so then overwrite the existing vital
    }

    public char VitalNameChangeValue(string input, int charIndex, char character) {
        if (ConditionSetup.Instance.vitalsChosen.transform.FindChild(input + character) != null) {
            vitalNameDuplicateWarning.SetActive(true);
        } else {
            vitalNameDuplicateWarning.SetActive(false);
        }
        return character;
    }
}
