using System;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimulationSetup : MonoBehaviour {

    public static SimulationSetup instance { get; private set; }

    public TabManager tabManager;
    public GameObject togglePrefab;
    public GameObject rightLerpPanel;
    public Condition condition;

    #region Condition
    public Dropdown presets;
    public InputField simulationDurationMinutes;
    public InputField simulationDurationSeconds;
    public Button durationSubmit;
    public Button newVitalButton;
    public Button newDrugButton;
    public int duration = -1;
    public GameObject vitalsChosen;
    public GameObject drugsChosen;
    public GameObject equipmentChosen;
    #endregion

    #region Vital
    public GameObject newVitalPanel;
    //public InputField vitalName;
    //public InputField vitalUnit;
    //public InputField vitalMax;
    //public InputField vitalMin;
    //public GameObject vitalNameDuplicateWarning;
    #endregion

    #region Drug
    public GameObject newDrugPanel;
    //public InputField drugName;
    //public InputField drugUnit;
    //public InputField drugMax;
    //public InputField drugMin;
    //public InputField drugDuration;
    //public GameObject drugVitalsAffected;
    //public GameObject drugAdministrations;
    #endregion

    public Drugs drugs;
    public Vitals vitals;
    public Equipment equipment;
    public Graph graph;

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
        PopulatePresets();
        PopulateVitals();
        PopulateDrugs();
        PopulateEquipment();
        simulationDurationMinutes.onValueChanged.AddListener(enableSubmitButton);
        simulationDurationSeconds.onValueChanged.AddListener(enableSubmitButton);
        // vitalName.onValidateInput += delegate (string input, int charIndex, char addedChar) { return VitalNameChangeValue(input, charIndex, addedChar); };
    }

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
            Error.instance.boolMessageText.text = "This action may overwrite data on already active graphs. Are you sure?";
            Error.instance.boolPanel.SetActive(true);
            Error.instance.boolRightButton.onClick.AddListener(ChangeActiveGraphDurations);
            Error.instance.boolLeftButton.onClick.AddListener(ResetDurationBack);
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

    //public char VitalNameChangeValue(string input, int charIndex, char character) {
    //    if (vitalsChosen.transform.FindChild(input + character) != null) {
    //        vitalNameDuplicateWarning.SetActive(true);
    //    }
    //    else {
    //        vitalNameDuplicateWarning.SetActive(false);
    //    }
    //    return character;
    //}

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
        foreach (Drug item in drugs.drugs) {
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
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolDropdown.gameObject.SetActive(false);
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
        Error.instance.boolLeftButton.GetComponentInChildren<Text>().text = "no";

        replaceExistingGraphs = true;

        print("someBool "  + someBool);
        if (!someBool) {
            SubmitDuration();
        }

        for (int i = 0; i < vitalsChosen.transform.childCount; i++) {

            Toggle toggle = vitalsChosen.transform.GetChild(i).GetComponent<Toggle>();

            if (toggle.isOn) {

                string vitalName = toggle.gameObject.name;

                tabManager.GetComponent<TabManager>().tabGraphs.Remove(toggle);
                tabManager.activeTabs.GetComponent<ToggleGroup>().UnregisterToggle(toggle); // TODO Activetabs 

                Destroy(tabManager.activeTabs.transform.FindChild(vitalName).gameObject);

                GameObject graphObject = tabManager.contentArea.transform.FindChild(vitalName).gameObject;
                Graph graph = tabManager.contentArea.transform.FindChild(vitalName).GetComponent<Graph>();

                SortedList<float, Slider> sortedGraphPointsList = graph.sortedGraphPointsList;
                SortedList<float, Slider> pointsUpperThreshold = graph.pointsUpperThreshold;
                SortedList<float, Slider> pointsLowerThreshold = graph.pointsLowerThreshold;

                int vitalMin = graph.yStart;
                int vitalMax = graph.yEnd;

                string vitalUnits = graph.yAxisLabel.GetComponent<Text>().text;

                // Destroy(graphObject);

                graph = tabManager.GenerateTab(vitalName).GetComponent<Graph>();
                graph.GenerateGraph(0, duration, vitalMin, vitalMax, vitalUnits);

                // place here listener for graph right click menu

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
                graph.AddPoint(duration, ((vitalMax - vitalMin) / 2));

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
                graph.DrawLinkedPointLines();
            }
        }
        tabManager.SwitchTab();
    }

    public void SetInformPanelToFalse() {
        Error.instance.informPanel.SetActive(false);
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    public void ResetDurationBack() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        simulationDurationMinutes.text = (duration / 60).ToString();
        simulationDurationSeconds.text = (duration % 60).ToString();
    }

    public void CancelAddingPrefab() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolDropdown.gameObject.SetActive(false);
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
    }

    public void OverwriteDurationWithPreset() {
        replaceExistingGraphs = true;
        string choice = Error.instance.boolDropdown.options[Error.instance.boolDropdown.value].text;
        print(choice);

        if (choice == ("Current (" + simulationDurationMinutes.text + ":" + simulationDurationSeconds.text + ")")) {
            replaceExistingGraphs = true;

            // put here code to make duration = to the conditions duration
            LoadCondition(presets.value);

            duration = (int.Parse(simulationDurationMinutes.text) * 60) + int.Parse(simulationDurationSeconds.text);
            ChangeActiveGraphDurations();
            print("IF STRING 1");
        }
        else {
            simulationDurationMinutes.text = (condition.duration / 60).ToString();
            simulationDurationSeconds.text = (condition.duration % 60).ToString();
            duration = condition.duration;
            replaceExistingGraphs = false;
            someBool = true;
            ChangeActiveGraphDurations();
            someBool = false;
            LoadCondition(presets.value);
            //LoadCondition(presets.value);
            print("IF STRING 2");
        }
    }

    public void LoadCondition(int index) {
        print("Duration " + duration);
        if (presets.options[index].text != "None") {
            Debug.Log("Finding condition: " + presets.options[index].text);
            condition = ExportManager.instance.Load("Conditions/" + presets.options[index].text) as Condition;
            if (condition == null) {
                Error.instance.informMessageText.text = "Condition could not be found.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SetInformPanelToFalse);
                return;
            }

            // if a duration already exists prompt the user on how they want to overwrite the duration with the preset with different duration (warning: recursive function call)
            if (duration != -1 && replaceExistingGraphs == false) {

                Error.instance.boolPanel.SetActive(true);
                Error.instance.boolDropdown.gameObject.SetActive(true);
                Error.instance.boolMessageText.text = "The duration of the preset " + condition.name + " is not equal to the existing duration. Which duration would you prefer to keep?";
                Error.instance.boolLeftButton.onClick.AddListener(CancelAddingPrefab);
                Error.instance.boolRightButton.onClick.AddListener(OverwriteDurationWithPreset);
                Error.instance.boolDropdown.ClearOptions();
                Error.instance.boolDropdown.captionText.text = "Select...";
                Error.instance.boolLeftButton.GetComponentInChildren<Text>().text = "Cancel";
                List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                Dropdown.OptionData data = new Dropdown.OptionData();

                data.text = condition.name + " (" + (condition.duration / 60).ToString() + ":" + (condition.duration % 60).ToString() + ")";


                options.Add(data);
                data = new Dropdown.OptionData();
                data.text = "Current (" + simulationDurationMinutes.text + ":" + simulationDurationSeconds.text + ")";
                options.Add(data);
                Error.instance.boolDropdown.AddOptions(options);
                return;
            }
            else {

                simulationDurationMinutes.text = (condition.duration / 60).ToString();
                simulationDurationSeconds.text = (condition.duration % 60).ToString();
                duration = condition.duration;
                replaceExistingGraphs = false;
            }
        }


        // for each vital in the condition, loop through and create the graph for it

        bool firstInList = true;
        foreach (VitalData vitalData in condition.vitalsData) {
            if (!tabManager.transform.FindChild(vitalData.vital.name)) {
                Vital vital = vitalData.vital;

                graph = tabManager.GenerateTab(vital.name).GetComponent<Graph>();
                graph.GenerateGraph(0, duration, (int)Math.Ceiling(vital.min), (int)Math.Ceiling(vital.max), vital.units);

                // TODO place here listener for graph right click menu

                // TODO prompt user to see if they want vitals in the preset they dont have

                // Add the listener to the vitals list toggles so graphs can be selected / deselected at will
                Transform vitalChosen = vitalsChosen.transform.FindChild(vital.name);
                Toggle toggle = vitalChosen.GetComponent<Toggle>();
                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = true;
                toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, vitalChosen.transform.GetSiblingIndex(), vital.name));

                // populate the graph with its values
                foreach (Value value in vitalData.timeline.vitalValues) {
                    graph.AddPoint(value.second, value.value);
                }
                foreach (Value value in vitalData.timeline.upperThresholdValues) {
                    graph.AddThresholdPointUpper(value.second, value.value);
                }
                foreach (Value value in vitalData.timeline.lowerThresholdValues) {
                    graph.AddThresholdPointLower(value.second, value.value);
                }

                // ensure that the first vital added automatically appears for the user to view
                if (firstInList) {
                    graph.transform.gameObject.SetActive(true);
                    firstInList = false;
                }
                else {
                    graph.transform.gameObject.SetActive(false);
                }
            }
        }
    }

    //public void SelectMaxVitalValue() {
    //    Error.instance.informPanel.SetActive(false);
    //    vitalMax.Select();
    //    vitalMax.ActivateInputField();
    //    Error.instance.informOkButton.onClick.RemoveAllListeners();
    //}

    //public void AddVital() {

    //    if (float.Parse(vitalMax.text) <= float.Parse(vitalMin.text)) {
    //        Error.instance.informMessageText.text = "Max value is less or equal to min value for vital";
    //        Error.instance.informPanel.SetActive(true);
    //        Error.instance.informOkButton.onClick.AddListener(SelectMaxVitalValue);
    //        return;
    //    }

    //    Transform vitalTrans = tabManager.transform.FindChild(vitalName.text);

    //    if (vitalTrans != null) {
    //        Destroy(vitalTrans.gameObject);
    //    }

    //    int vitalIndex = -1;

    //    for (int i = 0; i < vitals.vitalList.Count; i++) {
    //        if (vitals.vitalList[i].name == vitalName.text) {
    //            vitalIndex = i;
    //        }
    //    }

    //    if (vitalIndex != -1) {
    //        vitals.vitalList.RemoveAt(vitalIndex);
    //        Destroy(vitalsChosen.transform.FindChild(vitalName.text).gameObject);
    //    }


    //    Vital vital = new Vital();
    //    vital.nodeID = vitals.vitalList.Count;
    //    vital.name = vitalName.text;
    //    vital.units = vitalUnit.text;
    //    vital.max = float.Parse(vitalMax.text);
    //    vital.min = float.Parse(vitalMin.text);

    //    newVitalPanel.SetActive(false);

    //    GameObject toggleObject = Instantiate(togglePrefab);
    //    toggleObject.transform.SetParent(vitalsChosen.transform);
    //    toggleObject.transform.SetAsFirstSibling();
    //    toggleObject.transform.localScale = Vector3.one;
    //    toggleObject.transform.localPosition = Vector3.zero;
    //    toggleObject.transform.GetChild(1).GetComponent<Text>().text = vital.name;

    //    Toggle toggle = toggleObject.GetComponent<Toggle>();
    //    toggle.name = vital.name;
    //    toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, toggleObject.transform.GetSiblingIndex(), vital.name));

    //    vitals.vitalList.Insert(0, vital);

    //    loadChosenVital(true, 0, vital.name);

    //    // add checking here for is duplicate exists. If so then overwrite the existing vital
    //}

    public void loadChosenVital(bool chosen, int index, string vitalName) {
        tabManager.activeTabs.transform.GetComponent<ToggleGroup>().SetAllTogglesOff();
        if (chosen) {

            Transform vitalTrans = tabManager.contentArea.transform.FindChild(vitalName);
            Transform vitalTab = tabManager.activeTabs.transform.FindChild(vitalName);


            if (vitalTrans == null || vitalTab == null) {
                if (duration == -1) {
                    Error.instance.informMessageText.text = "Please set the duration of the simulation before adding vitals.";
                    Error.instance.informPanel.SetActive(true);
                    Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
                    vitalsChosen.transform.GetChild(index).GetComponent<Toggle>().isOn = false;
                    return;
                }
                graph = tabManager.GenerateTab(vitals.vitalList[index].name).GetComponent<Graph>();
                graph.GenerateGraph(0, duration, (int)Math.Ceiling(vitals.vitalList[index].min), (int)Math.Ceiling(vitals.vitalList[index].max), vitals.vitalList[index].units);

                // TODO place here listener for graph right click menu

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
                tabManager.SwitchTab();
            }
            else {
                vitalTrans.gameObject.SetActive(true);
                vitalTab.SetParent(tabManager.activeTabs.transform);
                vitalTab.gameObject.SetActive(true);

                tabManager.SwitchTab();
            }
        }
        else {
            Transform tab = tabManager.activeTabs.transform.FindChild(vitalName);
            if (tab != null) {
                tab.gameObject.SetActive(false);
                tab.SetParent(tabManager.inactiveTabs.transform);
                if (tabManager.activeTabs.transform.childCount > 0) {
                    tabManager.activeTabs.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
                }
                // tabManager.SwitchTab();
                //   tabManager.transform.FindChild(vitalName).gameObject.SetActive(false);
            }
        }
    }

    public void NewVitalPanelToggleActive() {
        bool selected = !newVitalPanel.activeInHierarchy;
        rightLerpPanel.SetActive(selected);
        newVitalPanel.SetActive(selected);
        newDrugButton.interactable = !newDrugButton.interactable;
        if (selected) {
            newVitalButton.GetComponent<Image>().color = newVitalButton.colors.pressedColor;
        }
        else {
            newVitalButton.GetComponent<Image>().color = newVitalButton.colors.normalColor;
        }
    }

    public void NewDrugPanelToggleActive() {
        bool selected = !newDrugPanel.activeInHierarchy;
        newVitalButton.interactable = !newVitalButton.interactable;
        if (selected) {
            newDrugButton.GetComponent<Image>().color = newVitalButton.colors.pressedColor;
            rightLerpPanel.SetActive(selected);
            newDrugPanel.SetActive(selected);
        }
        else {
            newDrugButton.GetComponent<Image>().color = newVitalButton.colors.normalColor;
            rightLerpPanel.SetActive(selected);
            newDrugPanel.SetActive(selected);

        }
        
    }

    public Vital GetVital(string vitalName) {
        foreach (Vital vital in vitals.vitalList) {
            if (vital.name == vitalName) {
                return vital;
            }
        }
        return null;
    }
}