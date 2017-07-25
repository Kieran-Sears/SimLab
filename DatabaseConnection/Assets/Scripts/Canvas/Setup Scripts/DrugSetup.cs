using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrugSetup : MonoBehaviour {

    public static DrugSetup instance { get; private set; }

    public TabManager tabManager;
    public GameObject togglePrefab;
    public InputField drugName;
    public GameObject administrations;

    public InputField dose;
    public InputField minimum;
    public InputField maximum;
    public InputField minutes;
    public InputField seconds;
    public GameObject vitals;
    public Button visualisation;
    public Button submit;
    public Graph graph;

    private int duration = -1;
    private bool replaceExistingGraphs;

    private void Awake() {
        if (instance) {
            DestroyImmediate(this);
        } else {
            instance = this;
        }
    }

    public void Start() {
        PopulateAdministrations();
        PopulateVitals();
        for (int i = 0; i < vitals.transform.childCount; i++) {
            vitals.transform.GetChild(i).gameObject.SetActive(false);
        }
        submit.interactable = false;
        visualisation.interactable = false;
        dose.interactable = false;
        maximum.interactable = false;
        minimum.interactable = false;
        minutes.interactable = false;
        seconds.interactable = false;
    }

    //public void ResetValues() {

    //    for (int i = 0; i < vitals.transform.childCount; i++) {
    //        Destroy(vitals.transform.GetChild(i).gameObject);
    //    }
    //}

    public void ToggleElementsActive() {
        submit.interactable = !submit.interactable;
        dose.interactable = !dose.interactable;
        minimum.interactable = !minimum.interactable;
        maximum.interactable = !maximum.interactable;
        minutes.interactable = !minutes.interactable;
        seconds.interactable = !seconds.interactable;
    }

    void PopulateAdministrations() {
        Drugs drugs = ExportManager.instance.Load("drugs") as Drugs;
        foreach (Drug drug in drugs.drugs) {
            print("Exploring drug " + drug.name + " for administrations");
            foreach (Administration admin in drug.administrations) {
                print("found " + admin.name);
                if (administrations.transform.FindChild(admin.name) == null) {
                    print("no duplicate found so adding to administrationsChosen");
                    GameObject toggleObject = Instantiate(togglePrefab);
                    toggleObject.transform.SetParent(administrations.transform);
                    toggleObject.transform.localScale = Vector3.one;
                    toggleObject.transform.localPosition = Vector3.zero;
                    toggleObject.transform.GetChild(1).GetComponent<Text>().text = admin.name;
                    Toggle toggle = toggleObject.GetComponent<Toggle>();
                    toggle.name = admin.name;
                    toggle.isOn = false;
                    toggle.onValueChanged.AddListener((bool value) => LoadChosenAdministration(value, toggleObject.transform.GetSiblingIndex(), admin.name));
                }
            }
        }
    }

    public void SubmitDuration() {
        print("setting duration");
        int newDuration = 0;

        int minutes = 0;
        int seconds = 0;
        if (int.TryParse(this.minutes.text, out minutes)) {
            print("mins registered");
            if (minutes > 60) {
                Error.instance.informMessageText.text = "Minutes cannot exceed 60.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(this.SelectMinutesDuration);
            } else if (minutes < 0) {
                Error.instance.informMessageText.text = "Minutes cannot be less than 0.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(this.SelectMinutesDuration);
            } else {
                newDuration += minutes * 60;
            }
        }

        if (int.TryParse(this.seconds.text, out seconds)) {
            print("secs registered");

            if (seconds > 60) {
                Error.instance.informMessageText.text = "Seconds cannot exceed 60.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(this.SelectSecondsDuration);
            } else if (seconds < 0) {
                Error.instance.informMessageText.GetComponentInChildren<Text>().text = "Seconds cannot be less than 0.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(this.SelectMinutesDuration);
            } else if (minutes == 0 && seconds < 5) {
                Error.instance.informMessageText.text = "Simulation duration should exeed 5 seconds.";
                Error.instance.informPanel.SetActive(true);
                Error.instance.informOkButton.onClick.AddListener(this.SelectMinutesDuration);
            } else {
                newDuration += seconds;
            }
        }
        if (duration != newDuration && duration != -1 && replaceExistingGraphs == false) {
            print("overwrite action registered");

            Error.instance.boolMessageText.text = "This action may overwrite data on already active graphs. Are you sure?";
            Error.instance.boolPanel.SetActive(true);
            Error.instance.boolRightButton.onClick.AddListener(ChangeActiveGraphDurations);
            Error.instance.boolLeftButton.onClick.AddListener(ResetDurationBack);
        } else {
            if (newDuration != 0) {
                duration = newDuration;
            }
            print("Setting duration to " + newDuration);
            // submitButton.interactable = false;
            replaceExistingGraphs = false;
        }

    }

    private void SelectMinutesDuration() {
        Error.instance.informPanel.SetActive(false);
        minutes.Select();
        minutes.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    private void SelectSecondsDuration() {
        Error.instance.informPanel.SetActive(false);
        seconds.Select();
        seconds.ActivateInputField();
        Error.instance.informOkButton.onClick.RemoveAllListeners();
    }

    void PopulateVitals() {
        foreach (Vital vital in ConditionSetup.instance.vitals.vitalList) {
            Transform child = vitals.transform.FindChild(vital.name);
            if (child != null) {
                child.gameObject.SetActive(true);
            } else {
                GameObject toggleObject = Instantiate(togglePrefab);
                toggleObject.transform.SetParent(vitals.transform);
                toggleObject.transform.localScale = Vector3.one;
                toggleObject.transform.localPosition = Vector3.zero;
                toggleObject.transform.GetChild(1).GetComponent<Text>().text = vital.name;
                Toggle toggle = toggleObject.GetComponent<Toggle>();
                toggle.name = vital.name;
                toggle.isOn = false;
                toggle.onValueChanged.AddListener((bool value) => loadChosenVital(value, toggleObject.transform.GetSiblingIndex(), vital.name));
            }
        }
    }

    public void ResetDurationBack() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        minutes.text = (duration / 60).ToString();
        seconds.text = (duration % 60).ToString();
    }

    void ChangeActiveGraphDurations() {
        Error.instance.boolPanel.SetActive(false);
        Error.instance.boolDropdown.gameObject.SetActive(false);
        Error.instance.boolLeftButton.onClick.RemoveAllListeners();
        Error.instance.boolRightButton.onClick.RemoveAllListeners();
        Error.instance.boolLeftButton.GetComponentInChildren<Text>().text = "no";

        replaceExistingGraphs = true;

        //if (!someBool) {
        //    SubmitDuration();
        //}

        for (int i = 0; i < vitals.transform.childCount; i++) {

            Toggle toggle = vitals.transform.GetChild(i).GetComponent<Toggle>();

            if (toggle.isOn) {

                string vitalName = toggle.gameObject.name;

                tabManager.GetComponent<TabManager>().tabGraphs.Remove(toggle);
                tabManager.GetComponent<ToggleGroup>().UnregisterToggle(toggle);

                Destroy(tabManager.transform.FindChild(vitalName).gameObject);

                GameObject graphObject = tabManager.transform.FindChild(vitalName).gameObject;
                Graph graph = tabManager.transform.FindChild(vitalName).GetComponent<Graph>();

                SortedList<float, Slider> sortedGraphPointsList = graph.sortedGraphPointsList;
                SortedList<float, Slider> pointsUpperThreshold = graph.pointsUpperThreshold;
                SortedList<float, Slider> pointsLowerThreshold = graph.pointsLowerThreshold;

                int vitalMin = graph.yStart;
                int vitalMax = graph.yEnd;

                string vitalUnits = graph.yAxisLabel.GetComponent<Text>().text;

                Destroy(graphObject);

                graph = tabManager.GenerateTab(vitalName).GetComponent<Graph>();
                graph.GenerateGraph(0, duration, vitalMin, vitalMax, vitalUnits);

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

            }
        }
    }

    public void loadChosenVital(bool chosen, int index, string vitalName) {
      //  int halfValue = 0;
        SubmitDuration();
        // clear the background area ready for display
        tabManager.activeTabs.transform.GetComponent<ToggleGroup>().SetAllTogglesOff();
        if (chosen) {
            // check to see if vital selected has already been previously created and is idle
            Transform vitalTrans = tabManager.transform.FindChild(vitalName);
            Transform vitalTab = tabManager.inactiveTabs.transform.FindChild(vitalName);
            // if vital isn't sitting idle
            if (vitalTrans == null || vitalTab == null) {
                // check this isn't the first attempt of choosing a vital before a duration has been even set
                if (duration == -1) {
                    // if it is then warn user to set a duration first before choosing a vital
                    Error.instance.informMessageText.text = "Please set the duration of the drugs effects before adding vitals.";
                    Error.instance.informPanel.SetActive(true);
                    Error.instance.informOkButton.onClick.AddListener(SelectMinutesDuration);
                    vitals.transform.GetChild(index).GetComponent<Toggle>().isOn = false;
                    return;
                }
                // check range values are acceptable
                double min = double.Parse(minimum.text);
                double max = double.Parse(maximum.text);

                if (minimum.text.Length == 0 || maximum.text.Length == 0) {
                    Error.instance.informMessageText.text = "Enter both range values.";
                    Error.instance.informPanel.SetActive(true);
                    Error.instance.informOkButton.onClick.AddListener(this.SelectMinutesDuration);
                    vitals.transform.GetChild(index).GetComponent<Toggle>().isOn = false;
                    return;
                } else {
                    if (min + 5 >= max) {
                        Error.instance.informMessageText.text = "Range must exceed 5 units.";
                        Error.instance.informPanel.SetActive(true);
                        Error.instance.informOkButton.onClick.AddListener(this.SelectMinutesDuration);
                        vitals.transform.GetChild(index).GetComponent<Toggle>().isOn = false;
                        return;
                    }
                    if (max - min > 10000) {
                        Error.instance.informMessageText.text = "Range must not exceed 10,000.";
                        Error.instance.informPanel.SetActive(true);
                        Error.instance.informOkButton.onClick.AddListener(this.SelectMinutesDuration);
                        vitals.transform.GetChild(index).GetComponent<Toggle>().isOn = false;
                        return;
                    }
                    if (max < 0 || min > 0 ) {
                        Error.instance.informMessageText.text = "The value 0 must be included within the range.";
                        Error.instance.informPanel.SetActive(true);
                        Error.instance.informOkButton.onClick.AddListener(this.SelectMinutesDuration);
                        vitals.transform.GetChild(index).GetComponent<Toggle>().isOn = false;
                        return;
                    }
                    // set the initial points to mid range values
                    //if (min < 0) {
                    //    print("min range < 0");
                    //    halfValue = (int)Math.Ceiling(((max - min) / 2) + min ) ;// + ConditionSetup.instance.vitals.vitalList[index].min);
                    //    print(halfValue);
                    //} else {
                    //    halfValue = (int)Math.Ceiling(((max - min) / 2) + ConditionSetup.instance.vitals.vitalList[index].min);
                    //}
                }
                // if it is the first attempt of choosing a vital and a duration has been set then initialise the vital graph with starting values
                tabManager.gameObject.SetActive(true);
                graph = tabManager.GenerateTab(ConditionSetup.instance.vitals.vitalList[index].name).GetComponent<Graph>();
                graph.GenerateGraph(0, duration, (int)Math.Ceiling(min), (int)Math.Ceiling(max), ConditionSetup.instance.vitals.vitalList[index].units);
                if (graph.sortedGraphPointsList.Count == 0) {
                    // change to "halfValue" variable to make mid ranges, otherwise initialise points with value 0
                    graph.AddPoint(0, 0, false);
                    graph.AddPoint(duration, 0);
                }
                graph.gameObject.SetActive(false);
                tabManager.SwitchTab();
            } else {
                // if vital was on standby then reactivate it as its toggle has been selected
                vitalTrans.gameObject.SetActive(true);
                vitalTab.SetParent(tabManager.transform);
                vitalTab.gameObject.SetActive(true);
                tabManager.SwitchTab();
            }
            if (ConditionSetup.instance.tabManager.activeTabs.transform.FindChild(vitalName) != null && tabManager.activeTabs.transform.FindChild(vitalName) != null) {
                visualisation.interactable = true;
            } else {
                print("didnt find " + vitalName + " in conditions active tabs");
                visualisation.interactable = false;
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

        // LerpFromView.onEnd();
    }

    public void LoadChosenAdministration(bool chosen, int index, string administrationName) {
        if (chosen) {
            for (int i = 0; i < administrations.transform.childCount; i++) {
                if (index != i) {
                    administrations.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
            for (int i = 0; i < vitals.transform.childCount; i++) {
                vitals.transform.GetChild(i).gameObject.SetActive(true);
            }
        } else {
            for (int i = 0; i < administrations.transform.childCount; i++) {
                administrations.transform.GetChild(i).gameObject.SetActive(true);
            }
            for (int i = 0; i < vitals.transform.childCount; i++) {
                vitals.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        ToggleElementsActive();
    }

    public void AddNewDrug() {

        if (drugName.text.Length == 0) {
            Error.instance.informMessageText.text = "Enter a name for the drug.";
            Error.instance.informOkButton.onClick.AddListener(Error.instance.DeactivateErrorInformPanel);
            Error.instance.informPanel.SetActive(true);
            return;
        }

        Drug drug = new Drug();
        List<Administration> administrations = new List<Administration>();

        for (int i = 0; i < tabManager.contentArea.transform.childCount; i++) {
            //  AdminSetup admin = tabManager.contentArea.transform.GetChild(i).GetComponent<AdminSetup>();
            Administration administration = GetAdministration();
            if (administration == null) {
                return;
            } else {
                administrations.Add(GetAdministration());
            }
        }
        drug.administrations = administrations;
        drug.name = drugName.text;
        ConditionSetup.instance.drugs.drugs.Add(drug);

        GameObject toggleObject = Instantiate(togglePrefab);
        toggleObject.transform.SetParent(ConditionSetup.instance.drugsChosen.transform);
        toggleObject.transform.localScale = Vector3.one;
        toggleObject.transform.localPosition = Vector3.zero;
        toggleObject.transform.GetChild(1).GetComponent<Text>().text = drug.name;
        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.name = drug.name;
        ConditionSetup.instance.ToggleActiveDrugWindow();
    }

    public Administration GetAdministration() {

        if (dose.text.Length == 0) {
            Error.instance.informMessageText.text = "Enter units for " + gameObject.name + ".";
            Error.instance.informOkButton.onClick.AddListener(Error.instance.DeactivateErrorInformPanel);
            Error.instance.informPanel.SetActive(true);
            return null;
        }

        if (duration == -1) {
            Error.instance.informMessageText.text = "Enter a duration for " + gameObject.name + ".";
            Error.instance.informOkButton.onClick.AddListener(Error.instance.DeactivateErrorInformPanel);
            Error.instance.informPanel.SetActive(true);
            return null;
        }

        Administration administration = new Administration();
        administration.duration = duration;

        VitalData vitalData;
        TimeLine timeLine;
        List<Value> values;
        Value value;

        for (int j = 0; j < vitals.transform.childCount; j++) {

            Toggle toggle = vitals.transform.GetChild(j).GetComponent<Toggle>();

            if (toggle.isOn) {

                string vitalName = toggle.gameObject.name;

                GameObject graphObject = tabManager.contentArea.transform.FindChild(vitalName).gameObject;
                Graph graph = tabManager.contentArea.transform.FindChild(vitalName).GetComponent<Graph>();

                timeLine = new TimeLine();

                values = new List<Value>();
                foreach (KeyValuePair<float, Slider> item in graph.sortedGraphPointsList) {
                    value = new Value();
                    value.second = item.Key;
                    value.value = item.Value.value;
                    values.Add(value);
                }
                timeLine.vitalValues = values;

                values = new List<Value>();
                foreach (KeyValuePair<float, Slider> item in graph.pointsUpperThreshold) {
                    value = new Value();
                    value.second = item.Key;
                    value.value = item.Value.value;
                    values.Add(value);
                }
                timeLine.upperThresholdValues = values;

                values = new List<Value>();
                foreach (KeyValuePair<float, Slider> item in graph.pointsLowerThreshold) {
                    value = new Value();
                    value.second = item.Key;
                    value.value = item.Value.value;
                    values.Add(value);
                }
                timeLine.lowerThresholdValues = values;

                vitalData = new VitalData();
                vitalData.vital = ConditionSetup.instance.GetVital(vitalName);
                vitalData.timeline = timeLine;

                administration.vitalsData.Add(vitalData);
            }
        }
        administration.duration = duration;
        administration.units = dose.text;
        administration.name = name;
        return administration;
    }

    public void ToggleActiveVisualizeWindow() {
        WindowManager.instance.drug.SetActive(!WindowManager.instance.drug.activeInHierarchy);
        WindowManager.instance.visualise.SetActive(!WindowManager.instance.visualise.activeInHierarchy);
        if (WindowManager.instance.visualise.activeInHierarchy) {
            VisualizationSetup.instance.SetVisualization();
        } else {
            VisualizationSetup.instance.ReturnGraphs();
        }
    }
}