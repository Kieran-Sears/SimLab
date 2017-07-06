using System;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrugSetup : MonoBehaviour {


    public static DrugSetup instance { get; private set; }

    public TabManager tabs;
    public GameObject inactiveTabs;
    public GameObject tableEntryPrefab;
    public GameObject togglePrefab;
    public GameObject graphs;



    #region Condition
   // public Dropdown presets;
    public InputField drugDurationMinutes;
    public InputField drugDurationSeconds;
    public Button durationSubmit;
    public int duration = -1;
    public GameObject vitalsChosen;
    #endregion

    //#region Drug
    //public GameObject newDrugPanel;
    //public InputField drugName;
    //public InputField drugUnit;
    //public InputField drugMax;
    //public InputField drugMin;
    //public InputField drugDuration;
    //public GameObject drugVitalsAffected;
    //public GameObject drugAdministrations;
    //#endregion

    private Drugs drugs;
    private Vitals vitals;
    private Equipment equipment;
    private Graph graph;

    private bool replaceExistingGraphs;
    private bool someBool;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        }
        else {
            instance = this;
        }
    }

    void Start() {
        replaceExistingGraphs = false;
        PopulateVitals();
        drugDurationMinutes.onValueChanged.AddListener(enableSubmitButton);
        drugDurationSeconds.onValueChanged.AddListener(enableSubmitButton);
       // vitalName.onValidateInput += delegate (string input, int charIndex, char addedChar) { return VitalNameChangeValue(input, charIndex, addedChar); };
    }

    public void enableSubmitButton(string input) {
        durationSubmit.interactable = true;
    }

    public void SubmitDuration() {
        int newDuration = 0;

        int minutes = 0;
        int seconds = 0;
        if (int.TryParse(drugDurationMinutes.text, out minutes)) {
            if (minutes > 60) {
                Error.instance.informMessageText.text = "Minutes cannot exceed 60.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            }
            else if (minutes < 0) {
                Error.instance.informMessageText.text = "Minutes cannot be less than 0.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            }
            else {
                newDuration += minutes * 60;
            }
        }

        if (int.TryParse(drugDurationSeconds.text, out seconds)) {
            if (seconds > 60) {
                Error.instance.informMessageText.text = "Seconds cannot exceed 60.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectSecondsDuration);
            }
            else if (seconds < 0) {
                Error.instance.informMessageText.GetComponentInChildren<Text>().text = "Seconds cannot be less than 0.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            }
            else if (minutes == 0 && seconds < 5) {
                Error.instance.informMessageText.text = "Simulation duration should exeed 5 seconds.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            }
            else {
                newDuration += seconds;
            }
            duration = newDuration;
        }
    }

    private void SelectMinutesDuration() {
        Error.instance.informPanel.SetActive(false);
        drugDurationMinutes.Select();
        drugDurationMinutes.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    private void SelectSecondsDuration() {
        Error.instance.informPanel.SetActive(false);
        drugDurationSeconds.Select();
        drugDurationSeconds.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    void PopulateVitals() {
        vitals = ExportManager.instance.Load("vitals") as Vitals;
        foreach (Vital vital in vitals.vitalList) {
            GameObject toggleObject = Instantiate(togglePrefab);
            toggleObject.transform.SetParent(vitalsChosen.transform);
            toggleObject.transform.localScale = Vector3.one;
            toggleObject.transform.localPosition = Vector3.zero;
            toggleObject.transform.GetChild(1).GetComponent<Text>().text = vital.name;
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.name = vital.name;
            toggle.isOn = false;
            //  toggle.group = vitalsToggleGroup;
            toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, toggleObject.transform.GetSiblingIndex(), vital.name));
        }
    }

    public void SetInformPanelToFalse() {
        Error.instance.informPanel.SetActive(false);
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    public void ResetDurationBack() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        drugDurationMinutes.text = (duration / 60).ToString();
        drugDurationSeconds.text = (duration % 60).ToString();
    }

    public void CancelAddingPrefab() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolDropdown.gameObject.SetActive(false);
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
    }

  

    public void loadChosenVital(bool chosen, int index, string vitalName) {
        tabs.transform.GetComponent<ToggleGroup>().SetAllTogglesOff();
        if (chosen) {

            Transform vitalTrans = graphs.transform.FindChild(vitalName);
            Transform vitalTab = inactiveTabs.transform.FindChild(vitalName);

            if (vitalTrans == null || vitalTab == null) {
                if (duration == -1) {
                    Error.instance.informMessageText.text = "Please set the duration of the drugs effects before adding vitals.";
                    Error.instance.informPanel.SetActive(true);
                    Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
                    vitalsChosen.transform.GetChild(index).GetComponent<Toggle>().isOn = false;
                    return;
                }
                graph = tabs.GenerateTab(vitals.vitalList[index].name);
                graph.GenerateGraph(0, duration, (int)Math.Ceiling(vitals.vitalList[index].min), (int)Math.Ceiling(vitals.vitalList[index].max), vitals.vitalList[index].units);
                if (graph.sortedGraphPointsList.Count == 0) {
                    int halfValue = (int)Math.Ceiling(((vitals.vitalList[index].max - vitals.vitalList[index].min) / 2) + vitals.vitalList[index].min);
                    graph.AddPoint(0, halfValue);
                    graph.AddPoint(duration, halfValue);
                }
                if (graph.pointsUpperThreshold.Count == 0) {
                    int quaterValue = (int)Math.Ceiling(((vitals.vitalList[index].max - vitals.vitalList[index].min) / 4) + vitals.vitalList[index].min);
                    graph.AddThresholdPointUpper(0, quaterValue);
                    graph.AddThresholdPointUpper(duration, quaterValue);
                }
                if (graph.pointsLowerThreshold.Count == 0) {
                    int threeQuaterValue = (int)Math.Ceiling((((vitals.vitalList[index].max - vitals.vitalList[index].min) / 4) * 3) + vitals.vitalList[index].min);
                    graph.AddThresholdPointLower(0, threeQuaterValue);
                    graph.AddThresholdPointLower(duration, threeQuaterValue);
                }
                graph.gameObject.SetActive(false);
                tabs.SwitchTab();
            }
            else {
                vitalTrans.gameObject.SetActive(true);
                vitalTab.SetParent(tabs.transform);
                vitalTab.gameObject.SetActive(true);
                tabs.SwitchTab();
            }
        }
        else {
            Transform tab = tabs.transform.FindChild(vitalName);
            if (tab != null) {
                tab.gameObject.SetActive(false);
                tab.SetParent(inactiveTabs.transform);
                graphs.transform.FindChild(vitalName).gameObject.SetActive(false);
            }
        }
    }

    //public void NewVitalPanelToggleActive() {
    //    newVitalPanel.SetActive(!newVitalPanel.activeInHierarchy);
    //}

    //public void NewDrugPanelToggleActive() {
    //    newDrugPanel.SetActive(!newDrugPanel.activeInHierarchy);
    //}

    public Vital GetVital(string vitalName) {
        foreach (Vital vital in vitals.vitalList) {
            if (vital.name == vitalName) {
                return vital;
            }
        }
        return null;
    }
}
