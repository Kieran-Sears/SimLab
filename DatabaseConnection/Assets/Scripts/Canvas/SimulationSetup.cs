using System;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationSetup : MonoBehaviour {

    public static SimulationSetup instance { get; private set; }

    public TabManager tabs;
    public GameObject tableEntryPrefab;
    public GameObject togglePrefab;
    public GameObject graphs;

    #region Condition
    public Dropdown presets;
    public InputField simulationDurationMinutes;
    public InputField simulationDurationSeconds;
    public Button durationSubmit;
    public int duration = -1;
    public GameObject vitalsChosen;
    public GameObject drugsChosen;
    public GameObject equipmentChosen;
    #endregion

    #region Vital
    public GameObject newVitalPanel;
    public InputField vitalName;
    public InputField vitalUnit;
    public InputField vitalMax;
    public InputField vitalMin;
    public GameObject vitalNameDuplicateWarning;
    #endregion

    #region Drug
    public GameObject newDrugPanel;
    public InputField drugName;
    public InputField drugUnit;
    public InputField drugMax;
    public InputField drugMin;
    public InputField drugDuration;
    public GameObject drugVitalsAffected;
    public GameObject drugAdministrations;
    #endregion

    #region Bins
    public GameObject inactiveTabs;
    #endregion

    private Drugs drugs;
    private Vitals vitals;
    private Equipment equipment;
    private Graph graph;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        }
        else {
            instance = this;
        }
    }

    void Start() {
        PopulatePresets();
        PopulateVitals();
        PopulateDrugs();
        PopulateEquipment();
        // dynamic check for user input for the duration of simulation
        //  simulationDurationMinutes.onValidateInput += delegate (string input, int charIndex, char addedChar) { return SimulationDurationChangeValue(input, charIndex, addedChar); };
        //  simulationDurationSeconds.onValidateInput += delegate (string input, int charIndex, char addedChar) { return SimulationDurationChangeValue(input, charIndex, addedChar); };
        simulationDurationMinutes.onValueChanged.AddListener(enableSubmitButton);

            vitalName.onValidateInput += delegate (string input, int charIndex, char addedChar) { return VitalNameChangeValue(input, charIndex, addedChar); };
    }

    // dynamic check of duration instigated from the start function within this class
    //public char SimulationDurationChangeValue(string input, int charIndex, char character) {
    //    if (charIndex > 1) {
    //        return '\0';
    //    }
    //    int v;
    //    if (int.TryParse(input + character.ToString(), out v)) {
    //        if (charIndex <= 1 && v > 60) {
    //            Error.instance.PrintError("Number cannot exceed 60");
    //            return '\0';
    //        }
    //    } else {
    //        Error.instance.PrintError("Enter numerical values only.");
    //        return '\0';
    //    }
    //    return character;
    //}

    public void enableSubmitButton(string input) {
        durationSubmit.interactable = true;
    }

    public void SubmitDuration() {

        int minutes = 0;
        int seconds = 0;
        if (int.TryParse(simulationDurationMinutes.text, out minutes)) {
            if (minutes > 60) {
                Error.instance.errorPanel.GetComponentInChildren<Text>().text = "Minutes cannot exceed 60.";
                Error.instance.errorPanel.gameObject.SetActive(true);
                Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.AddListener(SelectMinutesDuration);
            } else if (minutes < 0) {
                Error.instance.errorPanel.GetComponentInChildren<Text>().text = "Minutes cannot be less than 0.";
                Error.instance.errorPanel.gameObject.SetActive(true);
                Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.AddListener(SelectMinutesDuration);
            }
            else {
                durationSubmit.interactable = false;
                duration += minutes * 60;
            }
        }
       
        if (int.TryParse(simulationDurationSeconds.text, out seconds)) {
            if (seconds > 60) {
                Error.instance.errorPanel.GetComponentInChildren<Text>().text = "Seconds cannot exceed 60.";
                Error.instance.errorPanel.gameObject.SetActive(true);
                Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.AddListener(SelectSecondsDuration);
            }
            else if (seconds < 0) {
                Error.instance.errorPanel.GetComponentInChildren<Text>().text = "Seconds cannot be less than 0.";
                Error.instance.errorPanel.gameObject.SetActive(true);
                Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.AddListener(SelectMinutesDuration);
            }
            else if (minutes == 0 && seconds < 30) {
                Error.instance.errorPanel.GetComponentInChildren<Text>().text = "Simulation duration should exeed 30 seconds.";
                Error.instance.errorPanel.gameObject.SetActive(true);
                Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.AddListener(SelectMinutesDuration);
            }
            else {
                durationSubmit.interactable = false;
                duration += seconds;
            }
        }
    }

    private void SelectMinutesDuration() {
        Error.instance.errorPanel.SetActive(false);
        simulationDurationMinutes.Select();
        simulationDurationMinutes.ActivateInputField();
        Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
    }

    private void SelectSecondsDuration() {
        Error.instance.errorPanel.SetActive(false);
        simulationDurationSeconds.Select();
        simulationDurationSeconds.ActivateInputField();
        Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
    }

    public char VitalNameChangeValue(string input, int charIndex, char character) {
        if (vitalsChosen.transform.FindChild(input + character) != null) {
            vitalNameDuplicateWarning.SetActive(true);
        }
        else {
            vitalNameDuplicateWarning.SetActive(false);
        }
        return character;
    }

    void PopulatePresets() {
        UnityEngine.Object[] files = Resources.LoadAll("Conditions");
        presets.options.Add(new Dropdown.OptionData() { text = "None" });
        for (int i = 0; i < files.Length; i++) {
            presets.options.Add(new Dropdown.OptionData() { text = files[i].name });
        }
        presets.captionText.text = "Preset Conditions...";
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

    void PopulateDrugs() {
        drugs = ExportManager.instance.Load("drugs") as Drugs;
        foreach (Drug item in drugs.drugList) {
            GameObject toggleObject = Instantiate(togglePrefab);
            toggleObject.transform.SetParent(drugsChosen.transform);
            toggleObject.transform.localScale = Vector3.one;
            toggleObject.transform.localPosition = Vector3.zero;
            toggleObject.transform.GetChild(1).GetComponent<Text>().text = item.name;
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.name = item.name;

        }
    }

    void PopulateAdministrations() { }

    void PopulateEquipment() {
        equipment = ExportManager.instance.Load("equipment") as Equipment;
        foreach (Item item in equipment.itemList) {
            GameObject toggleObject = Instantiate(togglePrefab);
            toggleObject.transform.SetParent(equipmentChosen.transform);
            toggleObject.transform.localScale = Vector3.one;
            toggleObject.transform.localPosition = Vector3.zero;
            toggleObject.transform.GetChild(1).GetComponent<Text>().text = item.name;
            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.name = item.name;
            //  toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, toggleObject.transform.GetSiblingIndex()));
        }
    }

    void ClearPreviousTabs() {
        if (tabs.transform.childCount > 0) {
            Debug.Log(tabs.transform.childCount);
            for (int i = tabs.transform.childCount - 1; i >= 0; i--) {
                Transform tab = tabs.transform.GetChild(i);
                Debug.Log("Getting rid of tab " + tab.name);
                tab.gameObject.SetActive(false);
                tab.SetParent(inactiveTabs.transform);
            }
            for (int i = graphs.transform.childCount - 1; i > 1; i--) {
                print(graphs.transform.GetChild(i).gameObject.name);
                graphs.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    public void SetErrorPanelToFalse() {
        Error.instance.errorPanel.gameObject.SetActive(true);
    }

    public void LoadCondition(int index) {
        ClearPreviousTabs();
        if (presets.options[index].text != "None") {
            Debug.Log("Finding condition: " + presets.options[index].text);
            Condition condition = ExportManager.instance.Load("Conditions/" + presets.options[index].text) as Condition;
            if (condition == null) {
                Error.instance.errorPanel.GetComponentInChildren<Text>().text = "Condition could not be found.";
                Error.instance.errorPanel.gameObject.SetActive(true);
                Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.AddListener(SetErrorPanelToFalse);
            }
            else {
                List<Value> vitalData = condition.timeline[0].vitalValues;
                for (int i = 0; i < vitalData.Count; i++) {
                    Vital vital = vitals.vitalList[vitalData[i].vitalID];
                    graph = tabs.GenerateTab(vital.name);
                    graph.GenerateGraph(0, condition.timeline.Count - 1, "Duration", (int)Math.Ceiling(vital.min), (int)Math.Ceiling(vital.max), vital.units);
                    if (i != 0) {
                        graph.transform.gameObject.SetActive(false);
                    }
                }

                for (int i = 0; i < condition.timeline.Count; i++) {
                    Time time = condition.timeline[i];
                    foreach (Value data in time.vitalValues) {
                        Vital vital = vitals.vitalList[data.vitalID];
                        Graph graph = graphs.transform.FindChild(vital.name).GetComponent<Graph>();
                        graph.AddPoint(i, data.value);

                        if (data.upperThreshold != -1) {
                            graph.AddThresholdPointUpper(i, data.upperThreshold);
                        }

                        if (data.lowerThreshold != -1) {
                            graph.AddThresholdPointLower(i, data.lowerThreshold);
                        }
                    }

                    // if contains threshold values (add here)
                }
            }
        }
    }

    //int GetDuration() {
    //     duration = -1;
    //    if (simulationDurationMinutes.text != "") {
    //        duration = int.Parse(simulationDurationMinutes.text) * 60;
    //    }
    //    else {
    //        Error.instance.errorPanel.GetComponentInChildren<Text>().text = "Please set the duration of the simulation before selecting adding vitals.";
    //        Error.instance.errorPanel.gameObject.SetActive(true);
    //        Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.AddListener(SelectMinutesDuration);
    //    }
    //    if (simulationDurationSeconds.text != "") {
    //        duration += int.Parse(simulationDurationSeconds.text);
    //    }
    //    return duration;
    //}

    public void SelectMaxVitalValue() {
        Error.instance.errorPanel.SetActive(false);
        vitalMax.Select();
        vitalMax.ActivateInputField();
        Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
    }

    public void AddVital() {
        if (float.Parse(vitalMax.text) <= float.Parse(vitalMin.text)) {
            Error.instance.errorPanel.GetComponentInChildren<Text>().text = "Max value is less or equal to min value for vital";
            Error.instance.errorPanel.gameObject.SetActive(true);
            Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.AddListener(SelectMaxVitalValue);
            return;
        }
        Transform vitalTrans = graphs.transform.FindChild(vitalName.text);
        if (vitalTrans != null) {
            Destroy(vitalTrans);
        }
        int vitalIndex = -1;
        for (int i = 0; i < vitals.vitalList.Count; i++) {
            if (vitals.vitalList[i].name == vitalName.text) {
                vitalIndex = i;
            }
        }
        if (vitalIndex != -1) {
            vitals.vitalList.RemoveAt(vitalIndex);
            Destroy(vitalsChosen.transform.FindChild(vitalName.text).gameObject);
        }


        Vital vital = new Vital();
        vital.nodeID = vitals.vitalList.Count;
        vital.name = vitalName.text;
        vital.units = vitalUnit.text;
        vital.max = float.Parse(vitalMax.text);
        vital.min = float.Parse(vitalMin.text);
        newVitalPanel.SetActive(false);
        GameObject toggleObject = Instantiate(togglePrefab);
        toggleObject.transform.SetParent(vitalsChosen.transform);
        toggleObject.transform.SetAsFirstSibling();
        toggleObject.transform.localScale = Vector3.one;
        toggleObject.transform.localPosition = Vector3.zero;
        toggleObject.transform.GetChild(1).GetComponent<Text>().text = vital.name;
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.name = vital.name;
        toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, toggleObject.transform.GetSiblingIndex(), vital.name));
        vitals.vitalList.Insert(0, vital);
        loadChosenVital(true, 0, vital.name);
        // add checking here for is duplicate exists. If so then overwrite the existing vital
    }

    public void AddDrug() {
        Drug drug = new Drug();
        //drug.nodeID = ; //TODO assign ID based on Id's already present
        drug.name = drugName.text;
        drug.units = drugUnit.text;
        drug.max = float.Parse(drugMax.text);
        drug.min = float.Parse(drugMin.text);
        drug.duration = float.Parse(drugDuration.text);

        for (int i = 0; i < drugVitalsAffected.transform.childCount; i++) {
            drug.vitals.Add(drugVitalsAffected.transform.GetChild(i).name);
        }

        for (int i = 0; i < drugAdministrations.transform.childCount; i++) {
            drug.administrations.Add(drugAdministrations.transform.GetChild(i).name);
        }

        newDrugPanel.SetActive(false);

        GameObject toggleObject = Instantiate(togglePrefab);
        toggleObject.transform.SetParent(drugsChosen.transform);
        toggleObject.transform.localScale = Vector3.one;
        toggleObject.transform.localPosition = Vector3.zero;
        toggleObject.transform.GetChild(1).GetComponent<Text>().text = drug.name;
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.name = drug.name;
    }

    public void loadChosenVital(bool chosen, int index, string vitalName) {
        tabs.transform.GetComponent<ToggleGroup>().SetAllTogglesOff();
        if (chosen) {
            Transform vitalTrans = graphs.transform.FindChild(vitalName);
            if (vitalTrans == null || vitalName == "") {
                if (duration == -1) {
                    Error.instance.errorPanel.GetComponentInChildren<Text>().text = "Please set the duration of the simulation before selecting adding vitals.";
                    Error.instance.errorPanel.gameObject.SetActive(true);
                    Error.instance.errorPanel.GetComponentInChildren<Button>().onClick.AddListener(SelectMinutesDuration);
                    return;
                }
                graph = tabs.GenerateTab(vitals.vitalList[index].name);
                graph.GenerateGraph(0, duration, "Duration (Seconds)", (int)Math.Ceiling(vitals.vitalList[index].min), (int)Math.Ceiling(vitals.vitalList[index].max), vitals.vitalList[index].units);
                if (graph.sortedGraphPointsList.Count == 0) {
                    int halfValue = (int)Math.Ceiling(((vitals.vitalList[index].max - vitals.vitalList[index].min) / 2) + vitals.vitalList[index].min);
                    graph.AddPoint(0, halfValue);
                    graph.AddPoint(duration, halfValue);
                }
                graph.gameObject.SetActive(false);
                tabs.SwitchTab();
            }
            else {
                // Overwrite existing vital
                vitalTrans.gameObject.SetActive(true);
                Transform vitalTab = inactiveTabs.transform.FindChild(vitalName);
                if (vitalTab == null) {
                    // Error.instance.PrintError("Could not find inactiveTabs child : " + vitalName);
                    print("Could not find inactiveTabs child : " + vitalName);
                }
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

    public void NewVitalPanelToggleActive() {
        newVitalPanel.SetActive(!newVitalPanel.activeInHierarchy);
    }

    public void NewDrugPanelToggleActive() {
        newDrugPanel.SetActive(!newDrugPanel.activeInHierarchy);
    }
}
