using System;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionSetup : MonoBehaviour {

    public static ConditionSetup Instance { get; private set; }

    public TabManager tabManager;
    public GameObject togglePrefab;
    public GameObject conditionWindow;
    public GameObject drugWindow1;
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

    #region Panels
    public GameObject newVitalPanel;
    public GameObject newDrugPanel;
    #endregion

    public Drugs drugs;
    public Vitals vitals;
    public Graph graph;

    private bool replaceExistingGraphs;
    private bool changeDuration;
    private int previousConditionChosen;
    private enum ConditionState { notAsked, current, condition, cancelled };
    private ConditionState state;

    private void Awake() {
        if (Instance) {
            DestroyImmediate(this);
        } else {
            Instance = this;
        }
    }

    void Start() {
        replaceExistingGraphs = false;
        PopulatePresets();
        PopulateVitals();
        PopulateDrugs();
        simulationDurationMinutes.onValueChanged.AddListener(EnableSubmitButton);
        simulationDurationSeconds.onValueChanged.AddListener(EnableSubmitButton);
    }

    public void EnableSubmitButton(string input) {
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
            } else if (minutes < 0) {
                Error.instance.informMessageText.text = "Minutes cannot be less than 0.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            } else {
                newDuration += minutes * 60;
            }
        }

        if (int.TryParse(simulationDurationSeconds.text, out seconds)) {
            if (seconds > 60) {
                Error.instance.informMessageText.text = "Seconds cannot exceed 60.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectSecondsDuration);
            } else if (seconds < 0) {
                Error.instance.informMessageText.GetComponentInChildren<Text>().text = "Seconds cannot be less than 0.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            } else if (minutes == 0 && seconds < 30) {
                Error.instance.informMessageText.text = "Simulation duration should exeed 30 seconds.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
            } else {
                newDuration += seconds;
            }
        }

        if (duration != newDuration && duration != -1 && replaceExistingGraphs == false) {
            Error.instance.boolMessageText.text = "This action may overwrite data on already active graphs. Are you sure?";
            Error.instance.boolPanel.SetActive(true);
            Error.instance.boolRightButton.onClick.AddListener(ChangeActiveGraphDurations);
            Error.instance.boolLeftButton.onClick.AddListener(ResetDurationBack);
        } else {
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

    void PopulatePresets() {
        UnityEngine.Object[] files = Resources.LoadAll("Conditions");
        presets.options.Add(new Dropdown.OptionData() { text = "None" });
        for (int i = 0; i < files.Length; i++) {
            presets.options.Add(new Dropdown.OptionData() { text = files[i].name });
        }
        presets.captionText.text = "Preset Conditions...";
        presets.onValueChanged.AddListener(LoadCondition);
    }

    #region developers note
    /*
     the "populate" functions below are used for adding vitals from disk to the selection on the condition setup side panel
     a "Selectable Toggle" prefab is set up to use for the functionality of choosing a vital
     to edit, same applying to drugs, however, further code for changing any active graphs
     of the edited vital / drug will be needed, so that upon applying the changes in edit mode the corrosponding
     graphs are updated. - this may prove challenging.
    */
    #endregion

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
            toggle.onValueChanged.AddListener((bool value) => LoadChosenVital(value, toggleObject.transform.GetSiblingIndex(), vital.name));
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

    void ChangeActiveGraphDurations() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolDropdown.gameObject.SetActive(false);
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
        Error.instance.boolLeftButton.GetComponentInChildren<Text>().text = "no";

        replaceExistingGraphs = true;
        
        if (!changeDuration) {
            SubmitDuration();
        }

        for (int i = 0; i < vitalsChosen.transform.childCount; i++) {

            Toggle toggle = vitalsChosen.transform.GetChild(i).GetComponent<Toggle>();

            if (toggle.isOn) {

                string vitalName = toggle.gameObject.name;

                tabManager.GetComponent<TabManager>().tabGraphs.Remove(toggle);
                tabManager.activeTabs.GetComponent<ToggleGroup>().UnregisterToggle(toggle);

                Destroy(tabManager.activeTabs.transform.FindChild(vitalName).gameObject);

                GameObject graphObject = tabManager.contentArea.transform.FindChild(vitalName).gameObject;
                Graph graph = tabManager.contentArea.transform.FindChild(vitalName).GetComponent<Graph>();

                SortedList<float, Slider> sortedGraphPointsList = graph.sortedGraphPointsList;
                SortedList<float, Slider> pointsUpperThreshold = graph.pointsUpperThreshold;
                SortedList<float, Slider> pointsLowerThreshold = graph.pointsLowerThreshold;

                int vitalMin = graph.yStart;
                int vitalMax = graph.yEnd;

                string vitalUnits = graph.yAxisLabel.GetComponent<Text>().text;

                graph = tabManager.GenerateTab(vitalName).GetComponent<Graph>();
                graph.GenerateGraph(0, duration, vitalMin, vitalMax, vitalUnits);

                toggle.onValueChanged.RemoveAllListeners();
                toggle.isOn = true;
                toggle.onValueChanged.AddListener((bool value) => LoadChosenVital(value, i, vitalName));

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
        state = ConditionState.cancelled;
        LoadCondition(presets.value);
    }

    public void ClearBoolError() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolDropdown.gameObject.SetActive(false);
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
        presets.onValueChanged.RemoveAllListeners();
        presets.value = previousConditionChosen;
        presets.onValueChanged.AddListener(LoadCondition);
    }

    public void ResetCondition() {
        duration = -1;
        simulationDurationMinutes.text = "Mins";
        simulationDurationSeconds.text = "Secs";
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolMessageText.text = "";
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        Error.instance.boolRightButton.onClick.RemoveAllListeners();

        if (tabManager.activeTabs.transform.childCount > 0) {
            for (int i = tabManager.activeTabs.transform.childCount - 1; i >= 0; i--) {
                Transform tab = tabManager.activeTabs.transform.GetChild(i);
                tab.gameObject.SetActive(false);
                tab.SetParent(tabManager.inactiveTabs.transform);
            }
            for (int i = tabManager.contentArea.transform.childCount - 1; i >= 0; i--) {
                tabManager.contentArea.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        for (int i = 0; i < vitalsChosen.transform.childCount; i++) {
            vitalsChosen.transform.GetChild(i).GetComponent<Toggle>().isOn = false;
        }
    }

    public void DurationOverwrite() {
        if (Error.instance.boolDropdown.value == 0) {
            state = ConditionState.condition;
            LoadCondition(presets.value);
       
        }
        if (Error.instance.boolDropdown.value == 1) { 
            state = ConditionState.current;
            LoadCondition(presets.value);
        }
    }

    public void LoadCondition(int index) {

        // if "none" clear any graphs to do with the current condition
        // if condition has been chosen
            // if there is no duration set load the condition with no questions asked
            // if there is a duration already
                // if user chooses to keep current duration
                    // change all the loading condition graphs to match the current duration
                // if user chooses the conditions duration
                    // change all the current graphs to match the condition's
                // if user presses cancel just close the prompt window and dont change anything
        
         if (presets.options[index].text == "None") {
            Error.instance.boolPanel.SetActive(true);
            Error.instance.boolMessageText.text = "This will erase any existing data. Are you sure?";
            Error.instance.boolLeftButton.onClick.AddListener(ClearBoolError);
            Error.instance.boolRightButton.onClick.AddListener(ResetCondition);
         } else {
            if (duration == -1) {
                print("initialising preset condition");
                condition = ExportManager.instance.Load("Conditions/" + presets.options[index].text) as Condition;
                previousConditionChosen = index;
                duration = condition.duration;
                simulationDurationMinutes.text = (condition.duration / 60).ToString();
                simulationDurationSeconds.text = (condition.duration % 60).ToString();
                durationSubmit.interactable = false;
                bool firstInList = true;
                foreach (VitalData vitalData in condition.vitalsData) {
                    if (!tabManager.transform.FindChild(vitalData.vital.name)) {
                        Vital vital = vitalData.vital;

                        graph = tabManager.GenerateTab(vital.name).GetComponent<Graph>();
                        graph.GenerateGraph(0, duration, (int)Math.Ceiling(vital.min), (int)Math.Ceiling(vital.max), vital.units);

                        // TODO prompt user to see if they want vitals in the preset they dont have?

                        // Add the listener to the vitals list toggles so graphs can be selected / deselected at will
                        Transform vitalChosen = vitalsChosen.transform.FindChild(vital.name);
                        Toggle toggle = vitalChosen.GetComponent<Toggle>();
                        toggle.onValueChanged.RemoveAllListeners();
                        toggle.isOn = true;
                        toggle.onValueChanged.AddListener((bool value) => LoadChosenVital(value, vitalChosen.transform.GetSiblingIndex(), vital.name));

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
                        } else {
                            graph.transform.gameObject.SetActive(false);
                        }
                    }
                }
            } else {
                print("Duration already set");
                switch (state) {
                    case ConditionState.notAsked:
                        print("prompting user for duration to keep");
                        Error.instance.boolPanel.SetActive(true);
                        Error.instance.boolDropdown.gameObject.SetActive(true);
                        Error.instance.boolMessageText.text = "The duration of the preset " + condition.name + " is not equal to the existing duration. Which duration would you prefer to keep?";
                        Error.instance.boolLeftButton.onClick.AddListener(CancelAddingPrefab);
                        Error.instance.boolRightButton.onClick.AddListener(DurationOverwrite);
                        Error.instance.boolDropdown.ClearOptions();
                        Error.instance.boolDropdown.captionText.text = "Select...";
                        Error.instance.boolLeftButton.GetComponentInChildren<Text>().text = "Cancel";
                        List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();
                        Dropdown.OptionData data = new Dropdown.OptionData();
                        data.text = condition.name + "Preset (" + (condition.duration / 60).ToString() + ":" + (condition.duration % 60).ToString() + ")";
                        options.Add(data);
                        data = new Dropdown.OptionData();
                        data.text = "Current (" + simulationDurationMinutes.text + ":" + simulationDurationSeconds.text + ")";
                        options.Add(data);
                        Error.instance.boolDropdown.AddOptions(options);
                        return;
                    case ConditionState.current:
                        print("keeping current duration");
                        replaceExistingGraphs = true;
                        LoadCondition(presets.value);
                        duration = (int.Parse(simulationDurationMinutes.text) * 60) + int.Parse(simulationDurationSeconds.text);
                        ChangeActiveGraphDurations();
                        state = ConditionState.notAsked;
                        break;
                    case ConditionState.condition:
                        print("changing to preset conditions duration");
                        simulationDurationMinutes.text = (condition.duration / 60).ToString();
                        simulationDurationSeconds.text = (condition.duration % 60).ToString();
                        duration = condition.duration;
                        replaceExistingGraphs = false;
                        changeDuration = true;
                        ChangeActiveGraphDurations();
                        changeDuration = false;
                        LoadCondition(presets.value);
                        state = ConditionState.notAsked;
                        break;
                    case ConditionState.cancelled:
                        print("user cancelled action to load preset condition");
                        ClearBoolError();
                        state = ConditionState.notAsked;
                        break;
                    default:
                        break;
                }
            }
         }
    }

    public void LoadChosenVital(bool chosen, int index, string vitalName) {
        // clear the background area ready for display
        tabManager.activeTabs.transform.GetComponent<ToggleGroup>().SetAllTogglesOff();
        if (chosen) {
            // check to see if vital selected has already been previously created and is idle
            Transform vitalTrans = tabManager.contentArea.transform.FindChild(vitalName);
            Transform vitalTab = tabManager.inactiveTabs.transform.FindChild(vitalName);

            // if vital isn't sitting idle
            if (vitalTrans == null || vitalTab == null) {
                // check this isn't the first attempt of choosing a vital before a duration has been even set
                if (duration == -1) {
                    // if it is then warn user to set a duration first before choosing a vital
                    Error.instance.informMessageText.text = "Please set the duration of the simulation before adding vitals.";
                    Error.instance.informPanel.SetActive(true);
                    Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
                    vitalsChosen.transform.GetChild(index).GetComponent<Toggle>().isOn = false;
                    return;
                }
                // if it is the first attempt of choosing a vital and a duration has been set then initialise the vital graph with starting values
                graph = tabManager.GenerateTab(vitals.vitalList[index].name).GetComponent<Graph>();
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
                tabManager.SwitchTab();
            } else {
                // if vital was on standby then reactivate it as its toggle has been selected
                vitalTrans.gameObject.SetActive(true);
                vitalTab.SetParent(tabManager.activeTabs.transform);
                vitalTab.gameObject.SetActive(true);
                tabManager.SwitchTab();
                vitalTab.GetComponent<Toggle>().isOn = true;
            }
        } else {
            // if the user has unticked the vitals toggle then hide the vital's graph display away along with its tab
            Transform tab = tabManager.activeTabs.transform.FindChild(vitalName);
            if (tab != null) {
                tab.gameObject.SetActive(false);
                tab.SetParent(tabManager.inactiveTabs.transform);
                // always set the vital now being looked at as the first one in the list
                if (tabManager.activeTabs.transform.childCount > 0) {
                    tabManager.activeTabs.transform.GetChild(0).GetComponent<Toggle>().isOn = true;
                }
            }
        }
    }

    public void ToggleActiveVitalWindow(bool clear = true) {
        if (clear) {
            newVitalPanel.GetComponent<VitalSetup>().ClearAttributes();
        }
        bool selected = !newVitalPanel.activeInHierarchy;
        drugWindow1.SetActive(selected);
        newVitalPanel.SetActive(selected);
        conditionWindow.SetActive(!selected);
    }

    public void ToggleActiveDrugWindow(bool clear = true) {
        if (clear) {
            newDrugPanel.GetComponent<DrugSetup>().ClearAttributes();
        }

        bool selected = !newDrugPanel.activeInHierarchy;
        if (selected) {
            drugWindow1.SetActive(selected);
            newDrugPanel.SetActive(selected);
            conditionWindow.SetActive(!selected);
        } else {
            drugWindow1.SetActive(selected);
            newDrugPanel.SetActive(selected);
            conditionWindow.SetActive(!selected);
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