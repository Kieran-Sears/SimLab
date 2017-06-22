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

    private bool replaceExistingGraphs;

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
        PopulatePresets();
        PopulateVitals();
        PopulateDrugs();
        PopulateEquipment();
        // dynamic check for user input for the duration of simulation
        //  simulationDurationMinutes.onValidateInput += delegate (string input, int charIndex, char addedChar) { return SimulationDurationChangeValue(input, charIndex, addedChar); };
        //  simulationDurationSeconds.onValidateInput += delegate (string input, int charIndex, char addedChar) { return SimulationDurationChangeValue(input, charIndex, addedChar); };
        simulationDurationMinutes.onValueChanged.AddListener(enableSubmitButton);
        simulationDurationSeconds.onValueChanged.AddListener(enableSubmitButton);
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
        int newDuration = 0;

        int minutes = 0;
        int seconds = 0;
        if (int.TryParse(simulationDurationMinutes.text, out minutes)) {
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

        if (int.TryParse(simulationDurationSeconds.text, out seconds)) {
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
            else if (minutes == 0 && seconds < 30) {
                Error.instance.informMessageText.text = "Simulation duration should exeed 30 seconds.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            }
            else {
                newDuration += seconds;
            }
        }

        if (duration != newDuration && duration != -1 && replaceExistingGraphs == false) {
            Error.instance.boolMessageText.text = "This action with overwrite existing graphs. Are you sure?";
            Error.instance.boolPanel.SetActive(true);
            Error.instance.boolYesButton.onClick.AddListener(ChangeActiveGraphDurations);
            Error.instance.boolNoButton.onClick.AddListener(ResetDurationBack);
        }
        else {
            duration = newDuration;
            durationSubmit.interactable = false;
            replaceExistingGraphs = false;
        }
    }

    //private void ChangeActiveGraphDurations() {
    //    replaceExistingGraphs = true;
    //    SubmitDuration();
    //    ClearPreviousTabs();
    //}

    private void SelectMinutesDuration() {
        Error.instance.informPanel.SetActive(false);
        simulationDurationMinutes.Select();
        simulationDurationMinutes.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    private void SelectSecondsDuration() {
        Error.instance.informPanel.SetActive(false);
        simulationDurationSeconds.Select();
        simulationDurationSeconds.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
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

    void ChangeActiveGraphDurations() {
        replaceExistingGraphs = true;
        SubmitDuration();
        for (int i = 0; i < vitalsChosen.transform.childCount; i++) {
            Toggle toggle = vitalsChosen.transform.GetChild(i).GetComponent<Toggle>();
            if (toggle.isOn) {
                string vitalName = toggle.gameObject.name;
                tabs.GetComponent<TabManager>().tabGraphs.Remove(toggle);
                tabs.GetComponent<ToggleGroup>().UnregisterToggle(toggle);
                Destroy(tabs.transform.FindChild(vitalName).gameObject);
                GameObject graphObject = graphs.transform.FindChild(vitalName).gameObject;
                Graph graph = graphs.transform.FindChild(vitalName).GetComponent<Graph>();

                SortedList<float, Slider> sortedGraphPointsList = graph.sortedGraphPointsList;
                SortedList<float, Slider> pointsUpperThreshold = graph.pointsUpperThreshold;
                SortedList<float, Slider> pointsLowerThreshold = graph.pointsLowerThreshold;
                int vitalMin = graph.yStart;
                int vitalMax = graph.yEnd;
                string vitalUnits = graph.yAxisLabel.GetComponent<Text>().text;

                Destroy(graphObject);

                graph = tabs.GenerateTab(vitalName);
                graph.GenerateGraph(0, duration, "Duration", vitalMin, vitalMax, vitalUnits);

                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = true;
                toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, i, vitalName));

                if (i != 0) {
                    graph.transform.gameObject.SetActive(false);
                }

                foreach (KeyValuePair<float, Slider> item in sortedGraphPointsList) {
                    if (item.Key < duration) {
                        graph.AddPoint(item.Key, item.Value.value);
                    }
                }
                foreach (KeyValuePair<float, Slider> item in pointsUpperThreshold) {
                    if (item.Key < duration) {
                        graph.AddThresholdPointUpper(item.Key, item.Value.value);
                    }
                }
                foreach (KeyValuePair<float, Slider> item in pointsLowerThreshold) {
                    if (item.Key < duration) {
                        graph.AddThresholdPointLower(item.Key, item.Value.value);
                    }
                }
                if (!graph.pointsLowerThreshold.ContainsKey(duration)) {
                    graph.AddThresholdPointLower(duration, graph.pointsLowerThreshold[0].value);
                }
                if (!graph.pointsUpperThreshold.ContainsKey(duration)) {
                    graph.AddThresholdPointUpper(duration, graph.pointsUpperThreshold[0].value);
                }
            }
        }
    }

    public void SetInformPanelToFalse() {
        Error.instance.informPanel.SetActive(false);
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    public void ResetDurationBack() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolYesButton.onClick.RemoveAllListeners();
        Error.instance.boolNoButton.onClick.RemoveAllListeners();
        simulationDurationMinutes.text = (duration / 60).ToString();
        simulationDurationSeconds.text = (duration % 60).ToString();
    }

    public void LoadCondition(int index) {
        if (duration != -1) {

            Debug.Break();
            SubmitDuration();
            ChangeActiveGraphDurations();
        }
        print(duration);
        if (presets.options[index].text != "None") {
            Debug.Log("Finding condition: " + presets.options[index].text);
            Condition condition = ExportManager.instance.Load("Conditions/" + presets.options[index].text) as Condition;
            if (condition == null) {
                Error.instance.informMessageText.text = "Condition could not be found.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SetInformPanelToFalse);
            }
            else {
                List<Value> vitalData = condition.timeline[0].vitalValues;
                for (int i = 0; i < vitalData.Count; i++) {
                    Vital vital = vitals.vitalList[vitalData[i].vitalID];
                    simulationDurationMinutes.text = ((condition.timeline.Count - 1) / 60).ToString();
                    simulationDurationSeconds.text = ((condition.timeline.Count - 1) % 60).ToString();
                    duration = condition.timeline.Count - 1;
                    graph = tabs.GenerateTab(vital.name);
                    graph.GenerateGraph(0, condition.timeline.Count - 1, "Duration", (int)Math.Ceiling(vital.min), (int)Math.Ceiling(vital.max), vital.units);
                    Transform vitalChosen = vitalsChosen.transform.FindChild(vital.name);
                    Toggle toggle = vitalChosen.GetComponent<Toggle>();
                    toggle.onValueChanged.RemoveAllListeners();
                    toggle.isOn = true;
                    toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, vitalChosen.transform.GetSiblingIndex(), vital.name));
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
                }
            }
        }
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
                    Error.instance.informMessageText.text = "Please set the duration of the simulation before adding vitals.";
                    Error.instance.informPanel.SetActive(true);
                    Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
                    vitalsChosen.transform.GetChild(index).GetComponent<Toggle>().isOn = false;
                    return;
                }
                graph = tabs.GenerateTab(vitals.vitalList[index].name);
                graph.GenerateGraph(0, duration, "Duration (Seconds)", (int)Math.Ceiling(vitals.vitalList[index].min), (int)Math.Ceiling(vitals.vitalList[index].max), vitals.vitalList[index].units);
                if (graph.sortedGraphPointsList.Count == 0) {
                    int halfValue = (int)Math.Ceiling(((vitals.vitalList[index].max - vitals.vitalList[index].min) / 2) + vitals.vitalList[index].min);
                    graph.AddPoint(0, halfValue);
                    graph.AddPoint(duration, halfValue);
                }
                if (graph.pointsUpperThreshold.Count == 0) {
                    int quaterValue = (int)Math.Ceiling(((vitals.vitalList[index].max - vitals.vitalList[index].min) / 4 ) + vitals.vitalList[index].min);
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
                // Overwrite existing vital
                vitalTrans.gameObject.SetActive(true);
                Transform vitalTab = inactiveTabs.transform.FindChild(vitalName);
               // if (vitalTab == null) {
                    // Error.instance.PrintError("Could not find inactiveTabs child : " + vitalName);
                   // print("Could not find inactiveTabs child : " + vitalName);
                //}
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
